using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm
{
	public class HomePayload : IStatePayload 
	{
		//public float SomeVariable;
	}

	public class HomeState : State<HomePayload>
	{
		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		protected override void Idle()
		{
			//App.SM.PushBlocking(InitializeCamera);
			//new ReticlePresenter().Show();
		}

		//void InitializeCamera(Action done)
		//{
		//	new CameraPresenter(new CameraModel()).Show(done);
		//}
	}
}