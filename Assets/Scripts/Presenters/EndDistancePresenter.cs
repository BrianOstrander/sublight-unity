using System;

using UnityEngine;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
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
		void OnShipPosition(UniversePosition position)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			SetDistance(position);
		}
		#endregion
	}
}