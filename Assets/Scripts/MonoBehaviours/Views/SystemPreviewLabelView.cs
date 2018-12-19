using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public struct PreviewBodyBlock
	{
		public int Index;
		public string Title;
		public string Description;
		public float Size;
		public XButtonColorBlock BodyColor;
		public XButtonColorBlock DropShadowColor;
		public XButtonColorBlock ShadowColor;
	}

	public struct PreviewSystemBlock
	{
		public UniversePosition Position;
		public string Title;
		public string Description;
		public PreviewBodyBlock[] Bodies;
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
		RectTransform labelsContainer;
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		CanvasGroup previewGroup;

		[SerializeField]
		SystemPreviewBodyLeaf bodyPrefab;
		[SerializeField]
		GameObject bodyArea;
		[SerializeField]
		Vector2 bodySizeRange;
		[SerializeField]
		AnimationCurve bodySizeCurve;
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
				previewGroup.alpha = 1f;
				state = States.Idle;
				ApplyPreview(block);
				return;
			}

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
					transitionBeginOpacity = previewGroup.alpha;
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
			group.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			bodyPrefab.Body.LocalStyle.Colors.NormalColor = Color.clear;
			bodyPrefab.DropShadow.LocalStyle.Colors.NormalColor = Color.clear;
			bodyPrefab.Shadow.LocalStyle.Colors.NormalColor = Color.clear;

			bodyPrefab.gameObject.SetActive(false);
			SetPreview(new PreviewSystemBlock(), true);
		}

		void ApplyPreview(PreviewSystemBlock block)
		{
			titleLabel.text = block.Title ?? string.Empty;
			descriptionLabel.text = block.Description ?? string.Empty;

			bodyArea.transform.ClearChildren<SystemPreviewBodyLeaf>();

			if (block.Bodies != null)
			{
				foreach (var body in block.Bodies)
				{
					var curr = bodyArea.InstantiateChild(bodyPrefab, setActive: true);
					curr.Layout.preferredWidth = bodySizeRange.x + ((bodySizeRange.y - bodySizeRange.x) * bodySizeCurve.Evaluate(body.Size));
					curr.Body.LocalStyle.Colors = body.BodyColor;
					curr.DropShadow.LocalStyle.Colors = body.DropShadowColor;
					curr.Shadow.LocalStyle.Colors = body.ShadowColor;
					curr.Button.ForceApplyState();
				}
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(labelsContainer);
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

			previewGroup.alpha = curve.Evaluate(scalar);
		}
		#endregion
	}

	public interface ISystemPreviewLabelView : IView
	{
		void SetPreview(PreviewSystemBlock block, bool instant = false);
	}
}