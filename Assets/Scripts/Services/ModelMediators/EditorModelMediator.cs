#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
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
		class ValidationEntry
		{
			public string Id;
			public ValidationStates ValidationState;
			public List<string> Issues = new List<string>();
		}
		
		static List<ValidationEntry> Cache = new List<ValidationEntry>();

		public static ValidationStates IsValidModel<M>(string id)
			where M : SaveModel
		{
			if (string.IsNullOrEmpty(id)) return ValidationStates.Invalid;
			
			var cached = Cache.FirstOrDefault(c => c.Id == id);

			if (cached == null)
			{
				cached = new ValidationEntry
				{
					Id = id,
					ValidationState = ValidationStates.Processing
				};
				Cache.Add(cached);
				
				Instance.Load<M>(
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
								var error = string.IsNullOrEmpty(result.Error) ? "Undefined ModelMediator load error" : result.Error;
								cached.Issues = cached.Issues.Append(error).Distinct().ToList();
								break;
						}
					}
				);
			}

			return cached.ValidationState;
		}
		#endregion
	}
}
#endif