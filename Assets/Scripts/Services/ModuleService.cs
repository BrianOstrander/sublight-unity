using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;
using UnityEditor;

namespace LunraGames.SubLight
{
	public abstract class ModuleService : IModuleService
	{
		public struct Result
		{
			public readonly RequestStatus Status;
			public readonly ModuleModel Module;
			public readonly string Error;

			public static Result Success(ModuleModel module)
			{
				return new Result(
					RequestStatus.Success,
					module
				);
			}

			public static Result Failure(ModuleModel module, string error)
			{
				return new Result(
					RequestStatus.Failure,
					module,
					error
				);
			}

			Result(
				RequestStatus status,
				ModuleModel module,
				string error = null
			)
			{
				Status = status;
				Module = module;
				Error = error;
			}
		}
		
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
			
			public TraitLimit(
				ModuleTraitSeverity severity,
				int countMinimum = 1,
				int countMaximum = 1
			)
			{
				Severity = severity;
				Count = new IntegerRange(countMinimum, countMaximum);
			}
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
		
		protected List<ModuleTraitModel> Traits { get; private set; }
		protected bool Initialized { get; private set; }

		protected bool IsNotInitialized(Action<Result> done)
		{
			if (!Initialized)
			{
				done(Result.Failure(null, nameof(ModuleService) + " is not initialized"));
				return true;
			}
			return false;
		}
		
		public ModuleService(
			IModelMediator modelMediator
		)
		{
			ModelMediator = modelMediator ?? throw new ArgumentNullException(nameof(modelMediator));
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
			Initialized = true;

			done(RequestStatus.Success);
		}
		#endregion

		public ModuleTraitModel GetTrait(string id) => Traits.FirstOrDefault(t => t.Id.Value == id);

		public void Generate(
			ModuleConstraint constraint,
			Action<Result> done
		)
		{
			if (IsNotInitialized(done)) return;
			if (constraint.ValidTypes == null) throw new ArgumentNullException(nameof(constraint.ValidTypes));
			if (constraint.ManufacturerIds == null) throw new ArgumentNullException(nameof(constraint.ManufacturerIds));
			if (constraint.TraitLimits == null) throw new ArgumentNullException(nameof(constraint.TraitLimits));

			if (constraint.ValidTypes.None()) constraint.ValidTypes = EnumExtensions.GetValues(ModuleTypes.Unknown);
			if (constraint.ManufacturerIds.None()) constraint.ManufacturerIds = new string[] { null }; // TODO: Actually list manufacturer ids

			var result = new ModuleModel();

			var random = new Demon();
			
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
			TraitConstraint traitConstraint,
			TraitLimit[] traitLimits,
			Action<Result> done
		)
		{
			if (IsNotInitialized(done)) return;
			if (traitConstraint.IncompatibleIds == null) throw new ArgumentNullException(nameof(traitConstraint.IncompatibleIds));
			if (traitConstraint.IncompatibleFamilyIds == null) throw new ArgumentNullException(nameof(traitConstraint.IncompatibleFamilyIds));
			if (traitConstraint.RequiredCompatibleIds == null) throw new ArgumentNullException(nameof(traitConstraint.RequiredCompatibleIds));
			if (traitConstraint.RequiredCompatibleFamilyIds == null) throw new ArgumentNullException(nameof(traitConstraint.RequiredCompatibleFamilyIds));
			if (traitConstraint.ValidSeverity == null) throw new ArgumentNullException(nameof(traitConstraint.ValidSeverity));
			
			var remaining = new Dictionary<ModuleTraitSeverity, int>();
			
			var random = new Demon();
			
			foreach (var traitLimit in traitLimits)
			{
				var count = random.GetNextInteger(
					traitLimit.Count.Primary,
					traitLimit.Count.Secondary
				);
				
				if (0 < count)
				{
					if (remaining.ContainsKey(traitLimit.Severity)) Debug.LogError("Module trait Severity \"" + traitLimit.Severity + "\" is specified multiple times, ignoring duplicates");
					else remaining.Add(traitLimit.Severity, count);
				}
			}

			OnAddTraits(
				null,
				module,
				traitConstraint,
				remaining,
				done
			);
		}

		void OnAddTraits(
			ModuleTraitModel result,
			ModuleModel module,
			TraitConstraint traitConstraint,
			Dictionary<ModuleTraitSeverity, int> remaining,
			Action<Result> done
		)
		{
			if (result != null)
			{
				module.TraitIds.Value = module.TraitIds.Value.Append(result.Id.Value).ToArray();
				traitConstraint.IncompatibleIds = traitConstraint.IncompatibleIds.Union(result.IncompatibleIds.Value).ToArray();
				traitConstraint.IncompatibleFamilyIds = traitConstraint.IncompatibleFamilyIds.Union(result.IncompatibleFamilyIds.Value).ToArray();
				traitConstraint.RequiredCompatibleIds = traitConstraint.RequiredCompatibleIds.Append(result.Id.Value).ToArray();
				remaining[result.Severity] = remaining[result.Severity] - 1;
			}

			traitConstraint.ValidSeverity = remaining.Where(kv => 0 < kv.Value).Select(kv => kv.Key).ToArray();

			var nextTrait = traitConstraint.ValidSeverity.None() ? null : Traits.Where(t => IsTraitValid(t, module.Type, traitConstraint)).Random();
			
			if (nextTrait == null)
			{
				done(Result.Success(module));
				return;	
			}
			
			OnAddTraits(
				nextTrait,
				module,
				traitConstraint,
				remaining,
				done
			);
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
			Action<ModuleService.Result> done
		);
		
		void AddTraits(
			ModuleModel module,
			ModuleService.TraitConstraint traitConstraint,
			ModuleService.TraitLimit[] traitLimits,
			Action<ModuleService.Result> done
		);
	}
}