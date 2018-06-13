using System;

using UnityEngine;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SpeedPresenter : Presenter<ISpeedView>
	{
		GameModel model;

		DayTime lastDayTime;

		public SpeedPresenter(GameModel model)
		{
			this.model = model;

			model.DayTime.Changed += OnDayTime;
			App.Callbacks.StateChange += OnStateChange;
			App.Callbacks.SpeedChange += OnSpeedChange;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.DayTime.Changed -= OnDayTime;
			App.Callbacks.StateChange -= OnStateChange;
			App.Callbacks.SpeedChange -= OnSpeedChange;
			App.Heartbeat.Update -= OnUpdate;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.Click = OnClick;
			if (done != null) View.Shown += done;
			ShowView(model.GameplayCanvas, true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnUpdate(float delta)
		{
			if (Mathf.Approximately(0f, model.Speed)) return;
			delta *= model.Speed;
			var newDayTime = model.DayTime.Value.Add(0, delta);
			model.DayTime.Value = newDayTime;
		}

		void OnDayTime(DayTime current)
		{
			View.Current = current;
			App.Callbacks.DayTimeDelta(new DayTimeDelta(current, lastDayTime));
			lastDayTime = current;
		}

		void OnSpeedChange(SpeedChange speedChange)
		{
			switch(speedChange.State)
			{
				case SpeedChange.States.Request:
					model.Speed.Value = speedChange.Speed;
					App.Callbacks.SpeedChange(speedChange.Duplicate(SpeedChange.States.Complete));
					break;
			}
		}

		void OnClick(float speed)
		{
			App.Callbacks.SpeedChange(new SpeedChange(SpeedChange.States.Request, speed));
		}
		#endregion
	}
}