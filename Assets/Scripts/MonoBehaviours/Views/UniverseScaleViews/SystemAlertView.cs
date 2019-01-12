using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class SystemAlertView : UniverseScaleView, ISystemAlertView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float yHeight;
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		Transform verticalLookAtArea;
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI detailLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public string TitleText { set { titleLabel.text = value ?? string.Empty; } }
		public string DetailText { set { detailLabel.text = value ?? string.Empty; } }
		public Action Click { set; private get; }

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + App.V.CameraForward.FlattenY());
			verticalLookAtArea.LookAt(verticalLookAtArea.position + App.V.CameraForward);
		}

		public override void Reset()
		{
			base.Reset();

			TitleText = string.Empty;
			DetailText = string.Empty;
			Click = ActionExtensions.Empty;
		}

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{
			lookAtArea.position = position.NewY(position.y + yHeight);
		}

		#region Events
		public void OnClick()
		{
			if (Click != null) Click();
		}
		#endregion
	}

	public interface ISystemAlertView : IUniverseScaleView
	{
		string TitleText { set; }
		string DetailText { set; }
		Action Click { set; }
	}
}