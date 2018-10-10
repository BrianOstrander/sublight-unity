using System.Linq;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class EndSystemPresenter : Presenter<IEndSystemView>, IPresenterCloseShow
	{
		GameModel model;
		SystemModel system;

		public EndSystemPresenter(GameModel model)
		{
			this.model = model;
			system = model.Universe.Value.GetSystem(model.EndSystem);

			model.FocusedSectors.Changed += OnFocusedSectors;
			model.TravelRequest.Changed += OnTravelRequest;
		}

		protected override void OnUnBind()
		{
			model.FocusedSectors.Changed -= OnFocusedSectors;
			model.TravelRequest.Changed -= OnTravelRequest;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = system.Position;
			View.Click = OnClick;
			ShowView(instant: true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
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
					App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.WonInfo, Strings.WonInfoTitle, DialogStyles.Neutral, OnAlertOkay));
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