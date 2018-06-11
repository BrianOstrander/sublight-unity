using System;

using UnityEngine;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SpeedPresenter : Presenter<ISpeedView>
	{
		GameModel model;

		public SpeedPresenter(GameModel model)
		{
			this.model = model;

			model.DayTime.Changed += OnDayTime;
			App.Callbacks.StateChange += OnStateChange;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.DayTime.Changed -= OnDayTime;
			App.Callbacks.StateChange -= OnStateChange;
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
			delta *= model.Speed;
			var newDayTime = model.DayTime.Value;
			if (newDayTime.Time + delta < DayTime.TimeInDay) newDayTime.Time += delta;
			else
			{
				var totalTime = newDayTime.Time + delta;
				var newTime = totalTime % DayTime.TimeInDay;
				var dayTime = totalTime - newTime;
				newDayTime.Day += Mathf.FloorToInt(dayTime / DayTime.TimeInDay);
				newDayTime.Time = newTime;
			}
			model.DayTime.Value = newDayTime;
		}

		void OnDayTime(DayTime current)
		{
			View.Current = current;
		}

		void OnClick(float speed)
		{
			model.Speed.Value = speed;
		}
		#endregion
	}
}