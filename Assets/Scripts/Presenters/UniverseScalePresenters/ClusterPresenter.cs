using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ClusterPresenter : UniverseScalePresenter<IClusterView>
	{
		GalaxyInfoModel galaxy;
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public ClusterPresenter(GameModel model, GalaxyInfoModel galaxy) : base(model, UniverseScales.Cluster)
		{
			this.galaxy = galaxy;
			scaleInUniverse = galaxy.GalaxySize;
			positionInUniverse = galaxy.GalaxyOrigin;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		#region Events
		protected override void OnShowView()
		{
			var transform = Model.ActiveScale.Transform.Value;
			View.SetGalaxy(galaxy.FullPreview, transform.UnityOrigin, transform.UnityRadius);
			View.GalaxyName = galaxy.Name;
			View.GalaxyNormal = galaxy.UniverseNormal;
			View.AlertHeightMultiplier = galaxy.AlertHeightMultiplier;
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}
		#endregion
	}
}