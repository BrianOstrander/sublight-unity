using System.Linq;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class EndSystemPresenter : Presenter<IEndSystemView>
	{
		GameModel model;
		SystemModel system;

		public EndSystemPresenter(GameModel model)
		{
			this.model = model;
			system = model.Universe.Value.GetSystem(model.EndSystem);

			model.FocusedSectors.Changed += OnFocusedSectors;
			App.Callbacks.TravelRequest += OnTravelRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.FocusedSectors.Changed -= OnFocusedSectors;
			App.Callbacks.TravelRequest -= OnTravelRequest;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = system.Position;
			View.Click = OnClick;
			ShowView(instant: true);
		}

		#region Events
		void OnClick()
		{
			App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.EndInfo, Strings.EndInfoTitle));
		}

		void OnFocusedSectors(UniversePosition[] positions)
		{
			switch(View.TransitionState)
			{
				case TransitionStates.Shown:
					if (!positions.Contains(system.Position.Value.SystemZero))
					{
						CloseView(true);
					}
					break;
				case TransitionStates.Unknown:
				case TransitionStates.Closed:
					if (positions.Contains(system.Position.Value.SystemZero))
					{
						Show();
					}
					break;
			}
		}

		void OnTravelRequest(TravelRequest travelRequest)
		{
			switch (travelRequest.State)
			{
				case TravelRequest.States.Complete:
					// Only pop up on end system
					if (travelRequest.Destination != model.EndSystem.Value) return;
					App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.WonInfo, Strings.WonInfoTitle, OnAlertOkay));
					break;
			}
		}

		void OnAlertOkay()
		{
			var payload = new HomePayload();
			App.SM.RequestState(payload);
		}
		#endregion
	}
}