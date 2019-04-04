using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class RingTransitPresenter : UniverseScalePresenter<IRingTransitView>
	{
		protected override UniversePosition PositionInUniverse { get { return Model.Ship.Position.Value; } }

		UniversePosition scaleInUniverse;
		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }

		public RingTransitPresenter(GameModel model, UniverseScales scale) : base(model, scale)
		{
			Model.Context.TransitState.Changed += OnTransitState;
			Model.Context.Grid.HazardOffset.Changed += OnGridHazardOffset;
			//Model.Ship.Range.Changed += OnTransitRange;

			//OnTransitRange(Model.Ship.Range);
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Context.TransitState.Changed -= OnTransitState;
			Model.Context.Grid.HazardOffset.Changed -= OnGridHazardOffset;
			//Model.Ship.Range.Changed -= OnTransitRange;
		}

		protected override void OnShowView()
		{
			View.GridHazardOffset = Model.Context.Grid.HazardOffset.Value;
		}

		#region Events
		void OnTransitState(TransitState transitState)
		{
			View.AnimationProgress = transitState.AnimationProgress;
		}

		void OnTransitRange(TransitRange transitRange)
		{
			scaleInUniverse = new UniversePosition(new Vector3(transitRange.Total, transitRange.Total, transitRange.Total));
		}

		void OnGridHazardOffset(float offset)
		{
			if (!View.Visible) return;

			View.GridHazardOffset = offset;
		}
		#endregion
	}
}