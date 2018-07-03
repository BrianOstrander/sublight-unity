using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;
using UnityEngine;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DetailSystemPresenter : Presenter<IDetailSystemView>
	{
		GameModel model;

		SystemHighlight nextHighlight;

		public DetailSystemPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.SystemHighlight += OnSystemHighlight;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.SystemHighlight -= OnSystemHighlight;
		}

		public void Show(SystemModel system)
		{
			if (View.Visible) return;
			View.Reset();
			View.Closed += OnClosed;
			View.Name = system.Name;
			View.DayTravelTime = Mathf.Min(1, UniversePosition.TravelTime(system.Position, model.Ship.Value.Position, model.Ship.Value.SpeedTotal).Day);

			var rationText = string.Empty;
			var rationColor = Color.white;
			GetResources(model.Ship.Value.ResourceDetection, system.RationsDetection, system.Rations, out rationText, out rationColor);
			View.Rations = rationText;
			View.RationsColor = rationColor;

			var fuelText = string.Empty;
			var fuelColor = Color.white;
			GetResources(model.Ship.Value.ResourceDetection, system.FuelDetection, system.Fuel, out fuelText, out fuelColor);
			View.Fuel = fuelText;
			View.FuelColor = fuelColor;

			ShowView(App.GameCanvasRoot, true);
		}

		void GetResources(float shipDetection, float systemDetection, float amount, out string text, out Color color)
		{
			if (shipDetection < systemDetection)
			{
				text = "[UNKNOWN]";
				color = Color.white;
				return;
			}
			if (Mathf.Approximately(0f, amount))
			{
				text = "[NONE]";
				color = Color.red;
				return;
			}
			text = "[DETECTED]";
			color = Color.green;
			return;
		}

		#region Events
		void OnSystemHighlight(SystemHighlight highlight)
		{
			nextHighlight = highlight;
			switch (highlight.State)
			{
				case SystemHighlight.States.End:
				case SystemHighlight.States.Change:
					if (View.TransitionState == TransitionStates.Shown) CloseView(true);
					break;
				case SystemHighlight.States.Begin:
					Show(highlight.System);
					break;
			}
		}

		void OnClosed()
		{
			if (App.SM.CurrentEvent == StateMachine.Events.End) return;

			switch (nextHighlight.State)
			{
				case SystemHighlight.States.Begin:
				case SystemHighlight.States.Change:
					Show(nextHighlight.System);
					break;
			}
		}
		#endregion
	}
}