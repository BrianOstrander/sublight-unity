using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CelestialSystemDistanceLinePresenter : UniverseScalePresenter<ICelestialSystemDistanceLineView>
	{
		UniversePosition positionInUniverse;

		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public CelestialSystemDistanceLinePresenter(
			GameModel model,
			UniverseScales scale
		) : base(model, scale)
		{
			positionInUniverse = Model.Ship.Value.Position;

			Model.Ship.Value.Position.Changed += OnShipPosition;
			Model.CelestialSystemState.Changed += OnCelestialSystemState;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Ship.Value.Position.Changed -= OnShipPosition;
			Model.CelestialSystemState.Changed -= OnCelestialSystemState;

			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		#region Events
		void OnShipPosition(UniversePosition position)
		{
			positionInUniverse = position;
		}

		protected override void OnShowView()
		{
			View.WorldOrigin = ScaleModel.Transform.Value.UnityOrigin;
			View.WorldRadius = ScaleModel.Transform.Value.UnityRadius;

			UniversePosition? endPosition = null;
			if (Model.CelestialSystemStateLastSelected.State == CelestialSystemStateBlock.States.Selected)
			{
				switch (Model.CelestialSystemState.Value.State)
				{
					case CelestialSystemStateBlock.States.Highlighted:
						endPosition = Model.CelestialSystemState.Value.Position;
						break;
					default:
						endPosition = Model.CelestialSystemStateLastSelected.Position;
						break;
				}
			}
			else
			{
				switch (Model.CelestialSystemState.Value.State)
				{
					case CelestialSystemStateBlock.States.Highlighted:
						endPosition = Model.CelestialSystemState.Value.Position;
						break;
				}
			}
			var noShow = !endPosition.HasValue;

			View.Opacity = noShow ? 0f : 1f;

			var transform = ScaleModel.Transform.Value;

			endPosition = endPosition ?? Model.Ship.Value.Position;

			View.SetPoints(transform.GetUnityPosition(Model.Ship.Value.Position), transform.GetUnityPosition(endPosition.Value));
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			UniversePosition? endPosition = null;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
					break;
				case CelestialSystemStateBlock.States.Highlighted:
				case CelestialSystemStateBlock.States.Selected:
					endPosition = block.Position;
					break;
				default:
					switch (Model.CelestialSystemStateLastSelected.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							endPosition = Model.CelestialSystemStateLastSelected.Position;
							break;
					}
					break;
			}

			var noShow = !endPosition.HasValue;

			View.Opacity = noShow ? 0f : 1f;

			var transform = ScaleModel.Transform.Value;

			endPosition = endPosition ?? Model.Ship.Value.Position;

			View.SetPoints(transform.GetUnityPosition(Model.Ship.Value.Position), transform.GetUnityPosition(endPosition.Value));

			//if (OnCelestialSystemStateProcess(block.Position.Equals(positionInUniverse), block))
			//{
			//	if (View.Visible) ApplyStates();
			//}
		}
		#endregion
	}
}