using System;
using TMPro;
using UnityEngine;

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
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI sourceLabel;
		[SerializeField]
		TextMeshProUGUI destinationLabel;
		[SerializeField]
		TextMeshProUGUI discardedLabel;
		
		[SerializeField]
		Transform sourceArea;
		[SerializeField]
		Transform destinationArea;
		[SerializeField]
		Transform discardedArea;

		[SerializeField]
		ModuleSwapEntryLeaf entryPrefab;
		
		[SerializeField]
		CanvasGroup detailsGroup;
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
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		#region Bindings
		public void SetEntries(
			ModuleSwapBlock source,
			ModuleSwapBlock destination,
			ModuleSwapBlock discarded
		)
		{
			highlightedModuleId = null;
			sourceArea.ClearChildren();
			destinationArea.ClearChildren();
			discardedArea.ClearChildren();

			ModuleSwapEntryLeaf DefaultEntryInitialization(
				GameObject parent,
				ModuleSwapBlock.ModuleEntry module
			)
			{
				var entry = parent.InstantiateChild(entryPrefab);
				entry.NameLabel.text = module.Name;
				entry.SeverityLabel.text = module.DefiningSeverityText;
				entry.TypeLabel.text = module.Type;
				entry.Button.OnEnter.AddListener(() => OnModuleEnter(module));
				entry.Button.OnExit.AddListener(() => OnModuleExit(module));
				entry.Button.OnClick.AddListener(() => module.Click());
				return entry;
			}
			
			foreach (var module in source.Modules)
			{
				var entry = DefaultEntryInitialization(sourceArea.gameObject, module);
				foreach (var control in entry.DownControls) control.SetActive(true);
				foreach (var control in entry.UpControls) control.SetActive(false);
			}
			
			foreach (var module in destination.Modules)
			{
				var entry = DefaultEntryInitialization(destinationArea.gameObject, module);
				if (module.IsForeign)
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
			
			foreach (var module in discarded.Modules)
			{
				var entry = DefaultEntryInitialization(discardedArea.gameObject, module);
				foreach (var control in entry.DownControls) control.SetActive(false);
				foreach (var control in entry.UpControls) control.SetActive(true);
			}
			
			
			SetDetails(null);
		}
		
		public Action ConfirmClick { set; private get; }
		#endregion
		
		#region Local
		string highlightedModuleId;
		#endregion
		
		public override void Reset()
		{
			base.Reset();

			highlightedModuleId = null;
			
			sourceArea.ClearChildren();
			destinationArea.ClearChildren();
			discardedArea.ClearChildren();

			ConfirmClick = ActionExtensions.Empty;
		}

		void SetDetails(ModuleSwapBlock.ModuleEntry? module)
		{
			// detailsGroup.alpha = module.HasValue ? 1f : 0f;

			if (!module.HasValue) return;

			/*
			nameLabel.text = module.Value.Name;
			typeLabel.text = module.Value.Type;
			yearManufacturedLabel.text = module.Value.YearManufactured;
			descriptionLabel.text = module.Value.Description;
			powerProductionLabel.text = module.Value.PowerProduction;
			powerConsumptionLabel.text = module.Value.PowerConsumption;
			transitVelocityLabel.text = module.Value.TransitVelocity;
			transitRangeLabel.text = module.Value.TransitRange;

			*/
			highlightedModuleId = module.Value.Id;
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
			ModuleSwapBlock source,
			ModuleSwapBlock destination,
			ModuleSwapBlock discarded
		);

		Action ConfirmClick { set; }
	}
}