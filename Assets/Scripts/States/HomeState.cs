using System;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Presenters;

namespace LunraGames.SpaceFarm
{
	public class HomePayload : IStatePayload 
	{
		public SaveModel[] Saves = new SaveModel[0];
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
			App.SM.PushBlocking(InitializeLoadSaves);
			App.SM.PushBlocking(InitializeMenu);
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

		void InitializeLoadSaves(Action done)
		{
			App.SaveLoadService.List<GameModel>(result => OnInitializeLoadSaves(result, done));
		}

		void OnInitializeLoadSaves(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				// TODO: Error logic.
			}
			else Payload.Saves = result.Models;
			done();
		}

		void InitializeMenu(Action done)
		{
			new HomeMenuPresenter(Payload.Saves).Show(done);
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