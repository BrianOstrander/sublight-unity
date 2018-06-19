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
			model.DestructionRadius.Changed += OnDestructionRadius;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.Ship.Value.Rations.Changed -= OnRations;
			model.DestructionRadius.Changed -= OnDestructionRadius;
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
			Show(Strings.OutOfRations);
		}

		void OnDestructionRadius(float radius)
		{
			if (radius < UniversePosition.Distance(UniversePosition.Zero, model.Ship.Value.Position)) return;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			Show(Strings.DestroyedByVoid);
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