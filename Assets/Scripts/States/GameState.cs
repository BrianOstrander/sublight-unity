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
		public int InterstellarSectorOffset = 1;
		public int InterstellarSectorOffsetTotal { get { return (InterstellarSectorOffset * 2) + 1; } }
		public int InterstellarSectorCount { get { return InterstellarSectorOffsetTotal * InterstellarSectorOffsetTotal; } }

		public GameModel Game;

		public HoloRoomFocusCameraPresenter MainCamera;
		public List<IPresenterCloseShowOptions> ShowOnIdle = new List<IPresenterCloseShowOptions>();

		public KeyValueListener KeyValueListener;

		public List<SectorInstanceModel> SectorInstances = new List<SectorInstanceModel>();
		public UniversePosition LastInterstellarFocus = new UniversePosition(new Vector3Int(int.MinValue, 0, int.MinValue));
	}

	public partial class GameState : State<GamePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Game, SceneConstants.HoloRoom }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(LoadModelDependencies);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(done => Focuses.InitializePresenters(this, done));
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

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeCallbacks(Action done)
		{
			App.Callbacks.DialogRequest += OnDialogRequest;
			Payload.Game.ToolbarSelection.Changed += OnToolbarSelection;

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
			var activeScale = Payload.Game.ActiveScale;
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
			if (transform.UniverseOrigin.SectorEquals(Payload.LastInterstellarFocus))
			{
				Payload.LastInterstellarFocus = transform.UniverseOrigin;
				return;
			}
			Payload.LastInterstellarFocus = transform.UniverseOrigin;

			var originInt = transform.UniverseOrigin.SectorInteger;
			var minSector = new Vector3Int(originInt.x - Payload.InterstellarSectorOffset, 0, originInt.z - Payload.InterstellarSectorOffset);
			var maxSector = new Vector3Int(originInt.x + Payload.InterstellarSectorOffset, 0, originInt.z + Payload.InterstellarSectorOffset);

			var staleSectors = new List<SectorInstanceModel>();

			var sectorPositionExists = new bool[Payload.InterstellarSectorOffsetTotal, Payload.InterstellarSectorOffsetTotal];

			foreach (var sector in Payload.SectorInstances)
			{
				var currSectorInt = sector.Sector.Value.Position.Value.SectorInteger;
				if (currSectorInt.x < minSector.x || maxSector.x < currSectorInt.x || currSectorInt.z < minSector.z || maxSector.z < currSectorInt.z)
				{
					// out of range
					staleSectors.Add(sector);
				}
				else sectorPositionExists[currSectorInt.x - minSector.x, currSectorInt.z - minSector.z] = true;
			}

			for (var x = 0; x < Payload.InterstellarSectorOffsetTotal; x++)
			{
				for (var z = 0; z < Payload.InterstellarSectorOffsetTotal; z++)
				{
					if (sectorPositionExists[x, z]) continue;
					var replacement = staleSectors.First();
					staleSectors.RemoveAt(0);
					OnCelestialSystemsTransformUpdateSystems(replacement, new UniversePosition(new Vector3Int(x + minSector.x, 0, z + minSector.z)));
				}
			}
		}

		void OnCelestialSystemsTransformUpdateSystems(SectorInstanceModel sectorModel, UniversePosition sectorPosition)
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
		#endregion
	}
}