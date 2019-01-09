using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class SystemPreviewLabelPresenter : FocusPresenter<ISystemPreviewLabelView, SystemFocusDetails>
	{
		GameModel model;
		SystemPreviewLanguageBlock language;
		SystemModel currentSystem;

		float? lastOpacity;
		float opacityDeltaScalar = 1f / 0.3f;

		public SystemPreviewLabelPresenter(
			GameModel model,
			SystemPreviewLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			App.Heartbeat.Update += OnUpdate;

			model.GridScaleOpacity.Changed += OnOpacityStale;
			model.CelestialSystemState.Changed += OnCelestialSystemState;
			model.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;

			model.GridScaleOpacity.Changed -= OnOpacityStale;
			model.CelestialSystemState.Changed -= OnCelestialSystemState;
			model.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateDisabled()
		{
			lastOpacity = null;
		}

		protected override void OnUpdateEnabled()
		{
			lastOpacity = 0f;
			View.PushOpacity(() => lastOpacity.Value);
			View.PushOpacity(() => 1f - model.GridScaleOpacity.Value);
			View.PushOpacity(() => model.TransitState.Value.State == TransitState.States.Active ? 0f : 1f);
			if (currentSystem != null) View.SetPreview(GetBlock(currentSystem), true);
		}
			
		PreviewSystemBlock GetBlock(SystemModel system)
		{
			var result = new PreviewSystemBlock();
			if (system == null) return result;

			result.Title = system.Name.Value;
			result.Description = language.PrimaryClassifications[system.PrimaryClassification.Value].Value.Value + " - " + system.SecondaryClassification.Value;

			var otherColor = new Color(0.757f, 0.757f, 0.757f);

			result.Bodies = new PreviewBodyBlock[] {
				new PreviewBodyBlock {
					Index = 0,
					Title = "",
					Description = "",
					Size = system.IconScale.Value,
					BodyColor = GetBodyColor(system.IconColor.Value),
					DropShadowColor = GetDropShadowColor(system.IconColor.Value),
					ShadowColor = GetShadowColor(system.IconColor.Value)
				},
				new PreviewBodyBlock {
					Index = 1,
					Title = "",
					Description = "",
					Size = system.IconScale.Value * 0.5f,
					BodyColor = GetBodyColor(otherColor),
					DropShadowColor = GetDropShadowColor(otherColor),
					ShadowColor = GetShadowColor(otherColor)
				},
				new PreviewBodyBlock {
					Index = 2,
					Title = "",
					Description = "",
					Size = system.IconScale.Value * 0.8f,
					BodyColor = GetBodyColor(otherColor),
					DropShadowColor = GetDropShadowColor(otherColor),
					ShadowColor = GetShadowColor(otherColor)
				},
				new PreviewBodyBlock {
					Index = 3,
					Title = "",
					Description = "",
					Size = system.IconScale.Value * 0.3f,
					BodyColor = GetBodyColor(otherColor),
					DropShadowColor = GetDropShadowColor(otherColor),
					ShadowColor = GetShadowColor(otherColor)
				}
			};

			return result;
		}

		XButtonColorBlock GetBodyColor(Color color)
		{
			var result = new XButtonColorBlock();

			if (Mathf.Approximately(color.GetS(), 0f)) color = color.NewV(0.6f);
			else color = color.NewV(0.8f);

			result.DisabledColor = color;
			result.NormalColor = color;
			result.HighlightedColor = color;
			result.PressedColor = color;

			return result;
		}

		XButtonColorBlock GetDropShadowColor(Color color)
		{
			var result = new XButtonColorBlock();

			color = color.NewV(0.45f);

			result.DisabledColor = color.NewA(0f);
			result.NormalColor = color.NewA(0f);
			result.HighlightedColor = color;
			result.PressedColor = color;

			return result;
		}

		XButtonColorBlock GetShadowColor(Color color)
		{
			var result = new XButtonColorBlock();

			if (Mathf.Approximately(color.GetS(), 0f)) color = color.NewV(0.62f);
			else color = color.NewS(color.GetS() - 0.25f).NewV(0.7f);

			result.DisabledColor = color.NewA(0f);
			result.NormalColor = color.NewA(0f);
			result.HighlightedColor = color;
			result.PressedColor = color;

			return result;
		}

		#region Events
		void OnUpdate(float delta)
		{
			if (!lastOpacity.HasValue || Mathf.Approximately(1f, lastOpacity.Value)) return;

			lastOpacity = Mathf.Min(1f, lastOpacity.Value + (delta * opacityDeltaScalar));
			View.SetOpacityStale();
		}

		void OnOpacityStale(float opacity)
		{
			View.SetOpacityStale();
		}

		void OnCelestialSystemState(CelestialSystemStateBlock state)
		{
			var newSystem = currentSystem;

			switch (state.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					newSystem = state.System;
					break;
				case CelestialSystemStateBlock.States.Idle:
					if (model.CelestialSystemStateLastSelected.Value.System != null)
					{
						newSystem = model.CelestialSystemStateLastSelected.Value.System;
					}
					break;
				case CelestialSystemStateBlock.States.UnSelected:
					newSystem = null;
					break;
			}

			if (newSystem == currentSystem) return;

			currentSystem = newSystem;
			View.SetPreview(GetBlock(currentSystem));
		}

		void OnTransitState(TransitState transitState)
		{
			if (!View.Visible) return;

			switch (transitState.State)
			{
				case TransitState.States.Request:
				case TransitState.States.Complete:
					View.SetOpacityStale();
					break;
			}
		}
		#endregion
	}
}