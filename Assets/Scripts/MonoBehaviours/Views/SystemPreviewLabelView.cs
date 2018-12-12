using System.Linq;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public struct PreviewBodyBlock
	{
		public int Index;
		public string Title;
		public string Description;
		public float Size;
		public XButtonColorBlock Color;
	}

	public struct PreviewSystemBlock
	{
		public UniversePosition Position;
		public string Title;
		public string Description;
		public PreviewBodyBlock[] Bodies;

		/// <summary>
		/// Too lazy to override equality comparisions, deal with this.
		/// </summary>
		public bool Equals(PreviewSystemBlock other)
		{
			return Position == other.Position;
		}
	}

	public class SystemPreviewLabelView : View, ISystemPreviewLabelView
	{
		enum States
		{
			Unknown = 0,
			Idle = 10,
			Revealing = 20,
			Hiding = 30
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float transitionDuration;
		[SerializeField, Range(0f, 1f)]
		float transitionHideRatio;
		[SerializeField]
		AnimationCurve transitionRevealCurve;
		[SerializeField]
		AnimationCurve transitionHideCurve;
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		[SerializeField]
		CanvasGroup blockGroup;
		[SerializeField]
		CanvasGroup previewGroup;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		PreviewSystemBlock currentBlock;
		PreviewSystemBlock nextBlock;

		float transitionBeginOpacity;
		float transitionRemaining;
		States state = States.Idle;

		public void SetPreview(PreviewSystemBlock block, bool instant = false)
		{
			if (instant)
			{
				currentBlock = block;
				nextBlock = block;
				transitionRemaining = 0f;
				ApplyPreview(block);
				blockGroup.alpha = 1f;
				state = States.Idle;
				return;
			}

			if (block.Equals(nextBlock)) return;

			switch (state)
			{
				case States.Idle:
					nextBlock = block;
					transitionBeginOpacity = 1f;
					transitionRemaining = transitionDuration;
					state = States.Hiding;
					break;
				case States.Revealing:
					currentBlock = nextBlock;
					nextBlock = block;
					transitionBeginOpacity = blockGroup.alpha;
					transitionRemaining = transitionDuration;
					state = States.Hiding;
					break;
				case States.Hiding:
					nextBlock = block;
					break;
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			blockGroup.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			SetPreview(new PreviewSystemBlock(), true);
		}

		void ApplyPreview(PreviewSystemBlock block)
		{
			titleLabel.text = block.Title ?? string.Empty;
			descriptionLabel.text = block.Description ?? string.Empty;
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
						currentBlock = nextBlock;
						ApplyPreview(currentBlock);
						break;
					default:
						Debug.LogError("Label transition is out of state: " + state);
						break;
				}
			}

			var curve = state == States.Revealing ? transitionRevealCurve : transitionHideCurve;

			blockGroup.alpha = curve.Evaluate(scalar);
		}
		#endregion
	}

	public interface ISystemPreviewLabelView : IView
	{
		void SetPreview(PreviewSystemBlock block, bool instant = false);
	}
}