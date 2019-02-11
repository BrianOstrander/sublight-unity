using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class BodiesDisabledPresenter : UniverseScalePresenter<IBodiesDisabledView>
	{
		LanguageStringModel title;
		LanguageStringModel description;

		protected override UniversePosition PositionInUniverse { get { return Model.Ship.Position.Value; } }

		public BodiesDisabledPresenter(
			GameModel model,
			LanguageStringModel title,
			LanguageStringModel description
		) : base(model, UniverseScales.System)
		{
			this.title = title;
			this.description = description;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
		}

		protected override void OnShowView()
		{
			View.SetText(title.Value.Value, description.Value.Value);
		}

		#region Events

		#endregion
	}
}