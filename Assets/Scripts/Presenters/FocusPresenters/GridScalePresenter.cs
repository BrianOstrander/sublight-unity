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
			model.FocusTransform.Changed += OnZoom;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			model.FocusTransform.Changed -= OnZoom;
		}

		protected override void OnUpdateEnabled()
		{
			View.ScaleText = scaleText.Value.Value;
			OnZoom(model.FocusTransform.Value);
		}

		#region
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnZoom(FocusTransform block)
		{
			switch(block.Zoom.Transition)
			{
				case TweenTransitions.ToLower:
				case TweenTransitions.ToHigher:
					View.ScaleNameText = block.ToScaleName.Value.Value;
					View.UnitCountText = block.GetToUnitCount();
					View.UnitTypeText = block.ToUnitType.Value.Value;
					View.Zoom(block.Zoom.Current, block.Zoom.Progress, block.Zoom.Transition == TweenTransitions.ToLower);
					break;
				default:
					Debug.LogError("Unrecognized zoom transition: " + block.Zoom.Transition);
					break;
			}
		}
		#endregion
	}
}