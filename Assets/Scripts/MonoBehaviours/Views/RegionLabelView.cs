using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class RegionLabelView : View, IRegionLabelView
	{
		enum States
		{
			Unknown = 0,
			Idle = 10,
			Revealing = 20,
			Hiding = 30
		}

		[SerializeField]
		float transitionDuration;
		[SerializeField, Range(0f, 1f)]
		float transitionHideRatio;
		[SerializeField]
		AnimationCurve transitionRevealCurve;
		[SerializeField]
		AnimationCurve transitionHideCurve;
		[SerializeField]
		TextMeshProUGUI regionLabel;
		[SerializeField]
		CanvasGroup labelGroup;

		string currentText;
		string nextText;

		float transitionBeginOpacity;
		float transitionRemaining;
		States state = States.Idle;

		float regionOpacity;

		public float RegionOpacity
		{
			get { return regionOpacity; }
			set
			{
				regionOpacity = value;
				//UpdateOpacity();
			}
		}

		public void SetRegion(string text, bool instant = false)
		{
			text = text ?? string.Empty;

			if (instant)
			{
				currentText = text;
				nextText = text;
				transitionRemaining = 0f;
				regionLabel.text = text;
				regionLabel.alpha = string.IsNullOrEmpty(text) ? 0f : 1f;
				state = States.Idle;
				return;
			}

			if (text == nextText) return;

			switch (state)
			{
				case States.Idle:
					nextText = text;
					transitionBeginOpacity = 1f;
					transitionRemaining = transitionDuration;
					state = States.Hiding;
					break;
				case States.Revealing:
					currentText = nextText;
					nextText = text;
					transitionBeginOpacity = regionLabel.alpha;
					transitionRemaining = transitionDuration;
					state = States.Hiding;
					break;
				case States.Hiding:
					nextText = text;
					break;
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			labelGroup.alpha = opacity;
			//labelGroup.alpha = Opacity * RegionOpacity;
		}

		public override void Reset()
		{
			base.Reset();

			SetRegion(string.Empty, true);
		}

		#region Events
		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			if (state == States.Idle) return;

			transitionRemaining = Mathf.Max(0f, transitionRemaining - delta);
			var hideCutoff = transitionDuration * (1f - transitionHideRatio);
			var scalar = 0f;
			var newState = state;
			if (hideCutoff <= transitionRemaining)
			{
				newState = States.Hiding;
				scalar = (transitionRemaining - hideCutoff) / (transitionDuration - hideCutoff);
			}
			else
			{
				newState = States.Revealing;
				scalar = transitionRemaining / hideCutoff;
			}

			scalar = 1f - scalar;

			if (newState != state)
			{
				state = newState;
				switch (state)
				{
					case States.Revealing:
						currentText = nextText;
						regionLabel.text = currentText;
						break;
					default:
						Debug.LogError("Label transition is out of state: " + state);
						break;
				}
			}

			var curve = state == States.Revealing ? transitionRevealCurve : transitionHideCurve;

			regionLabel.alpha = curve.Evaluate(scalar);
		}
		#endregion
	}

	public interface IRegionLabelView : IView
	{
		float RegionOpacity { set; }
		void SetRegion(string text, bool instant = false);
	}
}