﻿using System;
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
		
		[Serializable]
		public struct ModuleConstraint
		{
			public static ModuleConstraint Default => new ModuleConstraint
			{
				ValidTypes = new ModuleTypes[0],
				PowerProduction = FloatRange.Zero,
				PowerConsumption = FloatRange.Zero,
				TransitRange = FloatRange.Zero,
				TransitVelocity = FloatRange.Zero,
				RepairCost = FloatRange.Zero,
				Tags = new string[0],
				ManufacturerIds = new string[0] 
			};

			public ModuleTypes[] ValidTypes;
			public FloatRange PowerProduction;
			public FloatRange PowerConsumption;
			/// <summary>
			/// Maximum transit range in universe units.
			/// </summary>
			/// <remarks>
			/// This may not be the actual maximum range a player can travel per turn,
			/// but rather the maximum before certain encounters are triggered.
			/// </remarks>
			public FloatRange TransitRange;
			/// <summary>
			/// The maximum velocity as a fraction of the speed of light.
			/// </summary>
			public FloatRange TransitVelocity;
			public FloatRange RepairCost;
			public string[] Tags;
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

		[Serializable]
		public struct TraitLimit
		{
			public static TraitLimit WithSeverities(params ModuleTraitSeverity[] validSeverities)
			{
				return new TraitLimit(validSeverities);
			}
			
			public static TraitLimit WithTraitIds(params string[] validTraitIds)
			{
				return new TraitLimit(validTraitIds: validTraitIds);
			}
			
			public static TraitLimit WithTraitFamilyIds(params string[] validTraitFamilyIds)
			{
				return new TraitLimit(validTraitFamilyIds: validTraitFamilyIds);
			}
			
			/// <summary>
			/// The valid severity for this limit, specifying Unknown indicates any is valid.
			/// </summary>
			public ModuleTraitSeverity[] ValidSeverities;
			/// <summary>
			/// Only traits with these ids are valid.
			/// </summary>
			public string[] ValidTraitIds;
			/// <summary>
			/// Only traits with these family ids are valid.
			/// </summary>
			public string[] ValidTraitFamilyIds;
			/// <summary>
			/// The order in which this is sorted if in a list of other entries.
			/// </summary>
			public int Order;
			
			public TraitLimit(
				ModuleTraitSeverity[] validSeverities = null,
				string[] validTraitIds = null,
				string[] validTraitFamilyIds = null,
				int order = 0
			)
			{
				ValidSeverities = validSeverities ?? new ModuleTraitSeverity[0];
				ValidTraitIds = validTraitIds ?? new string[0];
				ValidTraitFamilyIds = validTraitFamilyIds ?? new string[0];
				Order = order;
			}
		}

		[Serializable]
		public struct ModuleConstraintWithTraitLimits
		{
			public ModuleConstraint Constraint;
			public TraitLimit[] Limits;

			public ModuleConstraintWithTraitLimits(
				ModuleConstraint constraint,
				params TraitLimit[] limits
			)
			{
				Constraint = constraint;
				Limits = limits;
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
				done(Result<IModuleService>.Failure(error));
				return;
			}

			Traits = results.Models.Select(m => m.TypedModel).ToList();
			Initialized = true;

			done(Result<IModuleService>.Success(this));
		}
		#endregion

		public ModuleTraitModel GetTrait(string id) => Traits.FirstOrDefault(t => t.Id.Value == id);
		public ModuleTraitModel[] GetTraits(Func<ModuleTraitModel, bool> predicate) => Traits.Where(predicate).ToArray();
		public ModuleTraitModel[] GetTraits(params string[] ids) => GetTraits(t => ids.Contains(t.Id.Value));
		public ModuleTraitModel[] GetTraitsByFamilyId(string familyId) => GetTraits(t => t.FamilyIds.Value.Contains(familyId));

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

			result.Id.Value = Guid.NewGuid().ToString();
			result.Type.Value = info.Type;
			result.Name.Value = GetName(info);
			result.Tags.Value = constraint.Tags ?? new string[0];
			result.YearManufactured.Value = GetYearManufactured(info);
			result.Description.Value = GetDescription(info);
			result.Tags.Value = constraint.Tags;
			
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
					result.TransitRange.Value = random.GetNextFloat(
						constraint.TransitRange.Primary,
						constraint.TransitRange.Secondary
					);
					break;
				case ModuleTypes.Propulsion:
					result.TransitVelocity.Value = random.GetNextFloat(
						constraint.TransitVelocity.Primary,
						constraint.TransitVelocity.Secondary
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
									nameof(GenerateModule) + " failed with status: " + result.Status + " and error: " + result.Error,
									results.ToArray()
								).Log()
							);
							return;
						}

						if (results.Count == constraints.Length) done(ResultArray<ModuleModel>.Success(results.ToArray()));
					}
				);
			}
		}

		/// <summary>
		/// Generates traits on the specified module.
		/// </summary>
		/// <remarks>
		///	The order in which you specify trait limits determines the bias for selecting traits.
		/// Ones earlier in the list are chosen first, which may exclude later ones. Undecided if this is always going to be the case.
		/// </remarks>
		/// <param name="module"></param>
		/// <param name="done"></param>
		/// <param name="traitLimits"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void GenerateTraits(
			ModuleModel module,
			Action<Result<Payloads.GenerateTraits>> done,
			params TraitLimit[] traitLimits
		)
		{
			CheckInitialization();
			if (module == null) throw new ArgumentNullException(nameof(module));
			if (done == null) throw new ArgumentNullException(nameof(done));

			OnGenerateTraits(
				module,
				GetTraitConstraint(module),
				traitLimits.OrderBy(t => t.Order).ToList(),
				new List<ModuleTraitModel>(), 
				done
			);
		}
		
		void OnGenerateTraits(
			ModuleModel module,
			TraitConstraint traitConstraint,
			List<TraitLimit> remaining,
			List<ModuleTraitModel> appended,
			Action<Result<Payloads.GenerateTraits>> done
		)
		{
			if (remaining.None())
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

			var next = remaining.First();
			remaining.RemoveAt(0);
			
			var possibleTraits = Traits.AsEnumerable();

			if (next.ValidTraitIds.Any()) possibleTraits = possibleTraits.Where(t => next.ValidTraitIds.Contains(t.Id.Value));
			if (next.ValidTraitFamilyIds.Any()) possibleTraits = possibleTraits.Where(t => t.FamilyIds.Value.Any(f => next.ValidTraitFamilyIds.Contains(f)));
			if (next.ValidSeverities.Any()) possibleTraits = possibleTraits.Where(t => next.ValidSeverities.Contains(t.Severity.Value));

			possibleTraits = possibleTraits.Where(t => IsTraitValid(t, traitConstraint));

			if (possibleTraits.Any())
			{
				var selectedTrait = possibleTraits.Random();
				AppendTraitToModule(selectedTrait, module);
				appended.Add(selectedTrait);
				AppendTraitToConstraint(selectedTrait, ref traitConstraint);
			}

			OnGenerateTraits(
				module,
				traitConstraint,
				remaining,
				appended,
				done
			);
		}

		public void GenerateModulesWithTraits(
			ModuleConstraintWithTraitLimits[] constraintsWithTraitLimits,
			Action<ResultArray<ModuleModel>> done
		)
		{
			CheckInitialization();
			if (constraintsWithTraitLimits == null) throw new ArgumentNullException(nameof(constraintsWithTraitLimits));
			if (done == null) throw new ArgumentNullException(nameof(done));
			
			OnGenerateModulesWithTraits(
				constraintsWithTraitLimits.ToList(),
				new List<Result<ModuleModel>>(),
				result => done(result.LogIfNotSuccess())
			);
		}

		void OnGenerateModulesWithTraits(
			List<ModuleConstraintWithTraitLimits> remaining,
			List<Result<ModuleModel>> generated,
			Action<ResultArray<ModuleModel>> done
		)
		{
			if (remaining.None())
			{
				done(ResultArray<ModuleModel>.Success(generated.ToArray()));
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);
			
			GenerateModule(
				next.Constraint,
				generateModuleResult =>
				{
					switch (generateModuleResult.Status)
					{
						case RequestStatus.Success:
							GenerateTraits(
								generateModuleResult.Payload,
								generateTraitsResult =>
								{
									switch (generateTraitsResult.Status)
									{
										case RequestStatus.Success:
											generated.Add(Result<ModuleModel>.Success(generateTraitsResult.Payload.Module));
											OnGenerateModulesWithTraits(
												remaining,
												generated,
												done
											);
											break;
										default:
											done(ResultArray<ModuleModel>.Failure(generateTraitsResult.Error));
											break;
									}
								},
								next.Limits
							);
							break;
						default:
							done(ResultArray<ModuleModel>.Failure(generateModuleResult.Error));
							break;
					}
				}
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
		ModuleTraitModel[] GetTraits(Func<ModuleTraitModel, bool> predicate);
		ModuleTraitModel[] GetTraits(params string[] ids);
		ModuleTraitModel[] GetTraitsByFamilyId(string familyId);

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
		
		void GenerateModulesWithTraits(
			ModuleService.ModuleConstraintWithTraitLimits[] constraintsWithTraitLimits,
			Action<ResultArray<ModuleModel>> done
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