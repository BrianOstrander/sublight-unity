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
		public GameModel Game;

		public HoloRoomFocusCameraPresenter MainCamera;
		public List<IPresenterCloseShowOptions> ShowOnIdle = new List<IPresenterCloseShowOptions>();

		public KeyValueListener KeyValueListener;


		public List<SectorInstanceModel> SectorInstances = new List<SectorInstanceModel>();
		//public Dictionary<SetFocusLayers, Ipresenter>
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

			//Debug.Log("Undo this hack here, it will mess with player start pos!");
			if (!Payload.Game.PlayerStartSelected.Value) Payload.Game.Ship.Value.Position.Value = Payload.Game.Galaxy.PlayerStart.Value;

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
		#endregion

		#region Idle
		protected override void Idle()
		{
			App.Callbacks.HoloColorRequest(new HoloColorRequest(Color.white));
			App.Callbacks.CameraMaskRequest(CameraMaskRequest.Reveal(0.75f, OnIdleShowFocus));

			Payload.Game.ActiveScale.Opacity.Changed(1f); // Probably bad to do and I should feel bad... but oh well...

			//App.Callbacks.
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
		#endregion
	}
}