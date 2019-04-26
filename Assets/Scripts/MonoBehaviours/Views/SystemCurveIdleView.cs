using UnityEngine;
using UnityEngine.UI;

using CurvedUI;

namespace LunraGames.SubLight.Views
{
	public class SystemCurveIdleView : View, ISystemCurveIdleView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		RectTransform canvasAnchor;
		[SerializeField]
		CurvedUIRaycaster raycaster;

		[Header("Mood")]
		[SerializeField]
		Gradient moodGradientIcon;
		[SerializeField]
		Gradient moodGradientLabel;
		[SerializeField]
		Gradient moodGradientLabelBackground;
		[SerializeField]
		Gradient moodGradientProgressTop;
		[SerializeField]
		Gradient moodGradientProgressBottom;
		[SerializeField]
		CurveRange moodHashAlphaCurve = CurveRange.Normal;

		[SerializeField]
		SystemCurveIdleMoodLeaf moodInstance;
		[SerializeField]
		Graphic[] moodGraphicIcons;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public override void Reset()
		{
			base.Reset();

			SetMood(string.Empty, 0.5f, 0.5f);

			InputHack();
		}

		public void SetMood(string title, float valueTop, float valueBottom)
		{
			var differenceDelta = (valueTop - valueBottom) / 1f; // Value from -1.0 to 1.0
			var difference = 1f - (0.5f + (differenceDelta * 0.5f)); // Value from 0.0 to 1.0 where equivelent values are 0.5

			var hashAlpha = moodHashAlphaCurve.Evaluate(Mathf.Abs(differenceDelta));
			var labelColor = moodGradientLabel.Evaluate(difference);

			var iconColor = moodGradientIcon.Evaluate(difference);
			foreach (var graphic in moodGraphicIcons) graphic.color = iconColor;

			SetMoodSlider(
				valueTop,
				difference,
				moodInstance.ProgressTop,
				valueTop < valueBottom ? 0f : hashAlpha,
				moodGradientProgressTop
			);
			SetMoodSlider(
				valueBottom,
				difference,
				moodInstance.ProgressBottom,
				valueBottom < valueTop ? 0f : hashAlpha,
				moodGradientProgressBottom
			);
		}

		void SetMoodSlider(
			float value,
			float difference,
			SystemCurveIdleMoodProgressLeaf instance,
			float hashAlpha,
			Gradient sliderGradient
		)
		{
			instance.HashGroup.alpha = hashAlpha;
			var color = sliderGradient.Evaluate(difference);
			foreach (var graphic in instance.PrimaryColors) graphic.color = color;
			instance.Slider.sizeDelta = new Vector2(
				instance.Range.Evaluate(value) + instance.RangeOffset,
				instance.Slider.sizeDelta.y
			);
		}

		public void InputHack()
		{
			// Idk but this fixes weird stuff.
			raycaster.enabled = false;
			raycaster.enabled = true;
		}

		public float testOffsetSpeed = 1f;
		float testOffsetTop;
		float testOffsetBottom;
		bool testReversedTop;
		bool testReversedBottom;

		bool testReverse;

		public void TestButton()
		{
			testOffsetTop = NumberDemon.DemonUtility.NextFloat;
			testOffsetBottom = NumberDemon.DemonUtility.NextFloat;
			testReversedTop = NumberDemon.DemonUtility.NextBool;
			testReversedBottom = NumberDemon.DemonUtility.NextBool;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			testOffsetTop = (testOffsetTop + (delta * testOffsetSpeed));
			testOffsetBottom = (testOffsetBottom + (delta * testOffsetSpeed));

			if (testOffsetTop < 0f)
			{
				testOffsetTop = 1f;
				testReversedTop = !testReversedTop;
			}
			else if (1f < testOffsetTop)
			{
				testOffsetTop = 0f;
				testReversedTop = !testReversedTop;
			}

			if (testOffsetBottom < 0f)
			{
				testOffsetBottom = 1f;
				testReversedBottom = !testReversedBottom;
			}
			else if (1f < testOffsetBottom)
			{
				testOffsetBottom = 0f;
				testReversedBottom = !testReversedBottom;
			}


			SetMood(
				string.Empty,
				testReversedTop ? (1f - testOffsetTop) : testOffsetTop,
				testReversedBottom ? (1f - testOffsetBottom) : testOffsetBottom
			);
		}
	}

	public interface ISystemCurveIdleView : IView
	{
		void SetMood(string title, float valueTop, float valueBottom);

		void InputHack();
	}
}