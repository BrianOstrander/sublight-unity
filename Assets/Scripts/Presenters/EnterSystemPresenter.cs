using UnityEngine;

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

			App.Callbacks.TravelRequest += OnTravelRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.TravelRequest -= OnTravelRequest;
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
		void OnTravelRequest(TravelRequest travelRequest)
		{
			switch(travelRequest.State)
			{
				case TravelRequest.States.Complete:
					if (!travelRequest.Destination.Visited) Show(travelRequest.Destination);
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