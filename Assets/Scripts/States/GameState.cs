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
		public ToolbarPresenter Toolbar;

		public KeyValueListener KeyValueListener;

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
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(done => Focuses.InitializePresenters(this, done));
			App.SM.PushBlocking(InitializeFocus);
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
		}

		void OnIdleShowFocus()
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetToolbarSelectionFocus(Payload.Game.ToolbarSelection), OnIdleShowFocusDone, 0.5f));
		}

		void OnIdleShowFocusDone()
		{
			Payload.Toolbar.Show();
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