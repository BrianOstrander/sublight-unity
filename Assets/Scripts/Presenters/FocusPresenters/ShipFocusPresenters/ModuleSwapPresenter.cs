using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ModuleSwapPresenter : ShipFocusPresenter<IModuleSwapView>
	{
		GameModel model;
		ModuleSwapLanguageBlock language;

		ModuleSwapHandlerModel handlerModel;

		protected override bool CanShow() => handlerModel != null;

		public ModuleSwapPresenter(
			GameModel model,
			ModuleSwapLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
			
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}
		
		protected override void OnUpdateEnabled()
		{
			if (handlerModel == null)
			{
				Debug.LogError("Swap presenter was enabled when it shouldn't be");
				return;
			}

			UpdateEntries();

			View.ConfirmText = language.Confirm.Value.Value;
			View.ConfirmClick = OnConfirmClick;
		}

		void UpdateEntries()
		{
			var state = handlerModel.FinalState.Value;

			ModuleSwapBlock.ModuleEntry GetEntry(ModuleModel module, ModuleTypes moduleType)
			{
				var current = new ModuleSwapBlock.ModuleEntry();
				current.Type = language.Types[moduleType].Value.Value;
				
				if (module == null)
				{
					current.Traits = new ModuleSwapBlock.ModuleEntry.TraitBlock[0];
					current.IsBlank = true;
				}
				else
				{
					current.Id = module.Id.Value;
					current.Name = module.Name.Value;
					current.YearManufactured = language.YearManufactured.Value.Value + ": " + module.YearManufactured.Value;
					current.Description = module.Description.Value;
					
					switch (module.Type.Value)
					{
						case ModuleTypes.Navigation:
							current.TransitRange = language.NavigationRange.Value.Value + ": " + module.TransitRange.Value.ToString("N2") + " " + language.NavigationRangeUnit.Value.Value;
							break;
						case ModuleTypes.Propulsion:
							current.TransitVelocity = language.Velocity.Value.Value + ": " + module.TransitVelocity.Value.ToString("N2") + " " + language.VelocityUnit.Value.Value;
							break;
						case ModuleTypes.PowerProduction:
							current.PowerProduction = language.PowerProduction.Value.Value + ": " + module.PowerProduction.Value.ToString("N2");
							break;
					}

					switch (module.Type.Value)
					{
						case ModuleTypes.PowerProduction: break;
						default:
							current.PowerConsumption = language.PowerConsumption.Value.Value + ": " + module.PowerConsumption.Value.ToString("N2");
							break;
					}
					
					current.IsInteractable = true;
					
					var traits = new List<ModuleSwapBlock.ModuleEntry.TraitBlock>();

					var definingTrait = ModuleTraitSeverity.Neutral;
					foreach (var trait in model.Context.ModuleService.GetTraits(trait => module.TraitIds.Value.Contains(trait.Id.Value)))
					{
						traits.Add(
							new ModuleSwapBlock.ModuleEntry.TraitBlock
							{
								Name = trait.Name.Value,
								Description = trait.Description.Value,
								SeverityText = language.Severities[trait.Severity].Value.Value,
								Severity = trait.Severity.Value
							}
						);
						if (trait.Severity.Value != ModuleTraitSeverity.Unknown || ((int) definingTrait < (int) trait.Severity.Value)) definingTrait = trait.Severity.Value;
					}

					current.Traits = traits.ToArray();

					current.DefiningSeverity = definingTrait;

					switch (definingTrait)
					{
						case ModuleTraitSeverity.Unknown: break;
						default:
							current.DefiningSeverityText = language.Severities[definingTrait].Value.Value;
							break;
					}
				}

				return current;
			}
			
			var availableModules = new List<ModuleSwapBlock.ModuleEntry>();
			
			foreach (var moduleType in EnumExtensions.GetValues(ModuleTypes.Unknown))
			{
				var module = state.Available.FirstOrDefault(m => m.Type.Value == moduleType);
				var entry = GetEntry(module, moduleType);
				entry.IsForeign = true;
				entry.Click = () => OnAvailableClick(module);
				availableModules.Add(entry);
			}
			
			var currentModules = new List<ModuleSwapBlock.ModuleEntry>();

			foreach (var module in state.Current)
			{
				var entry = GetEntry(module, module.Type.Value);
				entry.IsForeign = handlerModel.InitialState.Value.Available.Any(m => m.Id.Value == module.Id.Value);
				entry.Click = () => OnCurrentClick(module);
				entry.IsInteractable = handlerModel.InitialState.Value.Available.Any(m => m.Type.Value == module.Type.Value) || handlerModel.InitialState.Value.Removed.Any(m => m.Type.Value == module.Type.Value);
				currentModules.Add(entry);
			}

			var removedModules = new List<ModuleSwapBlock.ModuleEntry>();
			
			foreach (var moduleType in EnumExtensions.GetValues(ModuleTypes.Unknown))
			{
				var module = state.Removed.FirstOrDefault(m => m.Type.Value == moduleType);
				var entry = GetEntry(module, moduleType);
				entry.Click = () => OnRemovedClick(module);
				removedModules.Add(entry);
			}
			
			View.SetEntries(
				new ModuleSwapBlock
				{
					SourceType = language.AvailableSourceTypeDefaults[handlerModel.Log.Value.Style.Value],
					Modules = availableModules.ToArray()
				},
				new ModuleSwapBlock
				{
					SourceType = language.CurrentType.Value,
					Modules = currentModules.ToArray()
				},
				new ModuleSwapBlock
				{
					SourceType = language.RemovedType.Value,
					Modules = removedModules.ToArray()
				}
			);
		}
		
		#region Callback Events
		void OnEncounterRequest(EncounterRequest request)
		{
			if (request.State != EncounterRequest.States.Handle || request.LogType != EncounterLogTypes.ModuleSwap) return;
			if (!request.TryHandle<ModuleSwapHandlerModel>(OnEncounterModuleSwapHandle)) Debug.LogError("Unable to handle specified model");
		}

		void OnEncounterModuleSwapHandle(ModuleSwapHandlerModel handler)
		{
			switch (handler.Log.Value.Style.Value)
			{
				case ModuleSwapEncounterLogModel.Styles.Derelict: break;
				case ModuleSwapEncounterLogModel.Styles.Instant:
					Debug.LogError("Instant Styles for module swaps should have been handled in the module handler. Skipping...");
					handler.Done.Value();
					return;
				default:
					Debug.LogError("Unrecognized Style for module swaps: " + handler.Log.Value.Style.Value + ", skipping...");
					handler.Done.Value();
					return;
			}

			handlerModel = handler;

			ForceShowClose(true);
		}
		#endregion
		
		#region Model Events
		
		#endregion

		#region View Events
		void OnConfirmClick()
		{
			if (handlerModel.Log.Value.IsHaltingOnClose.Value) View.Closed += OnConfirmClosed;
			else OnConfirmClosed();
			
			CloseView();
		}

		void OnConfirmClosed()
		{
			var done = handlerModel.Done.Value; 
			
			handlerModel = null;
			done();
		}

		void OnAvailableClick(ModuleModel module)
		{
			if (module == null) return;

			var availableToCurrentModule = module;
			var currentToRemovedModule = handlerModel.FinalState.Value.Current.First(m => m.Type.Value == module.Type.Value);
			
			handlerModel.FinalState.Value = new ModuleSwapHandlerModel.State(
				handlerModel.FinalState.Value.Available.Where(m => m.Id.Value != availableToCurrentModule.Id.Value).ToArray(),
				handlerModel.FinalState.Value.Current.Where(m => m.Id.Value != currentToRemovedModule.Id.Value).Append(availableToCurrentModule).ToArray(),
				handlerModel.FinalState.Value.Removed.Append(currentToRemovedModule).ToArray()
			);
			
			UpdateEntries();
		}
		
		void OnCurrentClick(ModuleModel module)
		{
			if (module == null) return;

			if (handlerModel.InitialState.Value.Current.Any(m => m.Id.Value == module.Id.Value))
			{
				// It's as if we clicked the available module above this current entry.
				OnAvailableClick(handlerModel.InitialState.Value.Available.FirstOrDefault(m => m.Type.Value == module.Type.Value));
			}
			else if (handlerModel.InitialState.Value.Available.Any(m => m.Id.Value == module.Id.Value))
			{
				// We want to move a removed module back to current, and the current back to available.
				var currentToAvailableModule = module;
				var removedToCurrentModule = handlerModel.FinalState.Value.Removed.First(m => m.Type.Value == module.Type.Value);

				handlerModel.FinalState.Value = new ModuleSwapHandlerModel.State(
					handlerModel.FinalState.Value.Available.Append(currentToAvailableModule).ToArray(),
					handlerModel.FinalState.Value.Current.Where(m => m.Id.Value != currentToAvailableModule.Id.Value).Append(removedToCurrentModule).ToArray(),
					handlerModel.FinalState.Value.Removed.Where(m => m.Id.Value != removedToCurrentModule.Id.Value).ToArray()
				);
				UpdateEntries();
			}
		}
		
		void OnRemovedClick(ModuleModel module)
		{
			if (module == null) return;
			
			// It's as if we clicked the current module above this removed entry.
			OnCurrentClick(handlerModel.FinalState.Value.Current.First(m => m.Type.Value == module.Type.Value));
		}
		#endregion
	}
}