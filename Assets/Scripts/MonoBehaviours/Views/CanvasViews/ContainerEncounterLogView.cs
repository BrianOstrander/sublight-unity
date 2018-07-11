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
		Transform entryArea;
		[SerializeField]
		Scrollbar scrollbar;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public float Scroll { set { scrollbar.value = Mathf.Clamp01(value); } }
		public bool DoneEnabled { set { doneButton.interactable = value; } }
		public Action DoneClick { set; private get; }

		public Transform EntryArea { get { return entryArea; } }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			Scroll = 1f;
			DoneEnabled = false;
			DoneClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnDoneClick() { DoneClick(); }
		#endregion
	}

	public interface IContainerEncounterLogView : ICanvasView
	{
		string Title { set; }
		float Scroll { set; }
		bool DoneEnabled { set; }
		Action DoneClick { set; }

		Transform EntryArea { get; }
	}
}