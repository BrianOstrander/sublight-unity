using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridScalePresenter : SystemFocusPresenter<IGridScaleView>
	{
		GameModel model;
		LanguageStringModel scaleText;

		float lastZoomTo = -1f;

		float opacityTarget;
		float opacityBegin;
		float opacityCurrent;
		float? opacityRemaining;

		public GridScalePresenter(
			GameModel model,
			LanguageStringModel scaleText
		)
		{
			this.model = model;
			this.scaleText = scaleText;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Heartbeat.Update += OnUpdate;
			model.FocusTransform.Changed += OnZoom;
			model.CelestialSystemState.Changed += OnCelestialSystemState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Heartbeat.Update -= OnUpdate;
			model.FocusTransform.Changed -= OnZoom;
			model.CelestialSystemState.Changed -= OnCelestialSystemState;
		}

		protected override void OnUpdateEnabled()
		{
			View.ScaleText = scaleText.Value.Value;
			OnZoom(model.FocusTransform.Value);
			OnText(model.FocusTransform.Value);
			View.PushOpacity(() => model.GridScaleOpacity.Value);
			OnCelestialSystemState(model.CelestialSystemStateLastSelected);
		}

		#region
		void OnUpdate(float delta)
		{
			if (!opacityRemaining.HasValue) return;

			opacityRemaining = Mathf.Max(0f, opacityRemaining.Value - delta);

			var scalar = 1f - (opacityRemaining.Value / View.SelectionFadeDuration);
			opacityCurrent = opacityBegin + ((opacityTarget - opacityBegin) * scalar);
			if (Mathf.Approximately(0f, opacityRemaining.Value)) opacityRemaining = null;

			model.GridScaleOpacity.Value = opacityCurrent;
			View.SetOpacityStale();
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

		void OnCelestialSystemState(CelestialSystemStateBlock state)
		{
			var newTarget = opacityTarget;

			switch (state.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					newTarget = 0f;
					break;
				case CelestialSystemStateBlock.States.Idle:
					newTarget = model.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected ? 0f : 1f;
					break;
				case CelestialSystemStateBlock.States.UnSelected:
					newTarget = 1f;
					break;
			}

			if (Mathf.Approximately(newTarget, opacityTarget)) return;

			opacityBegin = opacityCurrent;
			opacityTarget = newTarget;
			opacityRemaining = View.SelectionFadeDuration;
		}
		#endregion
	}
}