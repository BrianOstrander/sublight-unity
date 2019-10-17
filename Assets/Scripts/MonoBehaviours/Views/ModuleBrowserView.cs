using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public struct ModuleBrowserBlock
	{
		public struct TraitBlock
		{
			public string Name;
			public string SeverityText;
			public string Description;
			public ModuleTraitSeverity Severity;
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
		
		public string DefiningSeverityText;
		public ModuleTraitSeverity DefiningSeverity;

		public TraitBlock[] Traits;
	}

	public class ModuleBrowserView : View, IModuleBrowserView
	{
		[Serializable]
		struct SeverityStyleEntry
		{
			public ModuleTraitSeverity Severity;
			public XButtonStyleObject StylePrimary;
			public XButtonStyleObject StyleSecondary;
		}
		
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
		SeverityStyleEntry[] severityStyles;
		
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

		Dictionary<string, ModuleBrowserEntryLeaf> entryInstances = new Dictionary<string, ModuleBrowserEntryLeaf>();
		
		ModuleBrowserBlock[] entries;
		public ModuleBrowserBlock[] Entries
		{
			set
			{
				entries = value;
				entryArea.ClearChildren();
				entryInstances.Clear();
				if (entries == null || entries.None() || (!string.IsNullOrEmpty(Selected) && entries.None(e => e.Id == Selected)))
				{
					Selected = null;
					return;
				}
				
				foreach (var entry in entries)
				{
					var current = entryArea.gameObject.InstantiateChild(entryPrefab, setActive: true);
					entryInstances.Add(entry.Id, current);
					
					current.NameLabel.text = entry.Name;
					current.TypeLabel.text = entry.Type;
					current.SeverityLabel.text = entry.DefiningSeverityText;

					var style = severityStyles.FirstOrDefault(s => s.Severity == entry.DefiningSeverity);

					current.SeverityPrimary.GlobalStyle = style.StylePrimary;
					current.SeveritySecondary.GlobalStyle = style.StyleSecondary;

					var currentEntry = entry; // TODO: Do I still need to do this?
					current.Button.OnClick.AddListener(() => OnClickEntry(currentEntry));
				}
				
				Selected = Selected; // Kinda weird...
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
					yearManufacturedTitleLabel.text = current.Value.YearManufacturedTitle;
					yearManufacturedLabel.text = current.Value.YearManufactured;
					powerProductionTitleLabel.text = current.Value.PowerProductionTitle;
					powerProductionLabel.text = current.Value.PowerProduction;
					powerConsumptionTitleLabel.text = current.Value.PowerConsumptionTitle;
					powerConsumptionLabel.text = current.Value.PowerConsumption;
					descriptionLabel.text = current.Value.Description;
				}
				else detailsGroup.alpha = 0f;

				foreach (var entry in entryInstances)
				{
					entry.Value.SelectedArea.SetActive(entry.Key == selected);
				}
			}
			private get => selected;
		}
		
		public Action<string> Selection { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Entries = null;
			Selected = null;
			Selection = ActionExtensions.GetEmpty<string>();
			
			entryPrefab.gameObject.SetActive(false);
		}

		#region Events
		void OnClickEntry(ModuleBrowserBlock block)
		{
			Selected = block.Id;
			Selection(block.Id);
		}
		#endregion
	}

	public interface IModuleBrowserView : IView
	{
		ModuleBrowserBlock[] Entries { set; }
		string Selected { set; }
		Action<string> Selection { set; }
	}
}