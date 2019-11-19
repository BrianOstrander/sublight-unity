using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace LunraGames.SubLight.Views
{
	public struct ModuleSwapBlock
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

			public bool IsForeign;
			public bool IsBlank;
			public bool IsInteractable;
			
			public string Id;
			public string Name;
			public string Type;
			public string YearManufactured;
			public string PowerProduction;
			public string PowerConsumption;
			public string TransitVelocity;
			public string TransitRange;
			public string Description;
		
			public string DefiningSeverityText;
			public ModuleTraitSeverity DefiningSeverity;

			public TraitBlock[] Traits;

			public Action Click;
		}
		
		public ModuleEntry[] Modules;

		public string SourceType;
		public string SourceName;
		public string PowerProduction;
		public string PowerConsumption;
		public string TransitVelocity;
		public string TransitRange;
	}
	
	public class ModuleSwapView : View, IModuleSwapView
	{
		[Serializable]
		struct SeverityColorEntry
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public ModuleTraitSeverity Severity;
			public Color Primary;
			public Color Secondary;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}
		
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI availableLabel;
		[SerializeField]
		TextMeshProUGUI currentLabel;
		[SerializeField]
		TextMeshProUGUI removedLabel;
		
		[SerializeField]
		Transform availableArea;
		[SerializeField]
		Transform currentArea;
		[SerializeField]
		Transform removedArea;

		[FormerlySerializedAs("entryPrefab")] [SerializeField]
		ModuleSwapModuleEntryLeaf moduleEntryPrefab;

		[SerializeField]
		ModuleSwapTraitEntryLeaf traitEntryPrefab;

		[SerializeField]
		SeverityColorEntry[] severityColors;
		
		[SerializeField]
		CanvasGroup detailsGroup;
		[SerializeField]
		Transform traitArea;
		[SerializeField]
		TextMeshProUGUI nameLabel;
		[SerializeField]
		TextMeshProUGUI typeLabel;
		[SerializeField]
		TextMeshProUGUI yearManufacturedLabel;
		[SerializeField]
		TextMeshProUGUI powerProductionLabel;
		[SerializeField]
		TextMeshProUGUI powerConsumptionLabel;
		[SerializeField]
		TextMeshProUGUI transitVelocityLabel;
		[SerializeField]
		TextMeshProUGUI transitRangeLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		
		[SerializeField]
		TextMeshProUGUI confirmLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		#region Bindings
		public void SetEntries(
			ModuleSwapBlock available,
			ModuleSwapBlock current,
			ModuleSwapBlock removed
		)
		{
			highlightedModuleId = null;
			availableArea.ClearChildren();
			currentArea.ClearChildren();
			removedArea.ClearChildren();

			ModuleSwapModuleEntryLeaf DefaultEntryInitialization(
				GameObject parent,
				ModuleSwapBlock.ModuleEntry module
			)
			{
				var entry = parent.InstantiateChild(moduleEntryPrefab, setActive: true);

				foreach (var control in entry.ActiveControls) control.SetActive(!module.IsBlank);
				foreach (var control in entry.InactiveControls) control.SetActive(module.IsBlank);
				
				entry.NameLabel.text = module.Name;
				entry.SeverityLabel.text = module.DefiningSeverityText;
				entry.TypeLabel.text = module.Type;
				entry.Button.OnEnter.AddListener(() => OnModuleEnter(module));
				entry.Button.OnExit.AddListener(() => OnModuleExit(module));
				entry.Button.OnClick.AddListener(() => module.Click());
				
				var colorEntry = severityColors.FirstOrDefault(e => e.Severity == module.DefiningSeverity);
				foreach (var graphic in entry.PrimaryColors) graphic.color = colorEntry.Primary;
				foreach (var graphic in entry.SecondaryColors) graphic.color = colorEntry.Secondary;
				
				return entry;
			}
			
			foreach (var module in available.Modules)
			{
				var entry = DefaultEntryInitialization(availableArea.gameObject, module);
				foreach (var control in entry.DownControls) control.SetActive(true);
				foreach (var control in entry.UpControls) control.SetActive(false);
			}
			
			foreach (var module in current.Modules)
			{
				var entry = DefaultEntryInitialization(currentArea.gameObject, module);

				if (!module.IsInteractable)
				{
					foreach (var control in entry.DownControls) control.SetActive(false);
					foreach (var control in entry.UpControls) control.SetActive(false);
				}
				else if (module.IsForeign)
				{
					foreach (var control in entry.DownControls) control.SetActive(false);
					foreach (var control in entry.UpControls) control.SetActive(true);
				}
				else
				{
					foreach (var control in entry.DownControls) control.SetActive(true);
					foreach (var control in entry.UpControls) control.SetActive(false);
				}
			}
			
			foreach (var module in removed.Modules)
			{
				var entry = DefaultEntryInitialization(removedArea.gameObject, module);
				foreach (var control in entry.DownControls) control.SetActive(false);
				foreach (var control in entry.UpControls) control.SetActive(true);
			}

			availableLabel.text = available.SourceType;
			currentLabel.text = current.SourceType;
			removedLabel.text = removed.SourceType;
			
			SetDetails(null);
		}

		public string ConfirmText { set => confirmLabel.text = value ?? string.Empty; }
		
		public Action ConfirmClick { set; private get; }
		#endregion
		
		#region Local
		string highlightedModuleId;
		#endregion
		
		public override void Reset()
		{
			base.Reset();

			moduleEntryPrefab.gameObject.SetActive(false);
			traitEntryPrefab.gameObject.SetActive(false);
			
			highlightedModuleId = null;
			
			availableArea.ClearChildren();
			currentArea.ClearChildren();
			removedArea.ClearChildren();

			ConfirmText = null;
			ConfirmClick = ActionExtensions.Empty;
		}

		void SetDetails(ModuleSwapBlock.ModuleEntry? module)
		{
			if (!module.HasValue || module.Value.IsBlank)
			{
				highlightedModuleId = null;
				detailsGroup.alpha = 0f;
				return;
			}

			highlightedModuleId = module.Value.Id;
			detailsGroup.alpha = 1f;

			nameLabel.text = module.Value.Name;
			typeLabel.text = module.Value.Type;
			yearManufacturedLabel.text = module.Value.YearManufactured;
			descriptionLabel.text = module.Value.Description;
			powerProductionLabel.text = module.Value.PowerProduction;
			powerConsumptionLabel.text = module.Value.PowerConsumption;
			transitVelocityLabel.text = module.Value.TransitVelocity;
			transitRangeLabel.text = module.Value.TransitRange;

			traitArea.ClearChildren<ModuleSwapTraitEntryLeaf>();

			foreach (var trait in module.Value.Traits)
			{
				var entry = traitArea.gameObject.InstantiateChild(traitEntryPrefab, setActive: true);
				entry.NameLabel.text = trait.Name;
				entry.SeverityLabel.text = trait.SeverityText;
				entry.DescriptionLabel.text = trait.Description;

				var colorEntry = severityColors.FirstOrDefault(e => e.Severity == trait.Severity);
				foreach (var graphic in entry.PrimaryColors) graphic.color = colorEntry.Primary;
				foreach (var graphic in entry.SecondaryColors) graphic.color = colorEntry.Secondary;
			}
		}

		#region Events
		void OnModuleEnter(ModuleSwapBlock.ModuleEntry module)
		{
			SetDetails(module);
		}
		
		void OnModuleExit(ModuleSwapBlock.ModuleEntry module)
		{
			if (highlightedModuleId == module.Id) SetDetails(null);
		}

		public void OnConfirmClick() => ConfirmClick();
		
		#endregion
	}

	public interface IModuleSwapView : IView
	{
		void SetEntries(
			ModuleSwapBlock available,
			ModuleSwapBlock current,
			ModuleSwapBlock removed
		);

		string ConfirmText { set; }
		Action ConfirmClick { set; }
	}
}