using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ClusterPresenter : UniverseScalePresenter<IClusterView>
	{
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public ClusterPresenter(GameModel model) : base(model, UniverseScales.Cluster)
		{
			scaleInUniverse = model.Galaxy.GalaxySize;
			positionInUniverse = model.Galaxy.GalaxyOrigin;

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
			View.SetGalaxy(Model.Galaxy.FullPreview, transform.UnityOrigin, transform.UnityRadius);
			View.GalaxyName = Model.Galaxy.Name.Value;
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}
		#endregion
	}
}