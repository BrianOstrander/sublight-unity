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
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
		}

		#region Events
		void OnPosition(UniversePosition position)
		{
			positionInUniverse = position;
		}
		#endregion
	}
}