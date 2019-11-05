using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace LunraGames.SubLight.Views
{
	public struct ModuleBrowserBlock
	{
		public struct ModuleEntry
		{
			public struct TraitBlock
			{
				public string Name;
				public string Description;
			
				public string SeverityText;
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
		
		public ModuleEntry[] Modules;

		public string StatsTitle;
		public string VelocityTitle;
		public string VelocityValue;
		public string NavigationRangeTitle;
		public string NavigationRangeValue;
	}
	
	public class ModuleBrowserView : View, IModuleBrowserView
	{
		[Serializable]
		struct SeverityStyleEntry
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public ModuleTraitSeverity Severity;
			public XButtonStyleObject StylePrimary;
			public XButtonStyleObject StyleSecondary;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}
		
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform entryArea;
		[SerializeField]
		ModuleBrowserEntryLeaf entryPrefab;
		[SerializeField]
		Transform traitArea;
		[SerializeField]
		ModuleBrowserTraitEntryLeaf traitPrefab;

		[SerializeField]
		SeverityStyleEntry[] severityStyles;
		
		[SerializeField]
		TextMeshProUGUI statsTitleLabel;
		[SerializeField]
		TextMeshProUGUI velocityLabel;
		[SerializeField]
		TextMeshProUGUI navigationRangeLabel;
		
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
		
		ModuleBrowserBlock info;
		public ModuleBrowserBlock Info
		{
			set
			{
				info = value;
				entryArea.ClearChildren();
				entryInstances.Clear();

				statsTitleLabel.text = info.StatsTitle;
				velocityLabel.text = info.VelocityTitle + ": " + info.VelocityValue;
				navigationRangeLabel.text = info.NavigationRangeTitle + ": " + info.NavigationRangeValue;
				
				if (info.Modules == null || info.Modules.None() || (!string.IsNullOrEmpty(Selected) && info.Modules.None(e => e.Id == Selected)))
				{
					Selected = null;
					return;
				}
				
				foreach (var entry in info.Modules)
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
			private get => info;
		}

		string selected;
		public string Selected
		{
			set
			{
				selected = value;

				var current = Info.Modules?.FirstOrDefault(e => e.Id == selected);

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
					
					traitArea.ClearChildren();

					foreach (var trait in current.Value.Traits)
					{
						var currentTrait = traitArea.gameObject.InstantiateChild(traitPrefab, setActive: true);
						currentTrait.NameLabel.text = trait.Name;
						currentTrait.SeverityLabel.text = trait.SeverityText;
						currentTrait.DescriptionLabel.text = trait.Description;
						
						var style = severityStyles.FirstOrDefault(s => s.Severity == trait.Severity);

						currentTrait.SeverityPrimary.GlobalStyle = style.StylePrimary;
						currentTrait.SeveritySecondary.GlobalStyle = style.StyleSecondary;
					}
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

			Info = default(ModuleBrowserBlock);
			Selected = null;
			Selection = ActionExtensions.GetEmpty<string>();
			
			entryPrefab.gameObject.SetActive(false);
			traitPrefab.gameObject.SetActive(false);
		}

		#region Events
		void OnClickEntry(ModuleBrowserBlock.ModuleEntry block)
		{
			Selected = block.Id;
			Selection(block.Id);
		}
		#endregion
	}

	public interface IModuleBrowserView : IView
	{
		ModuleBrowserBlock Info { set; }
		string Selected { set; }
		Action<string> Selection { set; }
	}
}