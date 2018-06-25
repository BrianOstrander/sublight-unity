using System;

using UnityEngine;
using UnityEngine.Events;

using LunraGames;

namespace LunraGames.SpaceFarm.Views
{
	public class HomeMenuView : CanvasView, IHomeMenuView
	{
		[SerializeField]
		LabelButtonLeaf loadEntryPrefab;
		[SerializeField]
		GameObject loadEntryArea;

		public Action StartClick { set; private get; }
		public LabelButtonBlock[] LoadEntries
		{
			set
			{
				loadEntryArea.transform.ClearChildren<LabelButtonLeaf>();
				if (value == null) return;
				foreach (var entry in value)
				{
					var instance = loadEntryArea.InstantiateChild(loadEntryPrefab, setActive: true);
					instance.ButtonLabel.text = entry.Text ?? string.Empty;
					instance.Button.OnClick.AddListener(new UnityAction(entry.Click ?? ActionExtensions.Empty));
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			loadEntryPrefab.gameObject.SetActive(false);
			StartClick = ActionExtensions.Empty;
			LoadEntries = null;
		}

		#region Events
		public void OnStartClick() { StartClick(); }
		#endregion
	}

	public interface IHomeMenuView : ICanvasView
	{
		Action StartClick { set; }
		LabelButtonBlock[] LoadEntries { set; }
	}
}