using System;

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
			RemoveTraitByFamilyId = 110,
			ReplaceModule = 200
		}
		
		[Serializable]
		public struct TraitBlock
		{
			public static TraitBlock Default(Operations operation)
			{
				var result = new TraitBlock
				{
					Operation = operation,
					ValidModuleTypes = new ModuleTypes[0]
				};
				
				switch (operation)
				{
					case Operations.AppendTraitByTraitId:
					case Operations.RemoveTraitByTraitId:
					case Operations.RemoveTraitByFamilyId:
						break;
					case Operations.Unknown:
						Debug.LogError("Operation cannot be \""+operation+"\", it must be specified");
						break;
					default:
						Debug.LogError("Operation \"" + operation + "\" is not a recognized " + nameof(TraitBlock) + " operation");
						break;
				}

				return result;
			}

			public Operations Operation;
			public string OperationId;
			public ModuleTypes[] ValidModuleTypes;

			[JsonIgnore]
			public string OperationIdName
			{
				get
				{
					switch (Operation)
					{
						case Operations.AppendTraitByTraitId:
						case Operations.RemoveTraitByTraitId:
							return "Trait Id";
						case Operations.RemoveTraitByFamilyId:
							return "Trait Family Id";
						default:
							return "Unknown Id";
					}
				}
			}
		}

		[Serializable]
		public struct ModuleBlock
		{
			public static ModuleBlock Default(Operations operation)
			{
				var result = new ModuleBlock
				{
					Operation = operation,
					ModuleConstraint = ModuleService.ModuleConstraint.Default,
					Traits = new ModuleService.TraitLimit[0]
				};
				
				switch (operation)
				{
					case Operations.ReplaceModule:
						break;
					case Operations.Unknown:
						Debug.LogError("Operation cannot be \""+operation+"\", it must be specified");
						break;
					default:
						Debug.LogError("Operation \"" + operation + "\" is not a recognized " + nameof(ModuleBlock) + " operation");
						break;
				}

				return result;
			}

			public Operations Operation;
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
					case Operations.RemoveTraitByFamilyId:
						return "Remove Trait By Family Id";
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