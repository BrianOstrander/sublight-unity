using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Presenters;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class GamePayload : IStatePayload
	{
		public struct Waypoint
		{
			public readonly string WaypointId;
			public readonly WaypointModel Model;
			public readonly WaypointPresenter[] Presenters;

			public Waypoint(
				WaypointModel model,
				WaypointPresenter[] presenters
			)
			{
				WaypointId = model.WaypointId.Value;
				Model = model;
				Presenters = presenters;
			}
		}

		public int LocalSectorOffset = 1; // Not set anywhere else at the moment...
		public int LocalSectorOffsetTotal { get { return (LocalSectorOffset * 2) + 1; } }
		public int LocalSectorCount { get { return LocalSectorOffsetTotal * LocalSectorOffsetTotal; } }

		public GameModel Game;

		public HoloRoomFocusCameraPresenter MainCamera;
		public List<IPresenterCloseShowOptions> ShowOnIdle = new List<IPresenterCloseShowOptions>();

		public KeyValueListener KeyValueListener;

		public List<SectorInstanceModel> LocalSectorInstances = new List<SectorInstanceModel>();
		public UniversePosition LastLocalFocus = new UniversePosition(new Vector3Int(int.MinValue, 0, int.MinValue));
		public UniverseScales LastUniverseFocusToScale;

		public UniverseScaleModel LastActiveScale;

		public List<Waypoint> Waypoints = new List<Waypoint>();
	}

	public partial class GameState : State<GamePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Game, SceneConstants.HoloRoom }; } }

		#region Begin
		protected override void Begin()
		{
			SM.PushBlocking(LoadScenes, "LoadScenes");
			SM.PushBlocking(LoadModelDependencies, "LoadModelDependencies");
			SM.PushBlocking(SetNonSerializedValues, "SetNonSerializedValues");
			SM.PushBlocking(InitializeInput, "InitializeInput");
			SM.PushBlocking(InitializeCallbacks, "InitializeCallbacks");
			SM.PushBlocking(done => Focuses.InitializePresenters(this, done), "InitializePresenters");
			SM.PushBlocking(InitializeScaleTransforms, "InitializeScaleTransforms");
			SM.PushBlocking(InitializeFocus, "InitializeFocus");
			SM.PushBlocking(InitializeCelestialSystems, "InitializeCelestialSystems");
		}

		void LoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.Load(result => done(), Scenes));
		}

		void LoadModelDependencies(Action done)
		{
			if (string.IsNullOrEmpty(Payload.Game.GalaxyId))
			{
				Debug.LogError("No GalaxyId to load");
				done();
				return;
			}
			App.M.Load<GalaxyInfoModel>(Payload.Game.GalaxyId, result => OnLoadGalaxy(result, done));
		}

		void OnLoadGalaxy(SaveLoadRequest<GalaxyInfoModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load galaxy, resulted in " + result.Status + " and error: " + result.Error);
				done();
				return;
			}
			Payload.Game.Galaxy = result.TypedModel;

			if (string.IsNullOrEmpty(Payload.Game.GalaxyTargetId))
			{
				Debug.LogError("No GalaxyTargetId to load");
				done();
				return;
			}

			App.M.Load<GalaxyInfoModel>(Payload.Game.GalaxyTargetId, targetResult => OnLoadGalaxyTarget(targetResult, done));
		}

		void OnLoadGalaxyTarget(SaveLoadRequest<GalaxyInfoModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load galaxy target, resulted in " + result.Status + " and error: " + result.Error);
				done();
				return;
			}
			Payload.Game.GalaxyTarget = result.TypedModel;

			done();
		}

		void SetNonSerializedValues(Action done)
		{
			Payload.Game.Ship.Value.SetCurrentSystem(App.Universe.GetSystem(Payload.Game.Galaxy, Payload.Game.Universe, Payload.Game.Ship.Value.Position, Payload.Game.Ship.Value.SystemIndex));
			if (Payload.Game.Ship.Value.CurrentSystem.Value == null) Debug.LogError("Unable to load current system at "+Payload.Game.Ship.Value.Position.Value+" and index "+Payload.Game.Ship.Value.SystemIndex.Value);

			foreach (var waypoint in Payload.Game.WaypointCollection.Waypoints.Value)
			{
				switch (waypoint.WaypointId.Value)
				{
					case WaypointIds.Ship:
						waypoint.Name.Value = "Ark"; 
						break;
					case WaypointIds.BeginSystem:
						waypoint.Name.Value = "Origin";
						break;
					case WaypointIds.EndSystem:
						waypoint.Name.Value = "Sagittarius A*";
						break;
				}

				if (!waypoint.Location.Value.IsSystem) continue;

				var currWaypointSystem = App.Universe.GetSystem(Payload.Game.Galaxy, Payload.Game.Universe, waypoint.Location.Value.Position, waypoint.Location.Value.SystemIndex);
				if (currWaypointSystem == null)
				{
					Debug.LogError("Unable to load waypoint system ( WaypointId: "+waypoint.WaypointId.Value+" , Name: "+waypoint.Name.Value+" ) at\n" + waypoint.Location.Value.Position + " and index " + waypoint.Location.Value.SystemIndex);
					continue;
				}
				waypoint.SetLocation(currWaypointSystem);
			}

			done();
		}

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeCallbacks(Action done)
		{
			Payload.KeyValueListener = new KeyValueListener(KeyValueTargets.Game, Payload.Game.KeyValues, App.KeyValues).Register();

			App.Callbacks.DialogRequest += OnDialogRequest;
			App.Callbacks.EncounterRequest += OnEncounterRequest;
			App.Callbacks.SaveRequest += OnSaveRequest;

			Payload.Game.ToolbarSelectionRequest.Changed += OnToolbarSelectionRequest;
			Payload.Game.FocusTransform.Changed += OnFocusTransform;

			Payload.Game.WaypointCollection.Waypoints.Changed += OnWaypoints;
			Payload.Game.Ship.Value.Position.Changed += OnShipPosition;

			Payload.Game.CelestialSystemState.Changed += OnCelestialSystemState;

			Payload.Game.TransitState.Changed += OnTransitState;

			done();
		}

		void InitializeScaleTransforms(Action done)
		{
			foreach (var scaleTransformProperty in EnumExtensions.GetValues(UniverseScales.Unknown).Select(s => Payload.Game.GetScale(s).Transform))
			{
				scaleTransformProperty.Changed += OnScaleTransform;
			}
			done();
		}

		void InitializeFocus(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Default(Focuses.GetDefaultFocuses(), () => OnInializeFocusDefaults(done)));
		}

		void OnInializeFocusDefaults(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.GetNoFocus(), done));
		}

		void InitializeCelestialSystems(Action done)
		{
			Payload.Game.GetScale(UniverseScales.Local).Transform.Changed += OnCelestialSystemsTransform;
			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			App.Callbacks.HoloColorRequest(new HoloColorRequest(Color.white));
			App.Callbacks.CameraMaskRequest(CameraMaskRequest.Reveal(0.75f, OnIdleShowFocus));

			// HACK BEGIN - Probably bad to do and I should feel bad... but oh well...
			var activeScale = Payload.Game.ActiveScale.Value;
			activeScale.Opacity.Changed(1f);
			activeScale.Transform.Changed(activeScale.Transform.Value);
			// HACK END
		}

		void OnIdleShowFocus()
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetToolbarSelectionFocus(Payload.Game.ToolbarSelection), OnIdleShowFocusDone, 0.5f));
		}

		void OnIdleShowFocusDone()
		{
			foreach (var presenter in Payload.ShowOnIdle) presenter.Show();
			App.Heartbeat.Wait(OnPresentersShown, 0.21f);
		}

		void OnPresentersShown()
		{
			OnTransitComplete(Payload.Game.TransitState.Value);
		}
		#endregion

		#region End
		protected override void End()
		{
			Payload.KeyValueListener.UnRegister();

			App.Callbacks.DialogRequest -= OnDialogRequest;
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
			App.Callbacks.SaveRequest -= OnSaveRequest;

			Payload.Game.ToolbarSelectionRequest.Changed -= OnToolbarSelectionRequest;
			Payload.Game.FocusTransform.Changed -= OnFocusTransform;

			Payload.Game.WaypointCollection.Waypoints.Changed -= OnWaypoints;
			Payload.Game.Ship.Value.Position.Changed -= OnShipPosition;

			Payload.Game.CelestialSystemState.Changed -= OnCelestialSystemState;

			Payload.Game.TransitState.Changed -= OnTransitState;

			foreach (var scaleTransformProperty in EnumExtensions.GetValues(UniverseScales.Unknown).Select(s => Payload.Game.GetScale(s).Transform))
			{
				scaleTransformProperty.Changed -= OnScaleTransform;
			}

			Payload.Game.GetScale(UniverseScales.Local).Transform.Changed -= OnCelestialSystemsTransform;

			App.Input.SetEnabled(false);

			SM.PushBlocking(
				done => App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetNoFocus(), done)),
				"GameSettingNoFocus"
			);

			SM.PushBlocking(
				done => App.Callbacks.CameraMaskRequest(CameraMaskRequest.Hide(CameraMaskRequest.DefaultHideDuration, done)),
				"GameHideMask"
			);

			SM.PushBlocking(
				done => App.P.UnRegisterAll(done),
				"GameUnBind"
			);

			SM.PushBlocking(
				done => App.Scenes.Request(SceneRequest.UnLoad(result => done(), Scenes)),
				"GameUnLoadScenes"
			);
		}
		#endregion

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
			if (request.OverrideFocusHandling) return;
			switch (request.State)
			{
				case DialogRequest.States.Request:
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetPriorityFocus(Payload.Game.ToolbarSelection.Value)));
					break;
				case DialogRequest.States.Completing:
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetToolbarSelectionFocus(Payload.Game.ToolbarSelection.Value)));
					break;
			}
		}

		void OnToolbarSelectionRequest(ToolbarSelectionRequest request)
		{
			Payload.Game.ToolbarLocking.Value = request.Locked;

			if (request.Selection == ToolbarSelections.Unknown || request.Selection == Payload.Game.ToolbarSelection.Value)
			{
				if (request.Done != null) request.Done();
				return;
			}

			if (request.Instant)
			{
				App.Callbacks.SetFocusRequest(
					SetFocusRequest.RequestInstant(
						Focuses.GetToolbarSelectionFocus(request.Selection),
						() => OnToolbarSelectionRequestDone(request)
					)
				);
			}
			else
			{
				App.Callbacks.SetFocusRequest(
					SetFocusRequest.Request(
						Focuses.GetToolbarSelectionFocus(request.Selection),
						() => OnToolbarSelectionRequestDone(request)
					)
				);
			}
		}

		void OnToolbarSelectionRequestDone(ToolbarSelectionRequest request)
		{
			Payload.Game.ToolbarSelection.Value = request.Selection;
			if (request.Done != null) request.Done();
		}

		void OnCelestialSystemsTransform(UniverseTransform transform)
		{
			if (transform.UniverseOrigin.SectorEquals(Payload.LastLocalFocus) || Payload.Game.FocusTransform.Value.Zoom.State != TweenStates.Complete)
			{
				Payload.LastLocalFocus = transform.UniverseOrigin;
				return;
			}
			Payload.LastLocalFocus = transform.UniverseOrigin;

			OnCalculateLocalSectors(transform, false);
		}

		void OnFocusTransform(FocusTransform focusTransform)
		{
			if (focusTransform.ToScale == Payload.LastUniverseFocusToScale) return;
			Payload.LastUniverseFocusToScale = focusTransform.ToScale;

 			switch (focusTransform.ToScale)
			{
				case UniverseScales.Local:
					OnCalculateLocalSectors(Payload.Game.GetScale(focusTransform.ToScale).TransformDefault.Value, true);
					break;
			}
		}

		/// <summary>
		/// Calculates the visible sectors given the specified transform.
		/// </summary>
		/// <param name="transform">Transform.</param>
		/// <param name="force">If set to <c>true</c> force all sectors to be set stale.</param>
		void OnCalculateLocalSectors(UniverseTransform transform, bool force)
		{
			if (Payload.LocalSectorInstances.None()) return;

			var originInt = transform.UniverseOrigin.SectorInteger;
			var minSector = new Vector3Int(originInt.x - Payload.LocalSectorOffset, 0, originInt.z - Payload.LocalSectorOffset);
			var maxSector = new Vector3Int(originInt.x + Payload.LocalSectorOffset, 0, originInt.z + Payload.LocalSectorOffset);

			var staleSectors = new List<SectorInstanceModel>();

			var sectorPositionExists = new bool[Payload.LocalSectorOffsetTotal, Payload.LocalSectorOffsetTotal];

			foreach (var sector in Payload.LocalSectorInstances)
			{
				if (force)
				{
					staleSectors.Add(sector);
					continue;
				}

				var currSectorInt = sector.Sector.Value.Position.Value.SectorInteger;
				if (currSectorInt.x < minSector.x || maxSector.x < currSectorInt.x || currSectorInt.z < minSector.z || maxSector.z < currSectorInt.z)
				{
					// out of range
					staleSectors.Add(sector);
				}
				else sectorPositionExists[currSectorInt.x - minSector.x, currSectorInt.z - minSector.z] = true;
			}

			for (var x = 0; x < Payload.LocalSectorOffsetTotal; x++)
			{
				for (var z = 0; z < Payload.LocalSectorOffsetTotal; z++)
				{
					if (!force && sectorPositionExists[x, z]) continue;
					var replacement = staleSectors.First();
					staleSectors.RemoveAt(0);
					OnCalculateLocalSectorsUpdateSystems(replacement, new UniversePosition(new Vector3Int(x + minSector.x, 0, z + minSector.z)));
				}
			}
		}

		void OnCalculateLocalSectorsUpdateSystems(SectorInstanceModel sectorModel, UniversePosition sectorPosition)
		{
			sectorModel.Sector.Value = App.Universe.GetSector(Payload.Game.Galaxy, Payload.Game.Universe, sectorPosition);
			var systemCount = sectorModel.Sector.Value.SystemCount.Value;
			for (var i = 0; i < sectorModel.SystemModels.Value.Length; i++)
			{
				var currSystem = sectorModel.SystemModels.Value[i];
				if (i < systemCount)
				{
					// is active
					currSystem.SetSystem(App.Universe.GetSystem(Payload.Game.Galaxy, Payload.Game.Universe, sectorModel.Sector, i));
				}
				else currSystem.SetSystem(null);
			}
		}

		void OnScaleTransform(UniverseTransform transform)
		{
			var labels = Payload.Game.Galaxy.GetLabels();
			var targetProperty = Payload.Game.ScaleLabelSystem;

			switch (transform.Scale)
			{
				case UniverseScales.System:
					Payload.Game.ScaleLabelSystem.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("System name here..."));
					return;
				case UniverseScales.Local:
					labels = Payload.Game.Galaxy.GetLabels(UniverseScales.Quadrant);
					targetProperty = Payload.Game.ScaleLabelLocal;
					break;
				case UniverseScales.Stellar:
					labels = Payload.Game.Galaxy.GetLabels(UniverseScales.Quadrant);
					targetProperty = Payload.Game.ScaleLabelStellar;
					break;
				case UniverseScales.Quadrant:
					labels = Payload.Game.Galaxy.GetLabels(UniverseScales.Galactic);
					targetProperty = Payload.Game.ScaleLabelQuadrant;
					break;
				case UniverseScales.Galactic:
					Payload.Game.ScaleLabelGalactic.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("Milky Way"));
					return;
				case UniverseScales.Cluster:
					Payload.Game.ScaleLabelCluster.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("Local Group"));
					return;
			}

			var normalizedPosition = UniversePosition.NormalizedSector(transform.UniverseOrigin, Payload.Game.Galaxy.GalaxySize);

			float? proximity = null;
			GalaxyLabelModel closestLabel = null;

			foreach (var label in labels)
			{
				var currProximity = label.Proximity(normalizedPosition, 4);
				if (!proximity.HasValue || currProximity < proximity)
				{
					proximity = currProximity;
					closestLabel = label;
				}
			}

			if (closestLabel == null)
			{
				Debug.LogError("No labels were found for current scale");
				targetProperty.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("No labels provided"));
				return;
			}

			// TODO: Add language support for this.
			targetProperty.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override(closestLabel.Name.Value));
		}

		void OnWaypoints(WaypointModel[] waypoints)
		{
			Debug.LogError("Waypoint binding not done yet!");
		}

		void OnShipPosition(UniversePosition position)
		{
			foreach (var waypoint in Payload.Game.WaypointCollection.Waypoints.Value)
			{
				switch (waypoint.WaypointId.Value)
				{
					case WaypointIds.Ship:
						waypoint.SetLocation(position);
						break;
				}
				waypoint.Distance.Value = UniversePosition.Distance(position, waypoint.Location.Value.Position);
			}
		}

		void OnTransitState(TransitState transitState)
		{
			switch(transitState.State)
			{
				case TransitState.States.Complete: OnTransitComplete(transitState); break;
			}
		}

		void OnTransitComplete(TransitState transitState)
		{
			var synchronizedId = SM.UniqueSynchronizedId;

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.SetDefined(
							DefinedKeyInstances.Game.DistanceFromBegin,
							Payload.Game.WaypointCollection.Waypoints.Value.First(w => w.WaypointId == WaypointIds.BeginSystem).Distance.Value,
							setDone => doneBlocking()
						)
					);
				},
				"UpdateDistanceFromBegin",
				synchronizedId
			);

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.SetDefined(
							DefinedKeyInstances.Game.DistanceToEnd,
							Payload.Game.WaypointCollection.Waypoints.Value.First(w => w.WaypointId == WaypointIds.EndSystem).Distance.Value,
							setDone => doneBlocking()
						)
					);
				},
				"UpdateDistanceFromEnd",
				synchronizedId
			);

			var transitDistance = UniversePosition.Distance(transitState.BeginSystem.Position.Value, transitState.EndSystem.Position.Value);

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.GetDefined(
							DefinedKeyInstances.Game.DistanceTraveled,
							distanceTraveled =>
							{
								App.Callbacks.KeyValueRequest(
									KeyValueRequest.SetDefined(
										DefinedKeyInstances.Game.DistanceTraveled,
										distanceTraveled.Value + transitDistance,
										setDone => doneBlocking()
									)
								);
							}
						)
					);
				},
				"UpdateDistanceTraveled",
				synchronizedId
			);

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.GetDefined(
							DefinedKeyInstances.Game.FurthestTransit,
							furthestTransit =>
							{
								if (furthestTransit.Value < transitDistance)
								{
									App.Callbacks.KeyValueRequest(
										KeyValueRequest.SetDefined(
											DefinedKeyInstances.Game.FurthestTransit,
											transitDistance,
											setDone => doneBlocking()
										)
									);
								}
								else doneBlocking();
							}
						)
					);
				},
				"UpdateFurthestTransit",
				synchronizedId
			);

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.SetDefined(
							DefinedKeyInstances.Game.YearsElapsedGalactic,
							Payload.Game.RelativeDayTime.Value.GalacticTime.TotalYears,
							setDone => doneBlocking()
						)
					);
				},
				"UpdateYearsElapsedGalactic",
				synchronizedId
			);

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.SetDefined(
							DefinedKeyInstances.Game.YearsElapsedShip,
							Payload.Game.RelativeDayTime.Value.ShipTime.TotalYears,
							setDone => doneBlocking()
						)
					);
				},
				"UpdateYearsElapsedShip",
				synchronizedId
			);

			SM.PushBlocking(
				doneBlocking =>
				{
					App.Callbacks.KeyValueRequest(
						KeyValueRequest.SetDefined(
							DefinedKeyInstances.Game.YearsElapsedDelta,
							(Payload.Game.RelativeDayTime.Value.GalacticTime - Payload.Game.RelativeDayTime.Value.ShipTime).TotalYears,
							setDone => doneBlocking()
						)
					);
				},
				"UpdateYearsElapsedDelta",
				synchronizedId
			);

			SM.Push(OnTransitCompleteCheckForEncounters, "TransitCompleteCheckForEncounters");
		}

		void OnTransitCompleteCheckForEncounters()
		{
			var encounterId = Payload.Game.Ship.Value.CurrentSystem.Value.SpecifiedEncounterId.Value;

			if (!OnCheckSpecifiedEncounter(encounterId, EncounterTriggers.TransitComplete))
			{
				Debug.Log("Check for non-specified encounters here!");
				return;
			}
		}

		/// <summary>
		/// This should be called whenever a trigger happens, even if the
		/// encounterId is null or empty.
		/// </summary>
		/// <returns><c>true</c>, if an encounter was triggered, <c>false</c> otherwise.</returns>
		/// <param name="encounterId">Encounter identifier.</param>
		/// <param name="trigger">Trigger.</param>
		bool OnCheckSpecifiedEncounter(string encounterId, EncounterTriggers trigger)
		{
			if (DevPrefs.EncounterIdOverrideActive && trigger == DevPrefs.EncounterIdOverrideTrigger.Value)
			{
				var encounterOverride = App.Encounters.GetEncounter(DevPrefs.EncounterIdOverride.Value);
				if (encounterOverride == null) Debug.LogError("Unable to find specified override encounter: " + DevPrefs.EncounterIdOverride.Value + ", falling through to non-override encounters.");
				else
				{
					App.ValueFilter.Filter(
						valid =>
						{
							if (valid) Debug.Log("Override Encounter is valid, triggering");
							else Debug.LogWarning("Override Encounter is not valid, triggering anyways");
							OnTransitCompleteFiltered(true, encounterOverride);
						},
						encounterOverride.Filtering,
						Payload.Game,
						encounterOverride
					);
					return true;
				}
			}

			if (string.IsNullOrEmpty(encounterId)) return false;
			if (trigger == EncounterTriggers.Unknown)
			{
				Debug.LogError("Specifying a trigger of type " + EncounterTriggers.Unknown + " is not supported");
				return false;
			}

			var encounter = App.Encounters.GetEncounter(encounterId);

			if (encounter == null)
			{
				Debug.LogError("Unable to find specified encounter: " + encounterId);
				return false;
			}

			if (encounter.Trigger.Value != trigger) return false;

			switch (encounter.Trigger.Value)
			{
				case EncounterTriggers.NavigationSelect:
				case EncounterTriggers.TransitComplete:
					App.ValueFilter.Filter(
						valid => OnTransitCompleteFiltered(valid, encounter),
						encounter.Filtering,
						Payload.Game,
						encounter
					);
					return true;
				default:
					Debug.LogError("Unrecognized encounter trigger: " + encounter.Trigger.Value);
					return false;
			}
		}

		void OnTransitCompleteFiltered(bool valid, EncounterInfoModel encounter)
		{
			if (!valid) return;

			App.Callbacks.EncounterRequest(
				EncounterRequest.Request(
					Payload.Game,
					encounter
				)
			);
		}

		void OnEncounterRequest(EncounterRequest request)
		{
			switch (request.State)
			{
				case EncounterRequest.States.Request:
					break;
				case EncounterRequest.States.Handle:
					if (request.TryHandle<EncounterEventHandlerModel>(handler => Encounter.OnHandleEvent(Payload, handler))) break;
					// The list below is used mainly for when you're making a new log type and need to catch unhandled ones.
					// Any in the switch below should be handled by an existing presenter, or something...
					switch (request.LogType)
					{
						case EncounterLogTypes.Dialog: break;
						case EncounterLogTypes.Bust: break;
						case EncounterLogTypes.Button: break;
						case EncounterLogTypes.Conversation: break;
						default:
							Debug.LogError("Unrecognized EncounterRequest Handle model type: " + request.LogType);
							break;
					}
					break;
				case EncounterRequest.States.Controls:
					// I don't think there's anything else I need to do here...
					if (request.PrepareCompleteControl)
					{
						App.Callbacks.EncounterRequest(EncounterRequest.PrepareComplete(Payload.Game.EncounterState.Current.Value.EncounterId));
					}
					else if (request.NextControl)
					{
						App.Callbacks.EncounterRequest(EncounterRequest.Next());
					}
					else Debug.LogError("Unexpected behaviour: Done and Next are both false");
					break;
				case EncounterRequest.States.Next:
					// I don't think I need to do anything here... maybe cleanup messages and stuff?
					break;
				case EncounterRequest.States.PrepareComplete:
					// I don't think I need to do anything here...
					break;
				case EncounterRequest.States.Complete:
					// Unlocking the toolbar incase it was locked during the encounter.
					Payload.Game.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(
						Payload.Game.ToolbarSelection.Value,
						false,
						ToolbarSelectionRequest.Sources.Encounter
					);
					break;
				default:
					Debug.LogError("Unrecognized EncounterRequest State: " + request.State);
					break;
			}
		}

		void OnSaveRequest(SaveRequest request)
		{
			switch (request.State)
			{
				case SaveRequest.States.Request:
					App.M.Save(Payload.Game, result => OnSaveDone(result, request));
					break;
				case SaveRequest.States.Complete:
					if (request.Done != null) request.Done(request);
					break;
				default:
					Debug.LogError("Unrecognized SaveRequest state " + request.State);
					break;
			}
		}

		void OnSaveDone(SaveLoadRequest<GameModel> result, SaveRequest request)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Save game returned " + result.Status + " with error: " + result.Error);
				return;
			}

			App.Callbacks.SaveRequest(SaveRequest.Success(request));
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			if (block.System == null || block.State != CelestialSystemStateBlock.States.Selected) return;

			OnCheckSpecifiedEncounter(block.System.SpecifiedEncounterId.Value, EncounterTriggers.NavigationSelect);
		}
		#endregion
	}
}