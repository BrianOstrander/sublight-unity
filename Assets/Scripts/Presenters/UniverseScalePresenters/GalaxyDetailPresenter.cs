using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GalaxyDetailPresenter : UniverseScalePresenter<IGalaxyDetailView>
	{
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public GalaxyDetailPresenter(GameModel model, UniverseScales scale) : base(model, scale)
		{
			scaleInUniverse = model.Galaxy.GalaxySize;
			positionInUniverse = model.Galaxy.GalaxyOrigin;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
		}

		#region Events
		protected override void OnShowView()
		{
			var transform = Model.ActiveScale.Value.Transform.Value;
			View.SetGalaxy(Model.Galaxy.FullPreview, Model.Galaxy.Details, transform.UnityOrigin, transform.UnityRadius);
		}
		#endregion
	}
}