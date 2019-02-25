using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class RingTransitPresenter : UniverseScalePresenter<IRingTransitView>
	{
		protected override UniversePosition PositionInUniverse { get { return Model.Ship.Position.Value; } }

		public RingTransitPresenter(GameModel model, UniverseScales scale) : base(model, scale)
		{
			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Context.TransitState.Changed -= OnTransitState;
		}

		protected override void OnShowView()
		{

		}

		#region Events
		void OnTransitState(TransitState transitState)
		{

		}
		#endregion
	}
}