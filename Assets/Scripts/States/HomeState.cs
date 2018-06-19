using System;

using LunraGames.SpaceFarm.Presenters;

namespace LunraGames.SpaceFarm
{
	public class HomePayload : IStatePayload 
	{
		
	}

	public class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Home }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
		}

		void LoadScenes(Action done)
		{
			App.SceneService.Request(SceneRequest.Load(result => done(), Scenes));
		}
  		#endregion

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

			App.Callbacks.ClearEscapables();

			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnLoadScenes);
		}

		void UnLoadScenes(Action done)
		{
			App.SceneService.Request(SceneRequest.UnLoad(result => done(), Scenes));
		}

		void UnBind(Action done)
		{
			// All presenters will have their views closed and unbinded. Events
			// will also be unbinded.
			App.P.UnRegisterAll(done);
		}
  		#endregion
	}
}