using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine;
using UnityEngine.Assertions;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;
using UnityEditor;

namespace LunraGames.SubLight
{
	public abstract class ModuleService : IModuleService
	{
		public static class Payloads
		{
			public struct GenerateTraits
			{
				public ModuleModel Module;
				public ModuleTraitModel[] Appended;
				
				public override string ToString() => this.ToReadableJson();
			}
			
			public struct AppendTraits
			{
				public ModuleModel Module;
				public ModuleTraitModel[] Appended;
				public ModuleTraitModel[] Ignored;
				
				public override string ToString() => this.ToReadableJson();
			}
			
			public struct CanAppendTraits
			{
				public ModuleModel Module;
				public ModuleTraitModel[] Valid;
				public ModuleTraitModel[] Invalid;

				public override string ToString() => this.ToReadableJson();
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
				ManufacturerIds = new string[0] 
			};

			public ModuleTypes[] ValidTypes;
			public FloatRange PowerProduction;
			public FloatRange PowerConsumption;
			public FloatRange NavigationRange;
			public FloatRange NavigationVelocity;
			public FloatRange RepairCost;
			public string[] ManufacturerIds;
			
			public override string ToString() => this.ToReadableJson();
		}

		public struct TraitConstraint
		{
			public static TraitConstraint Default => new TraitConstraint
			{
				ValidTypes = new ModuleTypes[0],
				ExistingIds = new string[0],
				IncompatibleIds = new string[0],
				IncompatibleFamilyIds = new string[0],
				RequiredCompatibleIds = new string[0],
				RequiredCompatibleFamilyIds = new string[0]
			};

			public ModuleTypes[] ValidTypes;
			public string[] ExistingIds;
			public string[] IncompatibleIds;
			public string[] IncompatibleFamilyIds;
			public string[] RequiredCompatibleIds;
			public string[] RequiredCompatibleFamilyIds;
			
			public override string ToString() => this.ToReadableJson();
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

		protected void CheckInitialization()
		{
			if (!Initialized) throw new Exception(typeof(ModeService).Name + " has not been initialized");
		}
		
		public ModuleService(
			IModelMediator modelMediator
		)
		{
			ModelMediator = modelMediator ?? throw new ArgumentNullException(nameof(modelMediator));
		}
		
		#region Initialization
		public void Initialize(
			Action<Result<IModuleService>> done
		)
		{
			if (done == null) throw new ArgumentNullException(nameof(done));
			
			ModelMediator.LoadAll<ModuleTraitModel>(results => OnInitializeLoadedAll(results, done));
		}

		void OnInitializeLoadedAll(
			ModelArrayResult<ModuleTraitModel> results,
			Action<Result<IModuleService>> done
		)
		{
			if (results.Status != RequestStatus.Success)
			{
				var error = "Loading all module traits failed with status: " + results.Status + " and error: " + results.Error;
				Debug.LogError(error);
				done(Result<IModuleService>.Failure(default, error));
				return;
			}

			Traits = results.Models.Select(m => m.TypedModel).ToList();
			Initialized = true;

			done(Result<IModuleService>.Success(this));
		}
		#endregion

		public ModuleTraitModel GetTrait(string id) => Traits.FirstOrDefault(t => t.Id.Value == id);

		public void GenerateModule(
			ModuleConstraint constraint,
			Action<Result<ModuleModel>> done
		)
		{
			CheckInitialization();
			if (constraint.ValidTypes == null) throw new ArgumentNullException(nameof(constraint.ValidTypes));
			if (constraint.ManufacturerIds == null) throw new ArgumentNullException(nameof(constraint.ManufacturerIds));

			if (constraint.ValidTypes.None()) constraint.ValidTypes = EnumExtensions.GetValues(ModuleTypes.Unknown);
			if (constraint.ManufacturerIds.None()) constraint.ManufacturerIds = new string[] { null }; // TODO: Actually list manufacturer ids

			if (done == null) throw new ArgumentNullException(nameof(done));
			
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
			
			done(Result<ModuleModel>.Success(result));
		}

