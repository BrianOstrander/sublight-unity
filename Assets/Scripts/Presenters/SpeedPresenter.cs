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
			App.Callbacks.SpeedRequest += OnSpeedChange;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.DayTime.Changed -= OnDayTime;
			App.Callbacks.SpeedRequest -= OnSpeedChange;
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

		void OnSpeedChange(SpeedRequest speedChange)
		{
			switch(speedChange.State)
			{
				case SpeedRequest.States.Request:
					model.Speed.Value = speedChange.Speed;
					App.Callbacks.SpeedRequest(speedChange.Duplicate(SpeedRequest.States.Complete));
					break;
			}
		}

		void OnClick(float speed)
		{
			App.Callbacks.SpeedRequest(new SpeedRequest(SpeedRequest.States.Request, speed));
		}
		#endregion
	}
}