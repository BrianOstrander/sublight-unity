#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using LunraGames.SubLight.Models;
using UnityEngine;

namespace LunraGames.SubLight
{
	public class EditorModelMediator : DesktopModelMediator 
	{
		public enum ValidationStates
		{
			Unknown = 0,
			Processing = 10,
			Valid = 20,
			Invalid = 30
		}
		
		static EditorModelMediator instance;
		public static EditorModelMediator Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EditorModelMediator(true);
					instance.Initialize(BuildPreferences.Instance.Info, instance.OnInstanceInitialized);
				}
				return instance;
			}
		}

		void OnInstanceInitialized(RequestStatus status)
		{
			switch (status)
			{
				case RequestStatus.Success: break;
				default:
					Debug.LogError("Editor time save load service returned: " + status);
					return;
			}
		}

		protected override bool SuppressErrorLogging => true;
		
		Dictionary<SaveTypes, bool> CanSaveOverrides
		{
			get
			{
				return new Dictionary<SaveTypes, bool>
				{
					{ SaveTypes.EncounterInfo, true },
					{ SaveTypes.GalaxyInfo, true },
					{ SaveTypes.GamemodeInfo, true },
					{ SaveTypes.ModuleTrait, true }
					// --
				};
			}
		}

		protected override Dictionary<SaveTypes, bool> CanSave
		{
			get
			{
				var dict = base.CanSave;
				var overrideDict = CanSaveOverrides;

				foreach (var kv in overrideDict)
				{
					if (dict.ContainsKey(kv.Key)) dict[kv.Key] = kv.Value;
					else dict.Add(kv.Key, kv.Value);
				}

				return dict;
			}
		}

		public EditorModelMediator(bool readableSaves = false) : base(readableSaves) {}
		
		#region Utility
		public class Validation
		{
			public string Id;
			public SaveTypes[] Types = new SaveTypes[0];
			public ValidationStates ValidationState;
			public List<string> Issues = new List<string>();

			public Color? Color
			{
				get
				{
					switch (ValidationState)
					{
						case ValidationStates.Valid: return null;
						case ValidationStates.Invalid: return UnityEngine.Color.red;
						default: return UnityEngine.Color.yellow;
					}
				}
			}
			
			public string Tooltip
			{
				get
				{
					var result = "Currently " + ValidationState;
					if (Issues.Any())
					{
						result += "\nThere are " + Issues.Count() + " issue(s)";
						foreach (var issue in Issues) result += "\n - " + issue;
					}
					return result;
				}
			}
		}

		static class DefaultValidations
		{
			public static Validation NullOrEmptyId = new Validation
			{
				Id = null,
				Types = EnumExtensions.GetValues<SaveTypes>(),
				ValidationState = ValidationStates.Invalid,
				Issues = new List<string>( new [] {"No id specified"})
			};
		}
		
		List<Validation> modelValidationCache = new List<Validation>();

		public Validation IsValidModel<M>(string id)
			where M : SaveModel
		{
			if (string.IsNullOrEmpty(id)) return DefaultValidations.NullOrEmptyId;
			
			var cached = modelValidationCache.FirstOrDefault(c => c.Id == id && ToEnum(typeof(M)).IntersectEqual(c.Types));

			if (cached == null)
			{
				cached = new Validation
				{
					Id = id,
					Types = ToEnum(typeof(M)),
					ValidationState = ValidationStates.Processing
				};
				modelValidationCache.Add(cached);
				
				Load<M>(
					id,
					result =>
					{
						switch (result.Status)
						{
							case RequestStatus.Success:
								cached.ValidationState = ValidationStates.Valid;
								break;
							default:
								cached.ValidationState = ValidationStates.Invalid;
								var error = string.IsNullOrEmpty(result.Error) ? "Undefined ModelMediator Load error" : result.Error;
								cached.Issues = cached.Issues.Append(error).Distinct().ToList();
								break;
						}
					}
				);
			}

			return cached;
		}

		List<Validation> moduleTraitFamilyIdValidationCache = new List<Validation>();
		
		public Validation IsValidModuleTraitFamilyId(string familyId)
		{
			if (string.IsNullOrEmpty(familyId)) return DefaultValidations.NullOrEmptyId;
			
			var cached = moduleTraitFamilyIdValidationCache.FirstOrDefault(c => c.Id == familyId);

			if (cached == null)
			{
				cached = new Validation
				{
					Id = familyId,
					ValidationState = ValidationStates.Processing,
					Issues = new List<string>()
				};
				modelValidationCache.Add(cached);
				
				LoadAll<ModuleTraitModel>(
					result =>
					{
						switch (result.Status)
						{
							case RequestStatus.Success:
								foreach (var model in result.TypedModels)
								{
									if (model.FamilyIds.Value.Contains(familyId))
									{
										cached.ValidationState = ValidationStates.Valid;
										return;
									}
								}
								cached.ValidationState = ValidationStates.Invalid;
								cached.Issues = cached.Issues.Append("No " + nameof(ModuleTraitModel) + " specifies FamilyId: " + familyId).Distinct().ToList();
								break;
							default:
								cached.ValidationState = ValidationStates.Invalid;
								var error = string.IsNullOrEmpty(result.Error) ? "Undefined ModelMediator LoadAll error" : result.Error;
								cached.Issues = cached.Issues.Append(error).Distinct().ToList();
								break;
						}
					}
				);
			}

			return cached;
		}
		#endregion
	}
}
#endif