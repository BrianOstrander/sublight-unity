using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class WaypointPresenter : UniverseScalePresenter<IWaypointView>
	{
		WaypointModel waypoint;

		protected override UniversePosition PositionInUniverse { get { return waypoint.Location.Value.Position; } }

		public WaypointPresenter(
			GameModel model,
			WaypointModel waypoint,
			UniverseScales scale
		) : base(model, scale)
		{
			this.waypoint = waypoint;

			ScaleModel.Transform.Changed += OnScaleTransform;
			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			ScaleModel.Transform.Changed -= OnScaleTransform;
			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		protected override void OnShowView()
		{
			View.HintVisible = Scale == UniverseScales.Local;
			View.SetDetails(waypoint.Name.Value, null, null);

			View.PushOpacity(() => ScaleModel.Opacity.Value);
		}

		#region Events
		void OnScaleTransform(UniverseTransform universeTransform)
		{
			if (!View.Visible) return;

		}

		void OnScaleOpacity(float opacity)
		{
			if (!View.Visible) return;
			View.SetOpacityStale();
		}
		#endregion
	}
}