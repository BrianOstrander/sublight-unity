using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class ContainerEncounterLogView : CanvasView, IContainerEncounterLogView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		Transform entryArea;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public Action DoneClick { set; private get; }

		public Transform EntryArea { get { return entryArea; } }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			DoneClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnDoneClick() { DoneClick(); }
		#endregion
	}

	public interface IContainerEncounterLogView : ICanvasView
	{
		string Title { set; }
		Action DoneClick { set; }

		Transform EntryArea { get; }
	}
}