		public void GenerateModules(
			ModuleConstraint[] constraints,
			Action<ResultArray<ModuleModel>> done
		)
		{
			var results = new List<Result<ModuleModel>>();
			foreach (var constraint in constraints)
			{
				GenerateModule(
					constraint,
					result =>
					{
						results.Add(result);
						if (result.Status != RequestStatus.Success)
						{
							done(
								ResultArray<ModuleModel>.Failure(
									results.ToArray(),
									nameof(GenerateModule) + " failed with status: " + result.Status + " and error: " + result.Error
								).Log()
							);
							return;
						}

						if (results.Count == constraints.Length) done(ResultArray<ModuleModel>.Success(results.ToArray()));
					}
				);
			}
		}

		public void GenerateTraits(
			ModuleModel module,
			Action<Result<Payloads.GenerateTraits>> done,
			params TraitLimit[] traitLimits
		)
		{
			CheckInitialization();
			if (module == null) throw new ArgumentNullException(nameof(module));
			if (done == null) throw new ArgumentNullException(nameof(done));

			var traitConstraint = GetTraitConstraint(module);
			
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

			OnGenerateTraits(
				null,
				module,
				traitConstraint,
				remaining,
				new List<ModuleTraitModel>(), 
				done
			);
		}
		
		void OnGenerateTraits(
			ModuleTraitModel result,
			ModuleModel module,
			TraitConstraint traitConstraint,
			Dictionary<ModuleTraitSeverity, int> remaining,
			List<ModuleTraitModel> appended,
			Action<Result<Payloads.GenerateTraits>> done
		)
		{
			if (result != null)
			{
				AppendTraitToModule(result, module);
				appended.Add(result);
				AppendTraitToConstraint(result, ref traitConstraint);
				remaining[result.Severity] = remaining[result.Severity] - 1;
			}

			var validSeverity = remaining.Where(kv => 0 < kv.Value).Select(kv => kv.Key);
			
			var nextTrait = validSeverity.None() ? null : Traits.Where(t => validSeverity.Contains(t.Severity.Value)).Where(t => IsTraitValid(t, traitConstraint)).Random();
			
			if (nextTrait == null)
			{
				done(
				Result<Payloads.GenerateTraits>.Success(
						new Payloads.GenerateTraits
						{
							Module = module,
							Appended = appended.ToArray()
						}
					)
				);
				return;	
			}
			
			OnGenerateTraits(
				nextTrait,
				module,
				traitConstraint,
				remaining,
				appended,
				done
			);
		}
		
		public void AppendTraits(
			ModuleModel module,
			Action<Result<Payloads.AppendTraits>> done,
			params string[] ids
		)
		{
			CheckInitialization();
			if (module == null) throw new ArgumentNullException(nameof(module));
			if (done == null) throw new ArgumentNullException(nameof(done));
			
			CanAppendTraits(
				module,
				result => OnAppendTraits(result, done),
				ids
			);
		}

		void OnAppendTraits(
			Result<Payloads.CanAppendTraits> result,
			Action<Result<Payloads.AppendTraits>> done
		)
		{
			if (result.Status != RequestStatus.Success)
			{
				done(
					Result<Payloads.AppendTraits>.Failure(
						default,
						nameof(CanAppendTraits) + " failed with status: " + result.Status + " and error: " + result.Error
					).Log()
				);
				return;	
			}

			foreach (var validTrait in result.Payload.Valid) AppendTraitToModule(validTrait, result.Payload.Module);
			
			done(
				Result<Payloads.AppendTraits>.Success(
					new Payloads.AppendTraits
					{
						Module = result.Payload.Module,
						Appended = result.Payload.Valid,
						Ignored = result.Payload.Invalid
					}
				)
			);
		}
		
