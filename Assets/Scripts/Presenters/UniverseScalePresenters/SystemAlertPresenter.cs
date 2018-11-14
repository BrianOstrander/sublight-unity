using System;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class SystemAlertPresenter : UniverseScalePresenter<ISystemAlertView>
	{
		LanguageStringModel titleText;
		LanguageStringModel detailText;
		Action click;

		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public SystemAlertPresenter(
			GameModel model,
			UniversePosition positionInUniverse,
			LanguageStringModel titleText,
			LanguageStringModel detailText,
			Action click,
			UniverseScales scale
		) : base(model, scale)
		{

			scaleInUniverse = UniversePosition.One;
			this.positionInUniverse = positionInUniverse;
			this.titleText = titleText;
			this.detailText = detailText;
			this.click = click;

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
			View.TitleText = titleText.Value;
			View.DetailText = detailText.Value;
			View.Click = click;
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}

		void OnClick()
		{
			if (click != null) click();
		}
		#endregion
	}
}