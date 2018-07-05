using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class BodyProbeDetailView : CanvasView, IBodyProbeDetailView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public string Description { set { descriptionLabel.text = value ?? string.Empty; } }
		public Action BackClick { set; private get; }
		public Action LaunchClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			Description = string.Empty;
			BackClick = ActionExtensions.Empty;
			LaunchClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnBackClick() { BackClick(); }
		public void OnLaunchClick() { LaunchClick(); }
		#endregion
	}

	public interface IBodyProbeDetailView : ICanvasView
	{
		string Title { set; }
		string Description { set; }
		Action BackClick { set; }
		Action LaunchClick { set; }
	}
}