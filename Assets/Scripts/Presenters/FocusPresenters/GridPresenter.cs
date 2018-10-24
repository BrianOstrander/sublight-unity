using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridPresenter : FocusPresenter<IGridView, SystemFocusDetails>
	{
		GameModel model;
		GridInfoBlock info;
		bool ignoreNextZoom;

		public GridPresenter(
			GameModel model,
			GridInfoBlock info
		)
		{
			this.model = model;
			this.info = info;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture += OnCurrentScrollGesture;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture -= OnCurrentScrollGesture;
		}

		protected override void OnUpdateEnabled()
		{
			View.TilingRadius = 4f;
			View.UpdateZoomInfo = OnUpdateZoomInfo;
			ignoreNextZoom = true;
			View.UpdateZoom(model.Zoom);
			ignoreNextZoom = false;
		}

		#region
		void OnUpdateZoomInfo(ZoomInfoBlock block)
		{
			if (!ignoreNextZoom) model.Zoom.Value = block.Zoom;

			block.ScaleName = info.ScaleNames[Mathf.Min(info.ScaleNames.Length - 1, block.ScaleIndex)];

			switch (block.UnitType)
			{
				case GridUnitTypes.AstronomicalUnit: block.UnitName = info.AstronomicalUnit.Get(block.UnitAmountMinimum); break;
				case GridUnitTypes.LightYear: block.UnitName = info.LightYearUnit.Get(block.UnitAmountMinimum); break;
				default:
					Debug.LogError("Unrecognized grid unit: " + block.UnitType);
					break;
			}

			block.UnitAmountFormatted = OnUnitAmountFormatted;

			model.ZoomInfo.Value = block;
		}

		string OnUnitAmountFormatted()
		{
			var block = model.ZoomInfo.Value;

			var unitValue = block.UnitAmountMinimum;
			var suffix = string.Empty;

			if (block.UnitAmountMinimum < 1000f) suffix = string.Empty;
			else if (block.UnitAmountMinimum < 1000000)
			{
				suffix = info.ThousandUnit.Value.Value;
				unitValue /= 1000f;
			}
			else
			{
				suffix = info.MillionUnit.Value.Value;
				unitValue /= 1000000;
			}

			return ((int)unitValue) + suffix;
		}

		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnCurrentScrollGesture(ScrollGesture gesture)
		{
			if (!View.Visible || !View.Highlighted) return;

			View.UpdateZoom(model.Zoom, gesture.Current.y * gesture.TimeDelta);
		}

		#endregion
	}
}