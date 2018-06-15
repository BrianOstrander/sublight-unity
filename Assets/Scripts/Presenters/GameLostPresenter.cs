using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class GameLostPresenter : Presenter<IGameLostView>
	{
		GameModel model;

		public GameLostPresenter(GameModel model)
		{
			this.model = model;
			model.Ship.Value.Rations.Changed += OnRations;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.Ship.Value.Rations.Changed -= OnRations;
		}

		void Show(string reason)
		{
			if (View.Visible) return;
			View.Reset();
			View.Reason = reason;
			View.MainMenuClick = OnMainMenuClick;
			ShowView(model.GameplayCanvas, true);
		}

		#region Events
		void OnRations(float rations)
		{
			if (!Mathf.Approximately(0f, rations)) return;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			Show("Out of rations");
		}

		void OnMainMenuClick()
		{
			switch(View.TransitionState)
			{
				case TransitionStates.Shown:
					CloseView(true);
					var payload = new HomePayload();
					App.SM.RequestState(payload);
					break;
			}
		}
		#endregion

	}
}