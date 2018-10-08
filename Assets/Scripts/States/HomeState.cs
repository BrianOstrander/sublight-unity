using System;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Presenters;

namespace LunraGames.SubLight
{
	public class HomePayload : IStatePayload 
	{
		public SaveModel[] Saves = new SaveModel[0];
	}

	public partial class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Home, SceneConstants.HoloRoom }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(Focuses.InitializePresenters);
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
			done();
		}

		void InitializeFocus(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Default(Focuses.GetDefaultFocuses(), () => OnInializeFocusDefaults(done)));
		}

		void OnInializeFocusDefaults(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.GetMainMenuFocus(), done));
			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			App.SM.PushBlocking(InitializeLoadSaves);
			App.SM.PushBlocking(ShowLoadedSaves);
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
			done();
		}

		void ShowLoadedSaves(Action done)
		{
			UnityEngine.Debug.Log("Show saves here...");
			done();
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
		#endregion
	}
}