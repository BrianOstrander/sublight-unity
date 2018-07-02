using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DestructionSpeedPresenter : Presenter<IDestructionSpeedView>
	{
		// TODO: These should be somewhere else...
		const float TimeInUnit = 1f;
		const float TimeInTimeline = TimeInUnit * 5f;

		GameModel model;
		DayTime lastUpdated;

		public DestructionSpeedPresenter(GameModel model)
		{
			this.model = model;

			model.DayTime.Changed += OnDayTime;
			model.DestructionSpeed.Changed += OnDestructionSpeed;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.DayTime.Changed -= OnDayTime;
			model.DestructionSpeed.Changed -= OnDestructionSpeed;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.TimeInUnit = TimeInUnit;
			View.Speed = model.DestructionSpeed;
			View.Current = model.DayTime;

			foreach (var delta in model.DestructionSpeedDeltas.Value) View.AddAlert(delta.Speed, delta.Start);
			lastUpdated = model.DayTime;

			ShowView(App.GameCanvasRoot, true);
		}

		#region Events
		void OnDayTime(DayTime current)
		{
			View.Current = current;

			if (current.Day == lastUpdated.Day) return;
			lastUpdated = current;

			if (0 < model.DestructionSpeedDeltas.Value.Length)
			{
				float? newSpeed = null;
				foreach (var delta in model.DestructionSpeedDeltas.Value)
				{
					if (current < delta.Start) break;
					newSpeed = delta.Speed;
				}
				if (newSpeed.HasValue) model.DestructionSpeed.Value = newSpeed.Value;
			}

			var newDeltas = model.DestructionSpeedDeltas.Value.Where(d => current.Day <= d.Start.Day).ToList();

			var possibleIncrements = new float[] {
				model.DestructionSpeedIncrement * -2f,
				-model.DestructionSpeedIncrement,
				0f,
				0f,
				0f,
				0f,
				0f,
				0f,
				model.DestructionSpeedIncrement,
				model.DestructionSpeedIncrement * 2f
			};

			var lastSpeed = model.DestructionSpeed.Value;
			if (newDeltas.Count != 0) lastSpeed = newDeltas.Last().Speed;
			var nextSpeed = Mathf.Max(0f, lastSpeed + possibleIncrements.Random());

			if (Mathf.Approximately(lastSpeed, nextSpeed)) return;

			var newDelta = new DestructionSpeedDelta(nextSpeed, current.TimeZero.Add(0, TimeInTimeline));
			newDeltas.Add(newDelta);
			View.AddAlert(newDelta.Speed, newDelta.Start);
			model.DestructionSpeedDeltas.Value = newDeltas.ToArray();
		}

		void OnDestructionSpeed(float speed)
		{
			//Debug.Log("Destruction Speed: " + speed.ToString("F4"));
			View.Speed = speed;
		}
		#endregion
	}
}