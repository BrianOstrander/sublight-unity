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
			/*
			Payload.Game.ScaleLabelSystem.Changed += value => Debug.Log("System label is now " + value.Name.Value);
			Payload.Game.ScaleLabelLocal.Changed += value => Debug.Log("Local label is now " + value.Name.Value);
			Payload.Game.ScaleLabelStellar.Changed += value => Debug.Log("Stellar label is now " + value.Name.Value);
			Payload.Game.ScaleLabelQuadrant.Changed += value => Debug.Log("Quadrant label is now " + value.Name.Value);
			Payload.Game.ScaleLabelGalactic.Changed += value => Debug.Log("Galactic label is now " + value.Name.Value);
			Payload.Game.ScaleLabelCluster.Changed += value => Debug.Log("Cluster label is now " + value.Name.Value);
			*/
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(LoadModelDependencies);
			App.SM.PushBlocking(SetNonSerializedValues);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(done => Focuses.InitializePresenters(this, done));
			App.SM.PushBlocking(InitializeScaleTransforms);
			App.SM.PushBlocking(InitializeFocus);
			App.SM.PushBlocking(InitializeCelestialSystems);
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
			App.Callbacks.DialogRequest += OnDialogRequest;
			Payload.Game.ToolbarSelection.Changed += OnToolbarSelection;
			Payload.Game.FocusTransform.Changed += OnFocusTransform;

			Payload.Game.WaypointCollection.Waypoints.Changed += OnWaypoints;
			Payload.Game.Ship.Value.Position.Changed += OnShipPosition;

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
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;
			Payload.Game.ToolbarSelection.Changed -= OnToolbarSelection;
			Payload.Game.FocusTransform.Changed -= OnFocusTransform;

			Payload.Game.WaypointCollection.Waypoints.Changed -= OnWaypoints;
			Payload.Game.Ship.Value.Position.Changed -= OnShipPosition;

			foreach (var scaleTransformProperty in EnumExtensions.GetValues(UniverseScales.Unknown).Select(s => Payload.Game.GetScale(s).Transform))
			{
				scaleTransformProperty.Changed -= OnScaleTransform;
			}

			Payload.Game.GetScale(UniverseScales.Local).Transform.Changed -= OnCelestialSystemsTransform;
		}
		#endregion

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
			switch (request.State)
			{
				case DialogRequest.States.Request:
					Debug.LogError("Todo reuqest logic");
					//App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetPriorityFocus()));
					break;
				case DialogRequest.States.Completing:
					Debug.LogError("Todo completing logic");
					//App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetMainMenuFocus()));
					break;
			}
		}

		void OnToolbarSelection(ToolbarSelections selection)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetToolbarSelectionFocus(selection)));
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
		#endregion
	}
}