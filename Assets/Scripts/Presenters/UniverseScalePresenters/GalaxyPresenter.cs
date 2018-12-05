using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GalaxyPresenter : UniverseScalePresenter<IGalaxyView>
	{
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public GalaxyPresenter(GameModel model) : base(model, UniverseScales.Galactic)
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
			var transform = Model.ActiveScale.Value.Transform.Value;
			View.SetGalaxy(Model.Galaxy.FullPreview, transform.UnityOrigin, transform.UnityRadius);
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}
		#endregion
	}
}