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
			scaleInUniverse = model.Context.Galaxy.GalaxySize;
			positionInUniverse = model.Context.Galaxy.GalaxyOrigin;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
		}

		#region Events
		protected override void OnShowView()
		{
			var transform = Model.Context.ActiveScale.Value.Transform.Value;
			//SetGrid(transform.UnityOrigin, transform.UnityRadius);
			View.SetGalaxy(Model.Context.Galaxy.FullPreview);
		}
		#endregion
	}
}