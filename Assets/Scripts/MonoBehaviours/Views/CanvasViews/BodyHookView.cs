using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class BodyHookView : CanvasView, IBodyHookView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		[SerializeField]
		TextMeshProUGUI rationsLabel;
		[SerializeField]
		TextMeshProUGUI fuelLabel;
		[SerializeField]
		XButton launchButton;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public string Description { set { descriptionLabel.text = value ?? string.Empty; } }
		public float Rations { set { rationsLabel.text = Strings.Rations(value); } }
		public float Fuel { set { fuelLabel.text = Strings.Fuel(value); } }
		public bool LaunchEnabled { set { launchButton.interactable = value; } }
		public Action BackClick { set; private get; }
		public Action LaunchClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			Description = string.Empty;
			Rations = 0f;
			Fuel = 0f;
			LaunchEnabled = false;
			BackClick = ActionExtensions.Empty;
			LaunchClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnBackClick() { BackClick(); }
		public void OnLaunchClick() { LaunchClick(); }
		#endregion
	}

	public interface IBodyHookView : ICanvasView
	{
		string Title { set; }
		string Description { set; }
		float Rations { set; }
		float Fuel { set; }
		bool LaunchEnabled { set; }
		Action BackClick { set; }
		Action LaunchClick { set; }
	}
}