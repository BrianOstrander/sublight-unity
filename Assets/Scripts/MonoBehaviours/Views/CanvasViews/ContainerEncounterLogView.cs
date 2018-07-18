using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class ContainerEncounterLogView : CanvasView, IContainerEncounterLogView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		XButton doneButton;
		[SerializeField]
		XButton nextButton;
		[SerializeField]
		Transform entryArea;
		[SerializeField]
		Scrollbar scrollbar;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public float Scroll { set { scrollbar.value = Mathf.Clamp01(value); } }
		public bool DoneEnabled { set { doneButton.interactable = value; } }
		public bool NextEnabled { set { nextButton.interactable = value; } }
		public Action DoneClick { set; private get; }
		public Action NextClick { set; private get; }

		public Transform EntryArea { get { return entryArea; } }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			Scroll = 1f;
			DoneEnabled = false;
			NextEnabled = false;
			DoneClick = ActionExtensions.Empty;
			NextClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnDoneClick() { DoneClick(); }
		public void OnNextClick() { NextClick(); }
		#endregion
	}

	public interface IContainerEncounterLogView : ICanvasView
	{
		string Title { set; }
		float Scroll { set; }
		bool DoneEnabled { set; }
		bool NextEnabled { set; }
		Action DoneClick { set; }
		Action NextClick { set; }

		Transform EntryArea { get; }
	}
}