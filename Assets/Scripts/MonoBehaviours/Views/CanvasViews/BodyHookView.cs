using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class BodyHookView : CanvasView, IBodyHookView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		[SerializeField]
		TextMeshProUGUI rationsLabel;
		[SerializeField]
		TextMeshProUGUI fuelLabel;
		[SerializeField]
		LabelButtonLeaf crewEntryPrefab;
		[SerializeField]
		GameObject crewEntryArea;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public string Description { set { descriptionLabel.text = value ?? string.Empty; } }
		public float Rations { set { rationsLabel.text = Strings.Rations(value); } }
		public float Fuel { set { fuelLabel.text = Strings.Fuel(value); } }
		public LabelButtonBlock[] CrewEntries { set { SetEntries(crewEntryArea, crewEntryPrefab, value); } }
		public Action BackClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			crewEntryPrefab.gameObject.SetActive(false);
			Title = string.Empty;
			Description = string.Empty;
			Rations = 0f;
			Fuel = 0f;
			CrewEntries = null;
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

	public interface IBodyHookView : ICanvasView
	{
		string Title { set; }
		string Description { set; }
		float Rations { set; }
		float Fuel { set; }
		LabelButtonBlock[] CrewEntries { set; }
		Action BackClick { set; }
	}
}