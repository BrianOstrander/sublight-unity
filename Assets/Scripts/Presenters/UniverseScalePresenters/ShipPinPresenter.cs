using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ShipPinPresenter : UniverseScalePresenter<IShipPinView>
	{
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public ShipPinPresenter(GameModel model, UniverseScales scale) : base(model, scale)
		{
			OnPosition(model.Ship.Value.Position.Value);
			scaleInUniverse = ScaleModel.Transform.Value.GetUniverseScale(Vector3.one * View.UnityScale);

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		#region Events
		void OnPosition(UniversePosition position)
		{
			positionInUniverse = position;
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}
		#endregion
	}
}