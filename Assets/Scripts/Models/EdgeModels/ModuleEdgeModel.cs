using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class ModuleEdgeModel : EdgeModel
	{
		public enum Operations
		{
			Unknown = 0,
			AppendTraitByTraitId = 10,
			RemoveTraitByTraitId = 100,
			RemoveTraitByTraitFamilyId = 110,
			ReplaceModule = 200
		}
		
		[Serializable]
		public struct TraitBlock
		{
			public static readonly Operations[] ValidOperations = {
				Operations.AppendTraitByTraitId,
				Operations.RemoveTraitByTraitId,
				Operations.RemoveTraitByTraitFamilyId
			};
			
			public static TraitBlock Default(Operations operation)
			{
				var result = new TraitBlock
				{
					ValidModuleTypes = new ModuleTypes[0]
				};
				
				if (operation == Operations.Unknown) Debug.LogError("Operation cannot be \""+operation+"\", it must be specified");
				else if (!ValidOperations.Contains(operation)) Debug.LogError("Operation \"" + operation + "\" is not a recognized " + nameof(TraitBlock) + " operation");
				
				return result;
			}

			public string OperationId;
			public ModuleTypes[] ValidModuleTypes;

			public string GetOperationIdName(Operations operation)
			{
				switch (operation)
				{
					case Operations.AppendTraitByTraitId:
					case Operations.RemoveTraitByTraitId:
						return "Trait Id";
					case Operations.RemoveTraitByTraitFamilyId:
						return "Trait Family Id";
					default:
						return "Unrecognized Id";
				}
			}
		}

		[Serializable]
		public struct ModuleBlock
		{
			public static readonly Operations[] ValidOperations = {
				Operations.ReplaceModule
			};
			
			public static ModuleBlock Default(Operations operation)
			{
				var result = new ModuleBlock
				{
					ModuleConstraint = ModuleService.ModuleConstraint.Default,
					Traits = new ModuleService.TraitLimit[0]
				};
				
				if (operation == Operations.Unknown) Debug.LogError("Operation cannot be \""+operation+"\", it must be specified");
				else if (!ValidOperations.Contains(operation)) Debug.LogError("Operation \"" + operation + "\" is not a recognized " + nameof(ModuleBlock) + " operation");
				
				return result;
			}

			public ModuleService.ModuleConstraint ModuleConstraint;
			public ModuleService.TraitLimit[] Traits;
		}

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore] public ValueFilterModel Filtering => filtering;

		[JsonProperty] Operations operation;
		[JsonIgnore] public readonly ListenerProperty<Operations> Operation;
		
		[JsonProperty] TraitBlock traitInfo;
		[JsonIgnore] public readonly ListenerProperty<TraitBlock> TraitInfo;
		
		[JsonProperty] ModuleBlock moduleInfo;
		[JsonIgnore] public readonly ListenerProperty<ModuleBlock> ModuleInfo;

		public override string EdgeName
		{
			get
			{
				switch (Operation.Value)
				{
					case Operations.AppendTraitByTraitId:
						return "Append Trait By Trait Id";
					case Operations.RemoveTraitByTraitId:
						return "Remove Trait By Trait Id";
					case Operations.RemoveTraitByTraitFamilyId:
						return "Remove Trait By Trait Family Id";
					case Operations.ReplaceModule:
						return "Replace Module";
					default:
						return "Unknown Operation";	
				}
			}
		}
		
		public ModuleEdgeModel()
		{
			Operation = new ListenerProperty<Operations>(value => operation = value, () => operation);
			TraitInfo = new ListenerProperty<TraitBlock>(value => traitInfo = value, () => traitInfo);
			ModuleInfo = new ListenerProperty<ModuleBlock>(value => moduleInfo = value, () => moduleInfo);
		}
	}
}