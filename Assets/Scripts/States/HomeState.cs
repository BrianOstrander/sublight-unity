using System;

using UnityEngine.SceneManagement;

using LunraGames.SpaceFarm.Presenters;

namespace LunraGames.SpaceFarm
{
	public class HomePayload : IStatePayload 
	{
		public Action UnloadSceneCallback = ActionExtensions.Empty;
	}

	public class HomeState : State<HomePayload>
	{
		/// Reminder: Keep local variables in the payload so it's easy to reset 
		/// states upon transition.

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		#region Idle
		protected override void Idle()
		{
			App.SM.PushBlocking(InitializeCamera);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeMenu);
			//new CursorPresenter().Show();
		}

		void InitializeCamera(Action done)
		{
			new CameraPresenter().Show(done);
		}

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeMenu(Action done)
		{
			new HomeMenuPresenter().Show(done);
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);
			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnloadScene);
		}

		void UnBind(Action done)
		{
			App.P.UnRegisterAll(done);
		}

		void UnloadScene(Action done)
		{
			Payload.UnloadSceneCallback = done;
			App.Callbacks.SceneUnload += OnSceneUnloaded;
			SceneManager.UnloadSceneAsync(SceneConstants.Home);
		}

		void OnSceneUnloaded(Scene scene)
		{
			App.Callbacks.SceneUnload -= OnSceneUnloaded;
			Payload.UnloadSceneCallback();
		}
  		#endregion
	}
}