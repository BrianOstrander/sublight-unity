using System;
using System.Linq;
using System.Collections.Generic;
using LunraGames.SubLight.Models;
using UnityEngine;

namespace LunraGames.SubLight
{
	public class ModuleLogHandler : EncounterLogHandler<ModuleEncounterLogModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Module;

		public ModuleLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ModuleEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			FilterAll(
				(status, result) => OnHandleFilterAll(status, result.ToList(), linearDone),
				edge => edge.Filtering,
				logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray()	
			);
		}

		void OnHandleFilterAll(
			RequestStatus status,
			List<ModuleEdgeModel> filtered,
			Action linearDone
		)
		{
			void OnDone(Result result)
			{
				switch (result.Status)
				{
					case RequestStatus.Success: break;
					default:
						result.Log("The following error interrupted module log handling, unexpected behaviour may occur.");
						break;
				}
				Configuration.Model.Ship.Statistics.Value = new ShipStatistics(Configuration.Model.Ship.Statistics.Value.Modules);
				linearDone();
			}
			
			if (status != RequestStatus.Success || filtered.None())
			{
				OnDone(Result.Success());
				return;
			}
			
			void OnNext(Result result)
			{
				switch (result.Status)
				{
					case RequestStatus.Success:
						OnHandleFilterAll(
							result.Status,
							filtered,
							linearDone
						);
						break;
					default:
						OnDone(result);
						break;
				}
			}
			
			var next = filtered.First();
			filtered.RemoveAt(0);
			
			switch (next.Operation.Value)
			{
				case ModuleEdgeModel.Operations.AppendTraitByTraitId:
				case ModuleEdgeModel.Operations.RemoveTraitByTraitId:
				case ModuleEdgeModel.Operations.RemoveTraitByTraitFamilyId:
					OnHandleTraitOperation(next.Operation.Value, next.TraitInfo.Value, OnNext);
					break;
				case ModuleEdgeModel.Operations.ReplaceModule:
					OnHandleModuleOperation(next.Operation.Value, next.ModuleInfo.Value, OnNext);
					break;
				default:
					OnDone(Result.Failure("Unrecognized " + nameof(ModuleEdgeModel.Operations) + ": " + next.Operation.Value));
					break;
			}
		}

		void OnHandleTraitOperation(
			ModuleEdgeModel.Operations operation,
			ModuleEdgeModel.TraitBlock block,
			Action<Result> done
		)
		{
			var moduleTypes = block.ValidModuleTypes.None() ? EnumExtensions.GetValues(ModuleTypes.Unknown) : block.ValidModuleTypes;
			var modules = Configuration.Model.Ship.Statistics.Value.Modules.Where(m => moduleTypes.Contains(m.Type.Value)).ToList();

			switch (operation)
			{
				case ModuleEdgeModel.Operations.AppendTraitByTraitId:
					OnHandleTraitAppendByTraitIdNext(modules, block.OperationId, done);
					break;
				case ModuleEdgeModel.Operations.RemoveTraitByTraitId:
					OnHandleTraitRemoveByTraitIdNext(modules, block.OperationId, done);
					break;
				case ModuleEdgeModel.Operations.RemoveTraitByTraitFamilyId:
					OnHandleTraitRemoveByTraitFamilyIdNext(modules, block.OperationId, done);
					break;
				default:
					done(Result.Failure("Operation \"" + operation + "\" is not a recognized " + nameof(ModuleEdgeModel.TraitBlock) + " operation"));
					break;
			}
		}

		void OnHandleTraitAppendByTraitIdNext(
			List<ModuleModel> remaining,
			string traitId,
			Action<Result> done	
		)
		{
			if (remaining.None())
			{
				done(Result.Success());
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);
			
			Configuration.Model.Context.ModuleService.AppendTraits(
				next,
				result =>
				{
					switch (result.Status)
					{
						case RequestStatus.Success:
							if (result.Payload.Appended.Any())
							{
								// TODO: Add to audit log here...
							}
							OnHandleTraitAppendByTraitIdNext(
								remaining,
								traitId,
								done
							);
							break;
						default: done(result); break;
					}
				},
				traitId
			);
		}
		
		void OnHandleTraitRemoveByTraitIdNext(
			List<ModuleModel> remaining,
			string traitId,
			Action<Result> done	
		)
		{
			foreach (var module in remaining)
			{
				var traitIdsRemoved = module.TraitIds.Value.Where(t => t == traitId); 
				if (traitIdsRemoved.None()) continue;

				// TODO: Add to audit log here...
				module.TraitIds.Value = module.TraitIds.Value.Except(traitIdsRemoved).ToArray();
			}
			done(Result.Success());
		}
		
		void OnHandleTraitRemoveByTraitFamilyIdNext(
			List<ModuleModel> remaining,
			string familyId,
			Action<Result> done	
		)
		{
			var traitIdsInFamily = Configuration.Model.Context.ModuleService.GetTraitsByFamilyId(familyId).Select(t => t.Id.Value);
			foreach (var module in remaining)
			{
				var traitIdsRemoved = module.TraitIds.Value.Where(t => traitIdsInFamily.Contains(t));
				if (traitIdsRemoved.None()) continue;

				// TODO: Add to audit log here...
				module.TraitIds.Value = module.TraitIds.Value.Except(traitIdsRemoved).ToArray();
			}
			done(Result.Success());
		}

		void OnHandleModuleOperation(
			ModuleEdgeModel.Operations operation,
			ModuleEdgeModel.ModuleBlock block,
			Action<Result> done
		)
		{
			Debug.LogError("Module operation not handled yet!");
			done(Result.Success());
		}
	}
}