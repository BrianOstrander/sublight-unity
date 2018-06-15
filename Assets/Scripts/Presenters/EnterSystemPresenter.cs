﻿using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class EnterSystemPresenter : Presenter<IEnterSystemView>
	{
		GameModel model;

		public EnterSystemPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.TravelProgress += OnTravelProgress;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.TravelProgress -= OnTravelProgress;
		}

		void Show(SystemModel destination)
		{
			if (View.Visible) return;
			View.Reset();
			View.Title = "Arrived in "+ destination.Name.Value;
			View.Details = "Aquired " + destination.Rations.Value + " rations";
			View.OkayClick = () => OnOkayClick(destination);
			ShowView(model.GameplayCanvas, true);
		}

		#region Events
		void OnTravelProgress(TravelProgress travelProgress)
		{
			switch(travelProgress.State)
			{
				case TravelProgress.States.Complete:
					if (!travelProgress.Destination.Visited) Show(travelProgress.Destination);
					break;
			}
		}

		void OnOkayClick(SystemModel destination)
		{
			destination.Visited.Value = true;
			model.Ship.Value.Rations.Value += destination.Rations;
			switch(View.TransitionState)
			{
				case TransitionStates.Shown:
					CloseView(true);
					break;
			}
		}
		#endregion

	}
}