		public void CanAppendTraits(
			ModuleModel module,
			Action<Result<Payloads.CanAppendTraits>> done,
			params string[] ids
		)
		{
			CheckInitialization();
			if (module == null) throw new ArgumentNullException(nameof(module));
			if (done == null) throw new ArgumentNullException(nameof(done));
			
			var traitConstraint = GetTraitConstraint(module);
			
			var valid = new List<ModuleTraitModel>();
			var invalid = new List<ModuleTraitModel>();

			foreach (var id in ids)
			{
				var current = GetTrait(id);
				if (current == null)
				{
					done(
						Result<Payloads.CanAppendTraits>.Failure(
							default,
							"No ModuleTrait with id \"" + id + "\" found"
						).Log()
					);
					return;
				}

				if (IsTraitValid(current, traitConstraint))
				{
					valid.Add(current);
					AppendTraitToConstraint(current, ref traitConstraint);
				}
				else invalid.Add(current);
			}

			done(
				Result<Payloads.CanAppendTraits>.Success(
					new Payloads.CanAppendTraits
					{
						Module = module,
						Valid = valid.ToArray(),
						Invalid = invalid.ToArray()
					}
				)
			);
		}

		bool IsTraitValid(
			ModuleTraitModel trait,
			TraitConstraint constraint
		)
		{
			if (constraint.ExistingIds.Contains(trait.Id.Value)) return false;
			if (trait.CompatibleModuleTypes.Value.Any() && constraint.ValidTypes.Any() && constraint.ValidTypes.Intersect(trait.CompatibleModuleTypes.Value).None()) return false;
			if (constraint.IncompatibleIds.Contains(trait.Id.Value)) return false;
			if (constraint.IncompatibleFamilyIds.Intersect(trait.FamilyIds.Value).Any()) return false;
			if (trait.IncompatibleIds.Value.Intersect(constraint.RequiredCompatibleIds).Any()) return false;
			if (trait.IncompatibleFamilyIds.Value.Intersect(constraint.RequiredCompatibleFamilyIds).Any()) return false;

			return true;
		}

		TraitConstraint GetTraitConstraint(ModuleModel module)
		{
			var result = TraitConstraint.Default;

			result.ValidTypes = result.ValidTypes.Append(module.Type.Value).ToArray();
			result.ExistingIds = module.TraitIds.Value;
			
			foreach (var trait in Traits.Where(t => module.TraitIds.Value.Contains(t.Id.Value)))
			{
				AppendTraitToConstraint(trait, ref result);
			}

			return result;
		}

		void AppendTraitToConstraint(
			ModuleTraitModel trait,
			ref TraitConstraint traitConstraint
		)
		{
			traitConstraint.ExistingIds = traitConstraint.ExistingIds.Append(trait.Id.Value).Distinct().ToArray();
			traitConstraint.IncompatibleIds = traitConstraint.IncompatibleIds.Union(trait.IncompatibleIds.Value).ToArray();
			traitConstraint.IncompatibleFamilyIds = traitConstraint.IncompatibleFamilyIds.Union(trait.IncompatibleFamilyIds.Value).ToArray();
			traitConstraint.RequiredCompatibleIds = traitConstraint.RequiredCompatibleIds.Append(trait.Id.Value).ToArray();
			traitConstraint.RequiredCompatibleFamilyIds = traitConstraint.RequiredCompatibleFamilyIds.Union(trait.FamilyIds.Value).ToArray();
		}

		void AppendTraitToModule(
			ModuleTraitModel trait,
			ModuleModel module
		)
		{
			module.TraitIds.Value = module.TraitIds.Value.Append(trait.Id.Value).ToArray();
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
		void Initialize(Action<Result<IModuleService>> done);
		
		ModuleTraitModel GetTrait(string id);

		void GenerateModule(
			ModuleService.ModuleConstraint constraint,
			Action<Result<ModuleModel>> done
		);
		
		void GenerateModules(
			ModuleService.ModuleConstraint[] constraints,
			Action<ResultArray<ModuleModel>> done
		);
		
		void GenerateTraits(
			ModuleModel module,
			Action<Result<ModuleService.Payloads.GenerateTraits>> done,
			params ModuleService.TraitLimit[] traitLimits
		);

		void AppendTraits(
			ModuleModel module,
			Action<Result<ModuleService.Payloads.AppendTraits>> done,
			params string[] ids
		);

		void CanAppendTraits(
			ModuleModel module,
			Action<Result<ModuleService.Payloads.CanAppendTraits>> done,
			params string[] ids
		);
	}
}