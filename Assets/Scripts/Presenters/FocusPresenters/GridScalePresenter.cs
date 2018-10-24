using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridScalePresenter : FocusPresenter<IGridScaleView, SystemFocusDetails>
	{
		GameModel model;
		LanguageStringModel scaleText;

		public GridScalePresenter(
			GameModel model,
			LanguageStringModel scaleText
		)
		{
			this.model = model;
			this.scaleText = scaleText;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			model.ZoomInfo.Changed += OnZoomInfo;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			model.ZoomInfo.Changed -= OnZoomInfo;
		}

		protected override void OnUpdateEnabled()
		{
			View.ScaleText = scaleText.Value.Value;
			View.ZoomInfo = model.ZoomInfo.Value;
		}

		#region
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnZoomInfo(ZoomInfoBlock zoomInfo)
		{
			if (!View.Visible) return;

			View.ZoomInfo = zoomInfo;
		}
		#endregion
	}
}