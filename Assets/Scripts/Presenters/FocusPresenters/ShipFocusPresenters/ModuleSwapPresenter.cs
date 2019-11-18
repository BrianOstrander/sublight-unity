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

			var state = handlerModel.InitialState.Value;

			ModuleSwapBlock.ModuleEntry GetEntry(ModuleModel module)
			{
				var current = new ModuleSwapBlock.ModuleEntry();
				current.Name = module.Name.Value;
				current.Description = module.Description.Value;
				current.Type = module.Type.Value.ToString();
				
				var traits = new List<ModuleSwapBlock.ModuleEntry.TraitBlock>();

				var definingTrait = ModuleTraitSeverity.Unknown;
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
					if ((int) definingTrait < (int) trait.Severity.Value) definingTrait = trait.Severity.Value;
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

				return current;
			}
			
			var sourceModules = new List<ModuleSwapBlock.ModuleEntry>();
			
			foreach (var module in state.Available)
			{
				var entry = GetEntry(module);
				sourceModules.Add(entry);
			}
			
			var destinationModules = new List<ModuleSwapBlock.ModuleEntry>();
			
			foreach (var module in state.Available)
			{
				var entry = GetEntry(module);
				destinationModules.Add(entry);
			}
			
			var discardedModules = new List<ModuleSwapBlock.ModuleEntry>();
			
			foreach (var module in state.Available)
			{
				var entry = GetEntry(module);
				discardedModules.Add(entry);
			}
			
			View.SetEntries(
				new ModuleSwapBlock
				{
					Modules = sourceModules.ToArray()
				},
				new ModuleSwapBlock
				{
					Modules = destinationModules.ToArray()
				},
				new ModuleSwapBlock
				{
					Modules = discardedModules.ToArray()
				}
			);
			
			View.ConfirmClick = OnConfirmClick;
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
			handlerModel.FinalState.Value = handlerModel.InitialState.Value;
			var done = handlerModel.Done.Value; 
			
			handlerModel = null;
			done();
		}
		#endregion
	}
}