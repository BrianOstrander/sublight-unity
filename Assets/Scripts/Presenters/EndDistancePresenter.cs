using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class EndDistancePresenter : Presenter<IEndDistanceView>, IPresenterCloseShow
	{
		GameModel model;

		public EndDistancePresenter(GameModel model)
		{
			this.model = model;

			model.Ship.Value.Position.Changed += OnShipPosition;
		}

		protected override void OnUnBind()
		{
			model.Ship.Value.Position.Changed -= OnShipPosition;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.EncyclopediaClick = OnEncyclopediaClick;
			SetDistance(model.Ship.Value.Position);

			ShowView(App.GameCanvasRoot, true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
		}

		void SetDistance(UniversePosition position)
		{
			View.Distance = UniversePosition.ToLightYearDistance(UniversePosition.Distance(position, model.EndSystem));
		}

		#region Events
		void OnEncyclopediaClick()
		{
			App.Callbacks.FocusRequest(
				EncyclopediaFocusRequest.Home()
			);
		}

		void OnShipPosition(UniversePosition position)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			SetDistance(position);
		}
		#endregion
	}
}