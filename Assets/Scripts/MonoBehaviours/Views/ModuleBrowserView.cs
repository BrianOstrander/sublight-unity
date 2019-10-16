using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public struct ModuleBrowserBlock
	{
		public struct TraitBlock
		{
			public string Name;
			public string Severity;
			public string Description;
			public ModuleTraitSeverity SeverityStyle;
		}

		public string Id;
		public string Name;
		public string Type;
		public string YearManufacturedTitle;
		public string YearManufactured;
		public string PowerProductionTitle;
		public string PowerProduction;
		public string PowerConsumptionTitle;
		public string PowerConsumption;
		public string Description;
		
		public ModuleTraitSeverity SeverityStyle;

		public TraitBlock[] Traits;
	}

	public class ModuleBrowserView : View, IModuleBrowserView
	{

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform entryArea;
		[SerializeField]
		ModuleBrowserEntryLeaf entryPrefab;
		[SerializeField]
		Transform traitArea;
		[SerializeField]
		ModuleBrowserEntryLeaf traitPrefab;

		[SerializeField]
		CanvasGroup detailsGroup;
		[SerializeField]
		TextMeshProUGUI nameLabel;
		[SerializeField]
		TextMeshProUGUI typeLabel;
		[SerializeField]
		TextMeshProUGUI yearManufacturedTitleLabel;
		[SerializeField]
		TextMeshProUGUI yearManufacturedLabel;
		[SerializeField]
		TextMeshProUGUI powerProductionTitleLabel;
		[SerializeField]
		TextMeshProUGUI powerProductionLabel;
		[SerializeField]
		TextMeshProUGUI powerConsumptionTitleLabel;
		[SerializeField]
		TextMeshProUGUI powerConsumptionLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		ModuleBrowserBlock[] entries;
		public ModuleBrowserBlock[] Entries
		{
			set
			{
				entries = value;
				entryArea.ClearChildren();
				if (entries == null || entries.None() || (!string.IsNullOrEmpty(Selected) && entries.None(e => e.Id == Selected)))
				{
					Selected = null;
					return;
				}
				Selected = Selected; // Kinda weird...

				foreach (var entry in entries)
				{
					var current = entryArea.gameObject.InstantiateChild(entryPrefab, setActive: true);
					current.NameLabel.text = entry.Name;
					current.TypeLabel.text = entry.Type;
					
				}
			}
			private get => entries;
		}

		string selected;
		public string Selected
		{
			set
			{
				selected = value;

				var current = Entries?.FirstOrDefault(e => e.Id == selected);

				if (!string.IsNullOrEmpty(selected) && current.HasValue)
				{
					detailsGroup.alpha = 1f;

					nameLabel.text = current.Value.Name;
					typeLabel.text = current.Value.Type;
					// yearManufacturedTitleLabel.text = current.Value.YearManufacturedTitle;
					// yearManufacturedLabel.text = current.Value.YearManufactured;
					// powerProductionTitleLabel.text = current.Value.PowerProductionTitle;
					// powerProductionLabel.text = current.Value.PowerProduction;
					// powerConsumptionTitleLabel.text = current.Value.PowerConsumptionTitle;
					// powerConsumptionLabel.text = current.Value.PowerConsumption;
					descriptionLabel.text = current.Value.Description;
					
					
				}
				else detailsGroup.alpha = 0f;
			}
			private get => selected;
		}
		
		public Action<string> Selection { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Entries = null;
			Selected = null;
			Selection = null;
			
			entryPrefab.gameObject.SetActive(false);
		}

		#region Events
		
		#endregion
	}

	public interface IModuleBrowserView : IView
	{
		ModuleBrowserBlock[] Entries { set; }
		string Selected { set; }
		Action<string> Selection { set; }
	}
}