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

			/*
			var text = "The following modules were generated:";
			var index = 0;
			foreach (var module in result.Payloads)
			{
				text += "\n[ " + index + " ] : " + module.ToReadableJson();
				index++;
			}
			Debug.Log(text);
			*/

			var allModules = result.Payloads;

			if (allModules.None())
			{
				linearDone();
				return;
			}

			var shipModules = Configuration.Model.Ship.Statistics.Value.Modules.ToList();
			
			foreach (var moduleType in EnumExtensions.GetValues(ModuleTypes.Unknown))
			{
				var modules = allModules.Where(m => m.Type.Value == moduleType);
				if (modules.None()) continue;
				if (1 < modules.Count()) Debug.LogWarning("Multiple modules generated for module swap encounter log, picking only the first");

				shipModules.RemoveAll(m => m.Type.Value == moduleType);
				shipModules.Add(modules.First());
			}

			switch (logModel.Style.Value)
			{
				case ModuleSwapEncounterLogModel.Styles.Instant:
					Configuration.Model.Ship.Statistics.Value = new ShipStatistics(shipModules.ToArray());
					linearDone();
					break;
				default:
					Debug.LogError("Unrecognized " + nameof(ModuleSwapEncounterLogModel.Style) + ": " + logModel.Style.Value + ", skipping");
					linearDone();
					break;
			}
		}
	}
}