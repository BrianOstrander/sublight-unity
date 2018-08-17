using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class SystemBodyListView : CanvasView, ISystemBodyListView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		LabelButtonLeaf bodyEntryPrefab;
		[SerializeField]
		GameObject bodyEntryArea;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public LabelButtonBlock[] BodyEntries { set { SetEntries(bodyEntryArea, bodyEntryPrefab, value); } }
		public Action DoneClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			bodyEntryPrefab.gameObject.SetActive(false);
			Title = string.Empty;
			BodyEntries = null;
			DoneClick = ActionExtensions.Empty;
		}

		void SetEntries(GameObject root, LabelButtonLeaf prefab, params LabelButtonBlock[] entries)
		{
			root.transform.ClearChildren<LabelButtonLeaf>();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var instance = root.InstantiateChild(prefab, setActive: true);
				instance.ButtonLabel.text = entry.Text ?? string.Empty;
				instance.Button.OnClick.AddListener(new UnityAction(entry.Click ?? ActionExtensions.Empty));
				instance.Button.interactable = entry.Interactable;
			}
		}

		#region Events
		public void OnDoneClick() { DoneClick(); } 
		#endregion
	}

	public interface ISystemBodyListView : ICanvasView
	{
		string Title { set; }
		LabelButtonBlock[] BodyEntries { set; }
		Action DoneClick { set; }
	}
}