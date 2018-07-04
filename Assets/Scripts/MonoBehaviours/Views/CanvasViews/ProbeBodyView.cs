using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class ProbeBodyView : CanvasView, IProbeBodyView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI rationsLabel;
		[SerializeField]
		TextMeshProUGUI fuelLabel;
		[SerializeField]
		LabelButtonLeaf probeEntryPrefab;
		[SerializeField]
		GameObject probeEntryArea;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public float Rations { set { rationsLabel.text = Strings.Rations(value); } }
		public float Fuel { set { fuelLabel.text = Strings.Fuel(value); } }
		public LabelButtonBlock[] ProbeEntries { set { SetEntries(probeEntryArea, probeEntryPrefab, value); } }
		public Action BackClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			probeEntryPrefab.gameObject.SetActive(false);
			Title = string.Empty;
			Rations = 0f;
			Fuel = 0f;
			ProbeEntries = null;
			BackClick = ActionExtensions.Empty;
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
			}
		}

		#region Events
		public void OnBackClick() { BackClick(); }
		#endregion
	}

	public interface IProbeBodyView : ICanvasView
	{
		string Title { set; }
		float Rations { set; }
		float Fuel { set; }
		LabelButtonBlock[] ProbeEntries { set; }
		Action BackClick { set; }
	}
}