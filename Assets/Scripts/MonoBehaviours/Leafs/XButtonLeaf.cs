using UnityEngine;
using UnityEngine.UI;
using SelectionState = LunraGames.SpaceFarm.XButton.SelectionState;

namespace LunraGames.SpaceFarm
{
	public class XButtonLeaf : MonoBehaviour
	{
		[HideInInspector]
		public XButtonStyleObject GlobalStyle;
		[HideInInspector]
		public XButtonStyleBlock LocalStyle = XButtonStyleBlock.Default;
		[SerializeField]
		GameObject[] targetToggles = new GameObject[0];
		[SerializeField]
		Transform[] targetTransforms = new Transform[0];
		[SerializeField]
		Graphic[] targetGraphics = new Graphic[0];

		XButtonStyleBlock Style { get { return GlobalStyle == null ? LocalStyle : GlobalStyle.Block; } }

		Color lastColorUpdate = Color.white;
		bool lastToggleActiveUpdate = true;

		Color lastColor = Color.white;
		bool lastToggleActive = true;

		public void CacheState()
		{
			lastColor = lastColorUpdate;
			lastToggleActive = lastToggleActiveUpdate;
		}

		public void UpdateState(SelectionState state, float scalar = 1, bool interactable = true)
		{
			var distScalePercent = 1f;
			if (Application.isPlaying && Camera.main != null) distScalePercent = 1f - Mathf.Clamp01(Vector3.Dot((transform.position - Camera.main.transform.position).normalized, App.Callbacks.LastPointerOrientation.Forward));

			float currScale = (1f + (Style.DistanceScaleIntensity.Evaluate(1f - distScalePercent)));

			if (state == SelectionState.Pressed) currScale += Style.ClickScaleDelta.Evaluate(scalar);

			var currPushback = Style.ClickDistanceDelta.Evaluate(state == SelectionState.Highlighted ? scalar : 0f);

			foreach (var targetTransform in targetTransforms)
			{
				if (targetTransform == null) continue;
				targetTransform.localScale = Vector3.one * currScale;
				targetTransform.localPosition = targetTransform.localPosition.NewZ(currPushback);
			}

			var tintColor = Color.white;
			var togglesActive = true;

			if (interactable)
			{
				switch (state)
				{
					case SelectionState.Normal:
						tintColor = Style.Colors.NormalColor;
						togglesActive = Style.Toggles.ActiveOnNormal;
						break;
					case SelectionState.Highlighted:
						tintColor = Style.Colors.HighlightedColor;
						togglesActive = Style.Toggles.ActiveOnHighlighted;
						break;
					case SelectionState.Pressed:
						tintColor = Style.Colors.PressedColor;
						togglesActive = Style.Toggles.ActiveOnPressed;
						break;
				}
			}
			else
			{
				tintColor = Style.Colors.DisabledColor;
				togglesActive = Style.Toggles.ActiveOnDisabled;
			}

			var color = Color.Lerp(lastColor, tintColor, scalar);

			foreach (var targetGraphic in targetGraphics)
			{
				if (targetGraphic == null) continue;
				targetGraphic.color = color;
			}

			if (!Mathf.Approximately(scalar, 1f)) togglesActive = lastToggleActive || togglesActive;

			foreach (var targetToggle in targetToggles)
			{
				if (targetToggle == null) continue;
				targetToggle.SetActive(togglesActive);
			}

			lastColorUpdate = color;
			lastToggleActiveUpdate = togglesActive;
		}
	}
}