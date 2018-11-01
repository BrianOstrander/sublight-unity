using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridScalePresenter : FocusPresenter<IGridScaleView, SystemFocusDetails>
	{
		GameModel model;
		LanguageStringModel scaleText;

		float lastZoomTo = -1f;

		public GridScalePresenter(
			GameModel model,
			LanguageStringModel scaleText
		)
		{
			this.model = model;
			this.scaleText = scaleText;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			model.CameraTransform.Changed += OnCameraTransform;
			model.FocusTransform.Changed += OnZoom;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			model.CameraTransform.Changed -= OnCameraTransform;
			model.FocusTransform.Changed -= OnZoom;
		}

		protected override void OnUpdateEnabled()
		{
			View.ScaleText = scaleText.Value.Value;
			OnZoom(model.FocusTransform.Value);
			OnText(model.FocusTransform.Value);
		}

		#region
		void OnCameraTransform(CameraTransformRequest transform)
		{
			if (!View.Visible) return;
			View.Pitch = transform.PitchValue();
		}

		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnZoom(FocusTransform block)
		{
			switch(block.Zoom.Transition)
			{
				case TweenTransitions.NoChange: break;
				case TweenTransitions.ToLower:
				case TweenTransitions.ToHigher:
					if (!Mathf.Approximately(lastZoomTo, block.Zoom.End))
					{
						lastZoomTo = block.Zoom.End;
						OnText(block);
					}
					View.Zoom(block.Zoom.Current, block.Zoom.Progress, block.Zoom.Transition == TweenTransitions.ToLower);
					break;
				default:
					Debug.LogError("Unrecognized zoom transition: " + block.Zoom.Transition);
					break;
			}
			switch(block.NudgeZoom.Transition)
			{
				case TweenTransitions.NoChange: break;
				case TweenTransitions.PingPongToLower:
				case TweenTransitions.PingPongToHigher:
					View.Nudge(block.NudgeZoom.Begin, block.NudgeZoom.Progress, block.NudgeZoom.Transition == TweenTransitions.PingPongToHigher);
					break;
				default:
					Debug.LogError("Unrecognized nudge transition: " + block.NudgeZoom.Transition);
					break;
			}
		}

		void OnText(FocusTransform block)
		{
			View.ScaleNameText = block.ToScaleName.Value.Value;
			View.UnitCountText = block.GetToUnitCount();
			View.UnitTypeText = block.ToUnitType.Value.Value;
		}
		#endregion
	}
}