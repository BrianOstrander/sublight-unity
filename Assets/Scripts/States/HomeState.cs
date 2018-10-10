using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Presenters;

using UnityEngine;

namespace LunraGames.SubLight
{
	public class HomePayload : IStatePayload 
	{
		public bool CanContinueGame;
		public SaveModel[] Saves = new SaveModel[0];

		public GameObject HoloSurfaceOrigin;

		public Dictionary<float, IPresenterCloseShowOptions[]> DelayedPresenterShows = new Dictionary<float, IPresenterCloseShowOptions[]>();
	}

	public partial class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Home, SceneConstants.HoloRoom }; } }
		static string[] Tags { get { return new string[] { TagConstants.HoloSurfaceOrigin }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(InitializeLoadSaves);
			App.SM.PushBlocking(done => Focuses.InitializePresenters(this, done));
			App.SM.PushBlocking(InitializeFocus);
		}

		void LoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.Load(result => OnLoadScenes(result, done), Scenes, Tags));
		}

		void OnLoadScenes(SceneRequest result, Action done)
		{
			foreach (var kv in result.FoundTags)
			{
				switch (kv.Key)
				{
					case TagConstants.HoloSurfaceOrigin: Payload.HoloSurfaceOrigin = kv.Value; break;
				}
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
			done();
		}

		void InitializeFocus(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Default(Focuses.GetDefaultFocuses(), () => OnInializeFocusDefaults(done)));
		}

		void OnInializeFocusDefaults(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.GetMainMenuFocus(), done));
		}

		void InitializeLoadSaves(Action done)
		{
			App.M.List<GameModel>(result => OnInitializeLoadSaves(result, done));
		}

		void OnInitializeLoadSaves(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				UnityEngine.Debug.LogError("Unable to load a list of saved games");
				// TODO: Error logic.
			}
			else Payload.Saves = result.Models;

			Payload.CanContinueGame = Payload.Saves.Any(s => s.SupportedVersion.Value);

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			foreach (var kv in Payload.DelayedPresenterShows)
			{
				App.Heartbeat.Wait(
					() =>
					{
						foreach (var presenter in kv.Value) presenter.Show();
					},
					kv.Key
				);
			}
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);

			App.Callbacks.ClearEscapables();

			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnLoadScenes);
		}

		void UnLoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.UnLoad(result => done(), Scenes));
		}

		void UnBind(Action done)
		{
			// All presenters will have their views closed and unbinded. Events
			// will also be unbinded.
			App.P.UnRegisterAll(done);
		}
		#endregion

		#region Events
		void OnNewGameClick()
		{
			Debug.Log("lol new game");
		}

		void OnContinueGameClick()
		{
			OnNotImplimentedClick();
		}

		void OnSettingsClick()
		{
			OnNotImplimentedClick();
		}

		void OnCreditsClick()
		{
			OnNotImplimentedClick();
		}

		void OnExitClick()
		{
			Debug.Log("Quiting");
			Application.Quit();
		}

		void OnNotImplimentedClick()
		{
			Debug.Log("lol not implimented");
		}
		#endregion
	}
}