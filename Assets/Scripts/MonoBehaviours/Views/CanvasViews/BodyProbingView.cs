using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class BodyProbingView : CanvasView, IBodyProbingView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		[SerializeField]
		XButton doneButton;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public string Description { set { descriptionLabel.text = value ?? string.Empty; } }
		public bool IsDone { set { doneButton.interactable = value; } }
		public Action BackClick { set; private get; }
		public Action DoneClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			Description = string.Empty;
			IsDone = false;
			DoneClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnDoneClick() { DoneClick(); }
		#endregion
	}

	public interface IBodyProbingView : ICanvasView
	{
		string Title { set; }
		string Description { set; }
		bool IsDone { set; }
		Action DoneClick { set; }
	}
}