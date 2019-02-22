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
		// TODO: Do we need this???
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

		public List<SectorInstanceModel> LocalSectorInstances = new List<SectorInstanceModel>();
		public UniversePosition LastLocalFocus = new UniversePosition(new Vector3Int(int.MinValue, 0, int.MinValue));
		public UniverseScales LastUniverseFocusToScale;

		public UniverseScaleModel LastActiveScale;

		// TODO: Why does this exist???
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

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeCallbacks(Action done)
		{
			Payload.Game.Context.KeyValueListener = new KeyValueListener(KeyValueTargets.Game, Payload.Game.KeyValues, App.KeyValues).Register();

			App.Callbacks.DialogRequest += OnDialogRequest;
			App.Callbacks.EncounterRequest += OnEncounterRequest;
			App.Callbacks.SaveRequest += OnSaveRequest;
			App.Callbacks.KeyValueRequest += OnKeyValueRequest;

			App.Heartbeat.Update += OnUpdate;

			Payload.Game.Context.ToolbarSelectionRequest.Changed += OnToolbarSelectionRequest;
			Payload.Game.Context.FocusTransform.Changed += OnFocusTransform;

			Payload.Game.Waypoints.Waypoints.Changed += OnWaypoints;
			Payload.Game.Ship.Position.Changed += OnShipPosition;

			Payload.Game.Context.CelestialSystemState.Changed += OnCelestialSystemState;

			Payload.Game.Context.TransitState.Changed += OnTransitState;

			done();
		}

		void InitializeScaleTransforms(Action done)
		{
			foreach (var scaleTransformProperty in EnumExtensions.GetValues(UniverseScales.Unknown).Select(s => Payload.Game.Context.GetScale(s).Transform))
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
			Payload.Game.Context.GetScale(UniverseScales.Local).Transform.Changed += OnCelestialSystemsTransform;
			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			App.Callbacks.HoloColorRequest(new HoloColorRequest(Color.white));

			// HACK BEGIN - Probably bad to do and I should feel bad... but oh well...
			var activeScale = Payload.Game.Context.ActiveScale.Value;
			activeScale.Opacity.Changed(1f);
			activeScale.Transform.Changed(activeScale.Transform.Value);
			// HACK END

			PushEncounterTriggers("IdleEncounterRules", EncounterTriggers.InitializeRules);
			App.Heartbeat.Wait(
				() => App.Callbacks.CameraMaskRequest(CameraMaskRequest.Reveal(0.75f, OnIdleShowFocus)),
				0.1f
			);
		}

		void OnIdleShowFocus()
		{
			App.Callbacks.SetFocusRequest(
				SetFocusRequest.Request(
					Focuses.GetToolbarSelectionFocus(
						Payload.Game.ToolbarSelection
					),
					OnIdleShowFocusDone,
					0.5f
				)
			);
		}

		void OnIdleShowFocusDone()
		{
			foreach (var presenter in Payload.ShowOnIdle) presenter.Show();
			App.Heartbeat.Wait(
				OnPresentersShown,
				0.21f
			);
		}

		void OnPresentersShown()
		{
			SM.Push(OnUpdateKeyValues, "InitialUpdateKeyValues");

			var triggers = new List<EncounterTriggers>(Payload.Game.EncounterTriggers.Value);

			if (Payload.Game.EncounterResume.Value.CanResume && Payload.Game.EncounterResume.Value.Trigger != EncounterTriggers.InitializeRules)
			{
				triggers.Insert(0, Payload.Game.EncounterResume.Value.Trigger);
			}
			if (!triggers.Contains(EncounterTriggers.Load)) triggers.Insert(0, EncounterTriggers.Load);

			SM.Push(
				() =>
				{
					PushEncounterTriggers(
						"IdleEncounterCheck",
						triggers.ToArray()
					);
				},
				"IdleEncounterResumeCheck"
			);
		}
		#endregion

		#region End
		protected override void End()
		{
			Payload.Game.Context.KeyValueListener.UnRegister();

			App.Callbacks.DialogRequest -= OnDialogRequest;
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
			App.Callbacks.SaveRequest -= OnSaveRequest;
			App.Callbacks.KeyValueRequest -= OnKeyValueRequest;

			App.Heartbeat.Update -= OnUpdate;

			Payload.Game.Context.ToolbarSelectionRequest.Changed -= OnToolbarSelectionRequest;
			Payload.Game.Context.FocusTransform.Changed -= OnFocusTransform;

			Payload.Game.Waypoints.Waypoints.Changed -= OnWaypoints;
			Payload.Game.Ship.Position.Changed -= OnShipPosition;

			Payload.Game.Context.CelestialSystemState.Changed -= OnCelestialSystemState;

			Payload.Game.Context.TransitState.Changed -= OnTransitState;

			foreach (var scaleTransformProperty in EnumExtensions.GetValues(UniverseScales.Unknown).Select(s => Payload.Game.Context.GetScale(s).Transform))
			{
				scaleTransformProperty.Changed -= OnScaleTransform;
			}

			Payload.Game.Context.GetScale(UniverseScales.Local).Transform.Changed -= OnCelestialSystemsTransform;

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
		void OnUpdate(float delta)
		{
			if (Payload.Game.Context.ElapsedTimeBlockers.None) Payload.Game.ElapsedTime.Value += TimeSpan.FromSeconds(delta);
		}

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
			if (transform.UniverseOrigin.SectorEquals(Payload.LastLocalFocus) || Payload.Game.Context.FocusTransform.Value.Zoom.State != TweenStates.Complete)
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
					OnCalculateLocalSectors(Payload.Game.Context.GetScale(focusTransform.ToScale).TransformDefault.Value, true);
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
			sectorModel.Sector.Value = App.Universe.GetSector(Payload.Game.Context.Galaxy, Payload.Game.Universe, sectorPosition);
			var systemCount = sectorModel.Sector.Value.SystemCount.Value;
			for (var i = 0; i < sectorModel.SystemModels.Value.Length; i++)
			{
				var currSystem = sectorModel.SystemModels.Value[i];
				if (i < systemCount)
				{
					// is active
					currSystem.SetSystem(App.Universe.GetSystem(Payload.Game.Context.Galaxy, Payload.Game.Universe, sectorModel.Sector, i));
				}
				else currSystem.SetSystem(null);
			}
		}

		void OnScaleTransform(UniverseTransform transform)
		{
			var labels = Payload.Game.Context.Galaxy.GetLabels();
			var targetProperty = Payload.Game.Context.ScaleLabelSystem;

			switch (transform.Scale)
			{
				case UniverseScales.System:
					Payload.Game.Context.ScaleLabelSystem.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("System name here..."));
					return;
				case UniverseScales.Local:
					labels = Payload.Game.Context.Galaxy.GetLabels(UniverseScales.Quadrant);
					targetProperty = Payload.Game.Context.ScaleLabelLocal;
					break;
				case UniverseScales.Stellar:
					labels = Payload.Game.Context.Galaxy.GetLabels(UniverseScales.Quadrant);
					targetProperty = Payload.Game.Context.ScaleLabelStellar;
					break;
				case UniverseScales.Quadrant:
					labels = Payload.Game.Context.Galaxy.GetLabels(UniverseScales.Galactic);
					targetProperty = Payload.Game.Context.ScaleLabelQuadrant;
					break;
				case UniverseScales.Galactic:
					Payload.Game.Context.ScaleLabelGalactic.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("Milky Way"));
					return;
				case UniverseScales.Cluster:
					Payload.Game.Context.ScaleLabelCluster.Value = UniverseScaleLabelBlock.Create(LanguageStringModel.Override("Local Group"));
					return;
			}

			var normalizedPosition = UniversePosition.NormalizedSector(transform.UniverseOrigin, Payload.Game.Context.Galaxy.GalaxySize);

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
			foreach (var waypoint in Payload.Game.Waypoints.Waypoints.Value)
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
			switch (transitState.State)
			{
				case TransitState.States.Complete: OnTransitComplete(transitState); break;
			}
		}

		void OnTransitComplete(TransitState transitState)
		{
			Payload.Game.TransitHistory.Push(
				TransitHistoryEntry.Create(
					DateTime.Now,
					Payload.Game.ElapsedTime.Value,
					Payload.Game.RelativeDayTime.Value,
					transitState.BeginSystem,
					transitState.EndSystem,
					Payload.Game.TransitHistory.Peek()
				)
			);

			SM.Push(OnUpdateGameplay, "UpdateGameplay");
			SM.Push(OnUpdateKeyValues, "UpdateKeyValues");

			PushEncounterTriggers(
				"TransitCompleteCheckForEncounters",
				EncounterTriggers.TransitComplete,
				EncounterTriggers.ResourceRequest,
				EncounterTriggers.ResourceConsume,
				EncounterTriggers.SystemIdle
			);
		}

		void PushEncounterTriggers(string description, params EncounterTriggers[] triggers)
		{
			Payload.Game.EncounterTriggers.Value = triggers;

			SM.Push(OnCheckForEncounters, description);
		}

		void OnUpdateGameplay()
		{
			// This should probably be run after every transit...
			GameplayUtility.ApplyTransit(
				Payload.Game.Context.TransitState.Value.RelativeTimeTotal.ShipTime.TotalYears,
				Payload.Game.KeyValues,
				Payload.Game.Context.TransitState.Value.BeginSystem.KeyValues
			);
		}

		void OnUpdateKeyValues()
		{
			Payload.Game.KeyValues.Set(
				KeyDefines.Game.DistanceFromBegin,
				Payload.Game.Waypoints.Waypoints.Value.First(w => w.WaypointId == WaypointIds.BeginSystem).Distance.Value
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.DistanceToEnd,
				Payload.Game.Waypoints.Waypoints.Value.First(w => w.WaypointId == WaypointIds.EndSystem).Distance.Value
			);

			var transitDistance = UniversePosition.Distance(
				Payload.Game.Context.TransitState.Value.BeginSystem.Position.Value,
				Payload.Game.Context.TransitState.Value.EndSystem.Position.Value
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.DistanceTraveled,
				Payload.Game.KeyValues.Get(KeyDefines.Game.DistanceTraveled) + transitDistance
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.FurthestTransit,
				Mathf.Max(Payload.Game.KeyValues.Get(KeyDefines.Game.FurthestTransit), transitDistance)
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.YearsElapsedGalactic,
				Payload.Game.RelativeDayTime.Value.GalacticTime.TotalYears
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.YearsElapsedShip,
				Payload.Game.RelativeDayTime.Value.ShipTime.TotalYears
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.YearsElapsedDelta,
				(Payload.Game.RelativeDayTime.Value.GalacticTime - Payload.Game.RelativeDayTime.Value.ShipTime).TotalYears
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.PreviousTransitYearsElapsedShip,
				Payload.Game.Context.TransitState.Value.RelativeTimeTotal.ShipTime.TotalYears
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.TransitsWithoutRationsUntilFailure,
				Payload.Game.KeyValues.Get(KeyDefines.Game.TransitsWithoutRationsMaximum) - Payload.Game.KeyValues.Get(KeyDefines.Game.TransitsWithoutRations)
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.TransitsWithOverPopulationUntilFailure,
				Payload.Game.KeyValues.Get(KeyDefines.Game.TransitsWithOverPopulationMaximum) - Payload.Game.KeyValues.Get(KeyDefines.Game.TransitsWithOverPopulation)
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.TransitsWithUnderPopulationUntilFailure,
				Payload.Game.KeyValues.Get(KeyDefines.Game.TransitsWithUnderPopulationMaximum) - Payload.Game.KeyValues.Get(KeyDefines.Game.TransitsWithUnderPopulation)
			);

			Payload.Game.KeyValues.Set(
				KeyDefines.Game.PopulationMaximum,
				Payload.Game.KeyValues.Get(KeyDefines.Game.ShipPopulationMaximum) * Payload.Game.KeyValues.Get(KeyDefines.Game.PopulationMaximumMultiplier)
			);

			Debug.Log("calculate propellant usage");
			//Payload.Game.Ship.SetVelocityMultiplierCurrent(Payload.Game.KeyValues.Get(KeyDefines.Game.PropellantUsage));
			//Payload.Game.Ship.SetVelocityMultiplierEnabledMaximum(Payload.Game.KeyValues.Get(KeyDefines.Game.Propellant));
		}

		void OnCheckForEncounters()
		{
			var target = EncounterTriggers.Unknown;
			var remaining = new List<EncounterTriggers>();

			foreach (var trigger in Payload.Game.EncounterTriggers.Value)
			{
				if (trigger == EncounterTriggers.Unknown)
				{
					Debug.LogError("Unknown trigger on stack, this should not happen, ignoring");
					continue;
				}

				if (target == EncounterTriggers.Unknown)
				{
					target = trigger;
					continue;
				}
				remaining.Add(trigger);
			}

			Payload.Game.EncounterTriggers.Value = remaining.ToArray();

			if (target == EncounterTriggers.Unknown) return;

			var encounterId = string.Empty;

			if (Payload.Game.EncounterResume.Value.CanResume && Payload.Game.EncounterResume.Value.Trigger == target)
			{
				// Resuming an encounter we were in the middle of...
				encounterId = Payload.Game.EncounterResume.Value.EncounterId;
			}
			else
			{
				// Checking if the system has any matching encounter triggers...
				var allSpecifiedEncounters = Payload.Game.Context.CurrentSystem.Value.SpecifiedEncounters.Value.Where(s => !string.IsNullOrEmpty(s.EncounterId) && s.Trigger == target);

				if (1 < allSpecifiedEncounters.Count())
				{
					Debug.LogError("Multiple encounters are specified for system " + Payload.Game.Context.CurrentSystem.Name + " with trigger " + target + ", this behaviour is not supported yet, choosing the first one instead.");
				}

				encounterId = allSpecifiedEncounters.FirstOrDefault().EncounterId;
			}

			OnCheckSpecifiedEncounter(encounterId, target, true, run => OnCheckForEncountersSpecified(run, target));
		}

		void OnCheckForEncountersSpecified(bool run, EncounterTriggers trigger)
		{
			if (run) return; // Our override was accepted, any triggers on stack will get checked on encounter completion...

			App.Encounters.GetNextEncounter(
				OnCheckForEncountersNext,
				trigger,
				Payload.Game
			);
		}

		void OnCheckForEncountersNext(RequestResult result, EncounterInfoModel model)
		{
			if (result.IsNotSuccess)
			{
				Debug.LogError("Filtering next encounter returning with status " + result.Status + ", error: " + result.Message);
				SM.Push(OnCheckForEncounters, "FilterErrorCheckForNextTrigger");
				return;
			}
			if (model == null)
			{
				SM.Push(OnCheckForEncounters, "NoEncounterForTriggerCheckForNextTrigger");
				return;
			}

			OnCheckSpecifiedEncounter(
				model,
				model.Trigger.Value,
				false,
				run =>
				{
					if (!run) SM.Push(OnCheckForEncounters, "NoEncounterTriggerCheckForNextTrigger");
				}
			);
		}

		/// <summary>
		/// This should be called whenever a trigger happens, even if the
		/// encounterId is null or empty.
		/// </summary>
		/// <returns><c>true</c>, if an encounter was triggered, <c>false</c> otherwise.</returns>
		/// <param name="encounterId">Encounter identifier.</param>
		/// <param name="trigger">Trigger.</param>
		void OnCheckSpecifiedEncounter(
			string encounterId,
			EncounterTriggers trigger,
			bool filter,
			Action<bool> done = null
		)
		{
			var encounter = App.Encounters.GetEncounter(encounterId);

			if (!string.IsNullOrEmpty(encounterId) && encounter == null)
			{
				Debug.LogError("Unable to find specified encounter: " + encounterId+", checking for other encounters anyways");
			}

			OnCheckSpecifiedEncounter(encounter, trigger, filter, done);
		}

		/// <summary>
		/// This should be called whenever a trigger happens, even if the
		/// encounterId is null or empty.
		/// </summary>
		/// <returns><c>true</c>, if an encounter was triggered, <c>false</c> otherwise.</returns>
		/// <param name="encounter">Encounter.</param>
		/// <param name="trigger">Trigger.</param>
		void OnCheckSpecifiedEncounter(
			EncounterInfoModel encounter,
			EncounterTriggers trigger,
			bool filter,
			Action<bool> done = null
		)
		{
			done = done ?? ActionExtensions.GetEmpty<bool>();

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
							OnEncounterFiltered(true, encounterOverride, trigger);
						},
						encounterOverride.Filtering,
						Payload.Game,
						encounterOverride
					);
					done(true);
					return;
				}
			}

			if (trigger == EncounterTriggers.Unknown)
			{
				Debug.LogError("Specifying a trigger of type " + EncounterTriggers.Unknown + " is not supported");
				done(false);
				return;
			}

			if (encounter == null)
			{
				done(false);
				return;
			}
			if (encounter.Trigger.Value != trigger)
			{
				//Debug.LogError("Trigger was specified as " + trigger + " but the encounter's trigger was " + encounter.Trigger.Value + ", this probably should not ever happen. Ignoring Encounter.");
				done(false);
				return;
			}

			if (filter)
			{
				App.ValueFilter.Filter(
					valid =>
					{
						OnEncounterFiltered(valid, encounter, trigger);
						done(valid);
					},
					encounter.Filtering,
					Payload.Game,
					encounter
				);
				return;
			}

			OnEncounterFiltered(true, encounter, trigger);
			done(true);
		}

		void OnEncounterFiltered(bool valid, EncounterInfoModel encounter, EncounterTriggers trigger)
		{
			if (!valid) return;

			Payload.Game.EncounterResume.Value = new EncounterResume(encounter.EncounterId.Value, trigger);

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
						App.Callbacks.EncounterRequest(EncounterRequest.PrepareComplete(Payload.Game.Context.EncounterState.Current.Value.EncounterId));
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
					Payload.Game.Context.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(
						Payload.Game.ToolbarSelection.Value,
						false,
						ToolbarSelectionRequest.Sources.Encounter
					);
					// Clear the EncounterResume so it doesn't play again when we open the game.
					Payload.Game.EncounterResume.Value = EncounterResume.Default;
					// Check the stack for any additional encounter triggers waiting to be called.
					SM.Push(OnCheckForEncounters, "EncounterCompleteCheckForEncounters");
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

					var details = Payload.Game.SaveDetails.Value;
					details.TransitHistoryId = Payload.Game.TransitHistory.Peek().Id;
					details.Time = DateTime.Now;
					details.ElapsedTime = Payload.Game.ElapsedTime.Value;
					Payload.Game.SaveDetails.Value = details;

					Payload.Game.SetMetaKey(
						MetaKeyConstants.Game.IsCompleted,
						Payload.Game.SaveDetails.Value.IsCompleted ? MetaKeyConstants.Values.True : MetaKeyConstants.Values.False
					);

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
				App.Callbacks.SaveRequest(SaveRequest.Failure(request, result.Error));
				return;
			}

			App.Callbacks.SaveRequest(SaveRequest.Success(request));
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			if (block.System == null || block.State != CelestialSystemStateBlock.States.Selected) return;
			if (Payload.Game.Context.EncounterState.Current.Value.State != EncounterStateModel.States.Complete)
			{
				Debug.LogWarning("Checking for Navigation Selection events during encounter is not supported, ignoring");
				return;
			}

			var allSpecifiedEncounters = block.System.SpecifiedEncounters.Value.Where(s => !string.IsNullOrEmpty(s.EncounterId) && s.Trigger == EncounterTriggers.NavigationSelect);

			if (1 < allSpecifiedEncounters.Count())
			{
				Debug.LogError("Multiple encounters are specified for system " + block.System.Name + " with trigger " + EncounterTriggers.NavigationSelect + ", this behaviour is not supported yet, choosing the first one instead.");
			}

			var encounterId = allSpecifiedEncounters.FirstOrDefault().EncounterId;

			OnCheckSpecifiedEncounter(encounterId, EncounterTriggers.NavigationSelect, true);
		}

		void OnKeyValueRequest(KeyValueRequest request)
		{
			if (request.State != KeyValueRequest.States.SetRequest) return;

			switch (request.ValueType)
			{
				case KeyValueTypes.Boolean: OnKeyValueRequestBoolean(request.Target, request.Key, request.BooleanValue); break;
				case KeyValueTypes.Integer: OnKeyValueRequestInteger(request.Target, request.Key, request.IntegerValue); break;
				case KeyValueTypes.String: OnKeyValueRequestString(request.Target, request.Key, request.StringValue); break;
				case KeyValueTypes.Float: OnKeyValueRequestFloat(request.Target, request.Key, request.FloatValue); break;
				default:
					Debug.LogError("Unrecognized ValueType: " + request.ValueType);
					break;
			}
		}

		void OnKeyValueRequestBoolean(KeyValueTargets target, string key, bool value)
		{
			switch (target)
			{
				case KeyValueTargets.Game:
					break;
			}
		}

		void OnKeyValueRequestInteger(KeyValueTargets target, string key, int value)
		{
			switch (target)
			{
				case KeyValueTargets.Game:
					if (key == KeyDefines.Game.PropellantUsage.Key)
					{
						Payload.Game.Ship.Velocity.Value = Payload.Game.Ship.Velocity.Value.Duplicate(
							propellantUsage: Mathf.Min(value, Payload.Game.Ship.Velocity.Value.PropellantUsageLimit)
						);
					}
					else if (key == KeyDefines.Game.PropellantUsageMinimum.Key)
					{
						Payload.Game.Ship.Velocity.Value = Payload.Game.Ship.Velocity.Value.Duplicate(
							propellantUsage: Mathf.Max(Payload.Game.Ship.Velocity.Value.PropellantUsage, value)
						);
					}
					else if (key == KeyDefines.Game.PropellantUsageMaximum.Key)
					{
						Payload.Game.Ship.Velocity.Value = Payload.Game.Ship.Velocity.Value.Duplicate(
							profile: Payload.Game.Ship.Velocity.Value.Profile.Duplicate(
								count: value
							),
							propellantUsage: Mathf.Min(Payload.Game.Ship.Velocity.Value.PropellantUsage, value)
						);
					}
					else if (key == KeyDefines.Game.Propellant.Key)
					{
						Payload.Game.Ship.Velocity.Value = Payload.Game.Ship.Velocity.Value.Duplicate(
							propellantUsage: Mathf.Min(Payload.Game.Ship.Velocity.Value.PropellantUsage, value),
							propellantUsageLimit: value
						);
					}
					break;
			}
		}

		void OnKeyValueRequestString(KeyValueTargets target, string key, string value)
		{
			switch (target)
			{
				case KeyValueTargets.Game:
					break;
			}
		}

		void OnKeyValueRequestFloat(KeyValueTargets target, string key, float value)
		{
			switch (target)
			{
				case KeyValueTargets.Game:
					if (key == KeyDefines.Game.TransitRangeMinimum.Key) Payload.Game.Ship.SetRangeMinimum(value);
					else if (key == KeyDefines.Game.TransitVelocityMinimum.Key)
					{
						Payload.Game.Ship.Velocity.Value = Payload.Game.Ship.Velocity.Value.Duplicate(
							profile: Payload.Game.Ship.Velocity.Value.Profile.Duplicate(
								velocityMinimumLightYears: value
							)
						);
					}
					break;
			}
		}
		#endregion
	}
}