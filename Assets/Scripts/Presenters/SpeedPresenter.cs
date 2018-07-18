using System;

using UnityEngine;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SpeedPresenter : Presenter<ISpeedView>, IPresenterCloseShow
	{
		GameModel model;

		public SpeedPresenter(GameModel model)
		{
			this.model = model;

			model.DayTime.Changed += OnDayTime;
			App.Callbacks.SpeedRequest += OnSpeedChange;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void OnUnBind()
		{
			model.DayTime.Changed -= OnDayTime;
			App.Callbacks.SpeedRequest -= OnSpeedChange;
			App.Heartbeat.Update -= OnUpdate;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Current = model.DayTime;
			View.Click = OnClick;
			View.SelectedSpeed = App.Callbacks.LastSpeedRequest.Index;

			ShowView(App.GameCanvasRoot, true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
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
			if (View.TransitionState == TransitionStates.Shown) View.Current = current;
			App.Callbacks.DayTimeDelta(new DayTimeDelta(current, App.Callbacks.LastDayTimeDelta.Current));
		}

		void OnSpeedChange(SpeedRequest speedChange)
		{
			switch(speedChange.State)
			{
				case SpeedRequest.States.Request:
					model.Speed.Value = speedChange.Speed;
					if (View.TransitionState == TransitionStates.Shown) View.SelectedSpeed = speedChange.Index;
					App.Callbacks.SpeedRequest(speedChange.Duplicate(SpeedRequest.States.Complete));
					break;
			}
		}

		void OnClick(int index)
		{
			var request = SpeedRequest.PauseRequest;
			switch(index)
			{
				case 0: break;
				case 1: request = SpeedRequest.PlayRequest; break;
				case 2: request = SpeedRequest.FastRequest; break;
				case 3: request = SpeedRequest.FastFastRequest; break;
				default: Debug.LogWarning("Unknown speed index: " + index); break;
			}
			View.SelectedSpeed = index;
			App.Callbacks.SpeedRequest(request);
		}
		#endregion
	}
}