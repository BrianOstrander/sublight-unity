using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Presenters;

namespace LunraGames.SpaceFarm
{
	public class InitializePayload : IStatePayload
	{
		public List<GameObject> DefaultViews = new List<GameObject>();
		public DefaultShaderGlobals ShaderGlobals;

		public HomePayload homePayload = new HomePayload();
	}

	public class InitializeState : State<InitializePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Initialize; } }

		protected override void Begin()
		{
			Payload.ShaderGlobals.Apply();
			App.SM.PushBlocking(InitializeSaveLoadService);
			App.SM.PushBlocking(InitializeViews);
			App.SM.PushBlocking(InitializeModels);
			App.SM.PushBlocking(InitializePresenters);
		}

		protected override void Idle()
		{
			App.P.AddGlobals(new DialogPresenter());
			App.P.AddGlobals(new ShadePresenter());
			App.SM.RequestState(Payload.homePayload);
		}

		void InitializeSaveLoadService(Action done)
		{
			App.SaveLoadService.Initialize(
				status =>
				{
					if (status == RequestStatus.Success) App.Log("SaveLoadService Initialized", LogTypes.Initialization);
					else App.Restart("Initializing SaveLoadService failed with status " + status);

					done();
				}
			);
		}

		#region Mediators
		void InitializeViews(Action done)
		{
			App.V.Initialize(
				Payload.DefaultViews,
				App.Main.transform,
				status =>
				{
					if (status == RequestStatus.Success) App.Log("ViewMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing ViewMediator failed with status " + status);

					done();
				}
			);
		}

		void InitializeModels(Action done)
		{
			App.M.Initialize(
				status =>
				{
					if (status == RequestStatus.Success) App.Log("ModelMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing ModelMediator failed with status " + status);

					done();
				}
			);
		}

		void InitializePresenters(Action done)
		{
			App.P.Initialize(
				status =>
				{
					if (status == RequestStatus.Success) App.Log("PresenterMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing PresenterMediator failed with status " + status);

					done();
				}
			);
		}
		#endregion
	}
}