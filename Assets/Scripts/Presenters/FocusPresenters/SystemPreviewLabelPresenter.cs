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

		public SystemPreviewLabelPresenter(
			GameModel model,
			SystemPreviewLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			model.GridScaleOpacity.Changed += OnOpacityStale;
			model.CelestialSystemState.Changed += OnCelestialSystemState;
		}

		protected override void OnUnBind()
		{
			model.GridScaleOpacity.Changed -= OnOpacityStale;
			model.CelestialSystemState.Changed -= OnCelestialSystemState;
		}

		protected override void OnUpdateEnabled()
		{
			View.PushOpacity(() => 1f - model.GridScaleOpacity.Value);
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

			result.NormalColor = color;
			result.HighlightedColor = color;
			result.PressedColor = color;

			return result;
		}

		XButtonColorBlock GetDropShadowColor(Color color)
		{
			var result = new XButtonColorBlock();

			color = color.NewV(0.45f);

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

			result.NormalColor = color.NewA(0f);
			result.HighlightedColor = color;
			result.PressedColor = color;

			return result;
		}

		#region Events
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
					if (model.CelestialSystemStateLastSelected.System != null)
					{
						newSystem = model.CelestialSystemStateLastSelected.System;
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
		#endregion
	}
}