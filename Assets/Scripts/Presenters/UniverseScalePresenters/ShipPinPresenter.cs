using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ShipPinPresenter : UniverseScalePresenter<IShipPinView>
	{
		UniversePosition positionInUniverse;

		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public ShipPinPresenter(GameModel model, UniverseScales scale) : base(model, scale)
		{
			OnPosition(model.Ship.Value.Position.Value);

			model.Ship.Value.Position.Changed += OnPosition;
			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Ship.Value.Position.Changed -= OnPosition;
			Model.Context.TransitState.Changed -= OnTransitState;
		}

		protected override void OnShowView()
		{
			View.PositionIgnores = UniverseScaleAxises.Y;

			switch (Scale)
			{
				case UniverseScales.Local:
					View.PositionIgnores = UniverseScaleAxises.None;
					break;
				case UniverseScales.Stellar: break;
				case UniverseScales.Quadrant:
					View.AdditionalYOffset = -0.15f;
					break;
				case UniverseScales.Galactic:
					View.AdditionalYOffset = -0.5f;
					break;
				case UniverseScales.Cluster:
					View.AdditionalYOffset = -0.6f;
					break;
			}

			View.TimeScalar = Model.Context.TransitState.Value.RelativeTimeScalar;
		}

		#region Events
		void OnPosition(UniversePosition position)
		{
			positionInUniverse = position;
		}

		void OnTransitState(TransitState transitState)
		{
			if (!View.Visible) return;
			View.TimeScalar = transitState.RelativeTimeScalar;
		}
		#endregion
	}
}