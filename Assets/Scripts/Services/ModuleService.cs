using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public abstract class ModuleService : IModuleService
	{
		public struct ModuleConstraint
		{
			public static ModuleConstraint Default => new ModuleConstraint
			{
				ValidTypes = new ModuleTypes[0],
				PowerProduction = FloatRange.Zero,
				PowerConsumption = FloatRange.Zero,
				NavigationRange = FloatRange.Zero,
				NavigationVelocity = FloatRange.Zero,
				RepairCost = FloatRange.Zero,
				ManufacturerIds = new string[0],
				TraitLimits = new TraitLimit[0],
				TraitConstraint = TraitConstraint.Default 
			};

			public ModuleTypes[] ValidTypes;
			public FloatRange PowerProduction;
			public FloatRange PowerConsumption;
			public FloatRange NavigationRange;
			public FloatRange NavigationVelocity;
			public FloatRange RepairCost;
			public string[] ManufacturerIds;
			public TraitLimit[] TraitLimits;
			public TraitConstraint TraitConstraint;
		}

		public struct TraitConstraint
		{
			public static TraitConstraint Default => new TraitConstraint
			{
				IncompatibleIds = new string[0],
				IncompatibleFamilyIds = new string[0],
				RequiredCompatibleIds = new string[0],
				RequiredCompatibleFamilyIds = new string[0],
				ValidSeverity = new ModuleTraitSeverity[0]
			};

			public string[] IncompatibleIds;
			public string[] IncompatibleFamilyIds;
			public string[] RequiredCompatibleIds;
			public string[] RequiredCompatibleFamilyIds;
			public ModuleTraitSeverity[] ValidSeverity;
		}

		public struct TraitLimit
		{
			public ModuleTraitSeverity Severity;
			public IntegerRange Count;
		}

		protected struct GenerationInfo
		{
			public static GenerationInfo Default => new GenerationInfo
			{
				Seed = 0,
				Type = ModuleTypes.Unknown,
				ManufacturerId = null,
				KeyValues = new KeyValueListModel()
			};
			
			public int Seed;
			public ModuleTypes Type;
			public string ManufacturerId;
			public KeyValueListModel KeyValues;
		}
		
		protected readonly IModelMediator ModelMediator;
		protected readonly GameModel Model;
		
		protected List<ModuleTraitModel> Traits { get; private set; }
		
		public ModuleService(
			IModelMediator modelMediator,
			GameModel model
		)
		{
			ModelMediator = modelMediator ?? throw new ArgumentNullException(nameof(modelMediator));
			Model = model ?? throw new ArgumentNullException(nameof(model));
		}
		
		#region Initialization
		public void Initialize(
			Action<RequestStatus> done
		)
		{
			if (done == null) throw new ArgumentNullException(nameof(done));
			
			ModelMediator.LoadAll<ModuleTraitModel>(results => OnInitializeLoadedAll(results, done));
		}

		void OnInitializeLoadedAll(
			ModelArrayResult<ModuleTraitModel> results,
			Action<RequestStatus> done
		)
		{
			if (results.Status != RequestStatus.Success)
			{
				Debug.LogError("Loading all module traits failed with status: "+results.Status+" and error: "+results.Error);
				done(results.Status);
				return;
			}

			Traits = results.Models.Select(m => m.TypedModel).ToList();
		}
		#endregion

		public ModuleTraitModel GetTrait(string id) => Traits.FirstOrDefault(t => t.Id.Value == id);

		public void Generate(
			ModuleConstraint constraint,
			Action<RequestResult, ModuleModel> done
		)
		{
			if (constraint.ValidTypes == null) throw new ArgumentNullException(nameof(constraint.ValidTypes));
			if (constraint.ManufacturerIds == null) throw new ArgumentNullException(nameof(constraint.ManufacturerIds));
			if (constraint.TraitLimits == null) throw new ArgumentNullException(nameof(constraint.TraitLimits));

			if (constraint.ValidTypes.None()) constraint.ValidTypes = EnumExtensions.GetValues(ModuleTypes.Unknown);
			if (constraint.ManufacturerIds.None()) constraint.ManufacturerIds = new string[] { null }; // TODO: Actually list manufacturer ids

			var result = new ModuleModel();

			var random = new Demon(10); // TODO: Remove this non random thing.
			
			var info = GetInfo(
				random.NextInteger,
				random.GetNextFrom(constraint.ValidTypes),
				null // TODO: Actually provide a manufacturer.
			);

			result.Type.Value = info.Type;
			result.Name.Value = GetName(info);
			result.YearManufactured.Value = GetYearManufactured(info);
			result.Description.Value = GetDescription(info);

			result.PowerConsumption.Value = random.GetNextFloat(
				constraint.PowerConsumption.Primary,
				constraint.PowerConsumption.Secondary
			);
			
			result.RepairCost.Value = random.GetNextFloat(
				constraint.RepairCost.Primary,
				constraint.RepairCost.Secondary
			);
			
			switch (result.Type.Value)
			{
				case ModuleTypes.PowerProduction:
					result.PowerProduction.Value = random.GetNextFloat(
						constraint.PowerProduction.Primary,
						constraint.PowerProduction.Secondary
					);
					result.PowerConsumption.Value = 0f;
					break;
				case ModuleTypes.Navigation:
					result.NavigationRange.Value = random.GetNextFloat(
						constraint.NavigationRange.Primary,
						constraint.NavigationRange.Secondary
					);
					break;
				case ModuleTypes.Propulsion:
					result.NavigationVelocity.Value = random.GetNextFloat(
						constraint.NavigationVelocity.Primary,
						constraint.NavigationVelocity.Secondary
					);
					break;
			}

			AddTraits(
				result,
				constraint.TraitConstraint,
				constraint.TraitLimits,
				done
			);
		}

		public void AddTraits(
			ModuleModel module,
			TraitConstraint constraint,
			TraitLimit[] traitLimits,
			Action<RequestResult, ModuleModel> done
		)
		{
			if (constraint.IncompatibleIds == null) throw new ArgumentNullException(nameof(constraint.IncompatibleIds));
			if (constraint.IncompatibleFamilyIds == null) throw new ArgumentNullException(nameof(constraint.IncompatibleFamilyIds));
			if (constraint.RequiredCompatibleIds == null) throw new ArgumentNullException(nameof(constraint.RequiredCompatibleIds));
			if (constraint.RequiredCompatibleFamilyIds == null) throw new ArgumentNullException(nameof(constraint.RequiredCompatibleFamilyIds));
			if (constraint.ValidSeverity == null) throw new ArgumentNullException(nameof(constraint.ValidSeverity));
			
			var remaining = new Dictionary<ModuleTraitSeverity, int>();
			
			// TODO: Add checking for duplicate severity entries.
			
			var random = new Demon(99); // TODO: Remove this nonrandom thing.
			
			foreach (var traitLimit in traitLimits)
			{
				var count = random.GetNextInteger(
					traitLimit.Count.Primary,
					traitLimit.Count.Secondary
				);
				if (0 < count) remaining.Add(traitLimit.Severity, count);
			}
			
			
		}

		void OnAddTraits(
			ModuleTraitModel result,
			ModuleModel module,
			TraitConstraint constraint,
			Dictionary<ModuleTraitSeverity, int> remaining,
			Action<RequestResult, ModuleModel> done
		)
		{
			if (result != null)
			{
				module.TraitIds.Value = module.TraitIds.Value.Append(result.Id.Value).ToArray();
				constraint.IncompatibleIds = constraint.IncompatibleIds.Union(result.IncompatibleIds.Value).ToArray();
				constraint.IncompatibleFamilyIds = constraint.IncompatibleFamilyIds.Union(result.IncompatibleFamilyIds.Value).ToArray();
				constraint.RequiredCompatibleIds = constraint.RequiredCompatibleIds.Append(result.Id.Value).ToArray();
				remaining[result.Severity] = remaining[result.Severity] - 1;
			}

			constraint.ValidSeverity = remaining.Where(kv => 0 < kv.Value).Select(kv => kv.Key).ToArray();

			if (constraint.ValidSeverity.None())
			{
				done(RequestResult.Success(), module);
				return;	
			}
			
			// OnAddTraits(
			// 	Traits.Where(t => IsTraitValid(t, module.Type.Value, constraint)).Random(),
			// 	
			// );
			// var possibleTraits = Traits.Where(t => !);

			Debug.LogWarning("TODO REST OF THIS LOGIC");
		}

		bool IsTraitValid(
			ModuleTraitModel trait,
			ModuleTypes type,
			TraitConstraint constraint
		)
		{
			if (!constraint.ValidSeverity.Contains(trait.Severity.Value)) return false;
			if (trait.CompatibleModuleTypes.Value.Any() && !trait.CompatibleModuleTypes.Value.Contains(type)) return false;
			if (constraint.IncompatibleIds.Contains(trait.Id.Value)) return false;
			if (constraint.IncompatibleFamilyIds.Intersect(trait.FamilyIds.Value).Any()) return false;
			if (trait.IncompatibleIds.Value.Intersect(constraint.RequiredCompatibleIds).Any()) return false;
			if (trait.IncompatibleFamilyIds.Value.Intersect(constraint.RequiredCompatibleFamilyIds).Any()) return false;

			return true;
		}
		
		#region Child Generators
		protected virtual GenerationInfo GetInfo(
			int seed,
			ModuleTypes type,
			string manufacturerId
		)
		{
			var result = GenerationInfo.Default;
			result.Seed = seed;
			result.Type = type;
			result.ManufacturerId = manufacturerId;
			return result;
		}
		
		protected abstract string GetName(GenerationInfo info);
		protected abstract string GetYearManufactured(GenerationInfo info);
		protected abstract string GetDescription(GenerationInfo info);
		#endregion
	}

	public interface IModuleService
	{
		void Initialize(Action<RequestStatus> done);
		
		ModuleTraitModel GetTrait(string id);

		void Generate(
			ModuleService.ModuleConstraint constraint,
			Action<RequestResult, ModuleModel> done
		);
		
		void AddTraits(
			ModuleModel module,
			ModuleService.TraitConstraint constraint,
			ModuleService.TraitLimit[] traitLimits,
			Action<RequestResult, ModuleModel> done
		);
	}
}