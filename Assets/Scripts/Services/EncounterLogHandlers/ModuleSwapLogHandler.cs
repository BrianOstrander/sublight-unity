using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;
using UnityEngine;

namespace LunraGames.SubLight
{
	public class ModuleSwapLogHandler : EncounterLogHandler<ModuleSwapEncounterLogModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.ModuleSwap;

		public ModuleSwapLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ModuleSwapEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			FilterAll(
				(status, result) => OnHandleFilterAll(status, result, logModel, linearDone),
				edge => edge.Filtering,
				logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray()		
			);
		}

		void OnHandleFilterAll(
			RequestStatus status,
			ModuleSwapEdgeModel[] filtered,
			ModuleSwapEncounterLogModel logModel,
			Action linearDone
		)
		{
			if (status != RequestStatus.Success || filtered.None())
			{
				linearDone();
				return;
			}

			Configuration.Model.Context.ModuleService.GenerateModulesWithTraits(
				filtered.Select(f => new ModuleService.ModuleConstraintWithTraitLimits(f.ModuleConstraint.Value, f.Traits.Value)).ToArray(),
				result => OnHandleGeneration(result, logModel, linearDone)
			);
		}

		void OnHandleGeneration(
			ResultArray<ModuleModel> result,
			ModuleSwapEncounterLogModel logModel,
			Action linearDone
		)
		{
			switch (result.Status)
			{
				case RequestStatus.Success: break;
				default:
					result.LogIfNotSuccess("The following error interrupted module swap log handling generation, skipping");
					linearDone();
					return;
			}

			var generatedModules = result.Payloads;

			if (generatedModules.None())
			{
				linearDone();
				return;
			}

			var generatedModulesSorted = new List<ModuleModel>();

			// Removing duplicates.
			foreach (var generatedModule in generatedModules)
			{
				if (generatedModulesSorted.Any(m => m.Type.Value == generatedModule.Type.Value)) continue;
				generatedModulesSorted.Add(generatedModule);
			}

			generatedModules = generatedModulesSorted.ToArray();

			switch (logModel.Style.Value)
			{
				case ModuleSwapEncounterLogModel.Styles.Instant:
					OnHandleInstant(
						generatedModules,
						logModel,
						linearDone
					);
					break;
				case ModuleSwapEncounterLogModel.Styles.Derelict:
					OnHandleRequest(
						generatedModules,
						logModel,
						linearDone
					);
					break;
				default:
					Debug.LogError("Unrecognized " + nameof(ModuleSwapEncounterLogModel.Style) + ": " + logModel.Style.Value + ", skipping");
					linearDone();
					break;
			}
		}

		void OnHandleInstant(
			ModuleModel[] generatedModules,
			ModuleSwapEncounterLogModel logModel,
			Action linearDone
		)
		{
			var request = new ModuleSwapHandlerModel(logModel);

			request.InitialState.Value = new ModuleSwapHandlerModel.State(
				generatedModules,
				Configuration.Model.Ship.Statistics.Value.Modules
			);

			var current = new List<ModuleModel>();
			var removed = new List<ModuleModel>();

			foreach (var module in Configuration.Model.Ship.Statistics.Value.Modules)
			{
				var generatedModule = generatedModules.FirstOrDefault(m => m.Type.Value == module.Type.Value);
				
				if (generatedModule == null) current.Add(module);
				else
				{
					current.Add(generatedModule);
					removed.Add(module);
				}
			}
			
			request.FinalState.Value = new ModuleSwapHandlerModel.State(
				current: current.ToArray(),
				removed: removed.ToArray()
			);

			OnHandleDone(request, linearDone);
		}
		
		void OnHandleRequest(
			ModuleModel[] generatedModules,
			ModuleSwapEncounterLogModel logModel,
			Action linearDone
		)
		{
			var request = new ModuleSwapHandlerModel(logModel);

			request.InitialState.Value = new ModuleSwapHandlerModel.State(
				generatedModules,
				Configuration.Model.Ship.Statistics.Value.Modules
			);

			request.FinalState.Value = request.InitialState.Value;

			request.Done.Value = () => OnHandleDone(request, linearDone);
			
			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(request));
		}

		void OnHandleDone(
			ModuleSwapHandlerModel handlerModel,
			Action linearDone
		)
		{
			Configuration.Model.Ship.Statistics.Value = new ShipStatistics(handlerModel.FinalState.Value.Current);
			linearDone();
		}
	}
}