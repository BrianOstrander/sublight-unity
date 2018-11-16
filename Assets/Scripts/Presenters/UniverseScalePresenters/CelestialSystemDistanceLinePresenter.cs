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

		void SetPoints(UniversePosition? end)
		{
			var noShow = !end.HasValue;

			View.Opacity = noShow ? 0f : 1f;

			var transform = ScaleModel.Transform.Value;

			end = end ?? Model.Ship.Value.Position;

			View.SetPoints(transform.GetUnityPosition(Model.Ship.Value.Position), transform.GetUnityPosition(end.Value));
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

			UniversePosition? end = null;
			if (Model.CelestialSystemStateLastSelected.State == CelestialSystemStateBlock.States.Selected)
			{
				switch (Model.CelestialSystemState.Value.State)
				{
					case CelestialSystemStateBlock.States.Highlighted:
						end = Model.CelestialSystemState.Value.Position;
						break;
					default:
						end = Model.CelestialSystemStateLastSelected.Position;
						break;
				}
			}
			else
			{
				switch (Model.CelestialSystemState.Value.State)
				{
					case CelestialSystemStateBlock.States.Highlighted:
						end = Model.CelestialSystemState.Value.Position;
						break;
				}
			}

			SetPoints(end);
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			UniversePosition? end = null;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
					break;
				case CelestialSystemStateBlock.States.Highlighted:
				case CelestialSystemStateBlock.States.Selected:
					end = block.Position;
					break;
				default:
					switch (Model.CelestialSystemStateLastSelected.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							end = Model.CelestialSystemStateLastSelected.Position;
							break;
					}
					break;
			}

			SetPoints(end);
		}
		#endregion
	}
}