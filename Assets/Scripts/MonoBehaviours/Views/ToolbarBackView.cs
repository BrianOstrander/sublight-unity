using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ToolbarBackView : View, IToolbarBackView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Vector3 rotation;
		[SerializeField]
		Transform rotationArea;
		[SerializeField]
		float animationTime;
		[SerializeField]
		TextMeshProUGUI backLabel;
		[SerializeField]
		float opacityInteractionThreshold;
		[SerializeField]
		CanvasGroup opacityGroup;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float AnimationTime { get { return animationTime; } }
		public string BackText { set { backLabel.text = value ?? string.Empty; } }
		public Action Click { set; private get; }

		protected override void OnOpacityStack(float opacity)
		{
			opacityGroup.alpha = opacity;
			opacityGroup.blocksRaycasts = opacityInteractionThreshold < opacity;
		}

		public override void Reset()
		{
			base.Reset();

			BackText = string.Empty;
			Click = ActionExtensions.Empty;
		}

		protected override void OnPrepare()
		{
			base.OnPrepare();

			rotationArea.localRotation = Quaternion.Euler(rotation);
		}

		#region Events
		public void OnClick()
		{
			if (Click != null) Click();
		}
		#endregion
	}

	public interface IToolbarBackView : IView
	{
		float AnimationTime { get; }
		string BackText { set; }
		Action Click { set; }
	}
}