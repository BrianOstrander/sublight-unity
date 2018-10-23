using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridScalePresenter : FocusPresenter<IGridScaleView, SystemFocusDetails>
	{
		GameModel model;
		LanguageStringModel unit;
		LanguageStringModel[] scales;

		public GridScalePresenter(
			GameModel model,
			LanguageStringModel unit,
			params LanguageStringModel[] scales
		)
		{
			this.model = model;
			this.unit = unit;
			this.scales = scales;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			model.Zoom.Changed += OnZoom;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			model.Zoom.Changed -= OnZoom;
		}

		protected override void OnUpdateEnabled()
		{
			View.UnitText = unit.Value.Value;
			View.SetZoom(model.Zoom, GetScaleText(model.Zoom));
		}

		string GetScaleText(float zoom)
		{
			var index = Mathf.FloorToInt(Mathf.Min(zoom, 4.9f));
			if (scales.Length <= index)
			{
				Debug.LogError("Zoom is too high for provided scale units");
				return null;
			}
			return scales[index].Value.Value;
		}

		#region
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnZoom(float zoom)
		{
			if (!View.Visible) return;

			View.SetZoom(model.Zoom, GetScaleText(model.Zoom));
		}

		#endregion
	}
}