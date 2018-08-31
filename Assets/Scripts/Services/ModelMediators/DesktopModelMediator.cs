using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	public class DesktopModelMediator : ModelMediator
	{
		const string Extension = ".json";
		static string ParentPath { get { return Path.Combine(Application.persistentDataPath, "saves"); } }
		static string InternalPath { get { return Path.Combine(Application.streamingAssetsPath, "internal"); } }

		static string InventoryReferencePath(string suffix) { return Path.Combine(Path.Combine(InternalPath, "inventory-references"), suffix); }

		bool readableSaves;

		protected override Dictionary<SaveTypes, int> MinimumSupportedSaves
		{
			get
			{
				// -1 means it only supports saves equal to the current version.
				return new Dictionary<SaveTypes, int>
				{
					{ SaveTypes.Game, -1 },
					{ SaveTypes.Preferences, -1 },
					{ SaveTypes.EncounterInfo, 0 },
					{ SaveTypes.InteractedEncounterInfoList, -1 },
					{ SaveTypes.InteractedInventoryReferenceList, -1 },
					{ SaveTypes.GlobalKeyValues, -1 },
					// -- Inventory References
					{ SaveTypes.ModuleReference, 4 },
					{ SaveTypes.OrbitalCrewReference, 4 },
					// --
					{ SaveTypes.LanguageDatabase, 7 }
				};
			}
		}

		protected override Dictionary<SaveTypes, bool> CanSave
		{
			get
			{
				return new Dictionary<SaveTypes, bool>
				{
					{ SaveTypes.Game, true },
					{ SaveTypes.Preferences, true },
					{ SaveTypes.EncounterInfo, false },
					{ SaveTypes.InteractedEncounterInfoList, true },
					{ SaveTypes.InteractedInventoryReferenceList, true },
					{ SaveTypes.GlobalKeyValues, true },
					// -- Inventory References
					{ SaveTypes.ModuleReference, false },
					{ SaveTypes.OrbitalCrewReference, false },
					// --
					{ SaveTypes.LanguageDatabase, false }
				};
			}
		}

		public DesktopModelMediator(bool readableSaves = false)
		{
			this.readableSaves = readableSaves;
		}

		public override void Initialize(IBuildInfo info, Action<RequestStatus> done)
		{
			BuildInfo = info;
			try
			{
				var canSave = CanSave;
				foreach (var curr in Enum.GetValues(typeof(SaveTypes)).Cast<SaveTypes>())
				{
					if (curr == SaveTypes.Unknown) continue;
					if (!CanSave.ContainsKey(curr) || !CanSave[curr]) continue;

					var path = GetPath(curr);
					Directory.CreateDirectory(path);
				}
				done(RequestStatus.Success);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				done(RequestStatus.Failure);
			}
		}

		string GetPath(SaveTypes saveType)
		{
			switch (saveType)
			{
				case SaveTypes.Game: return Path.Combine(ParentPath, "games");
				case SaveTypes.Preferences: return Path.Combine(ParentPath, "preferences");
				case SaveTypes.EncounterInfo: return Path.Combine(InternalPath, "encounters");
				case SaveTypes.GlobalKeyValues: return Path.Combine(ParentPath, "global-kv");
				// -- Interacted
				case SaveTypes.InteractedEncounterInfoList: return Path.Combine(ParentPath, "interacted-encounters");
				case SaveTypes.InteractedInventoryReferenceList: return Path.Combine(ParentPath, "interacted-references");
				// -- Inventory References
				case SaveTypes.ModuleReference: return InventoryReferencePath("modules");
				case SaveTypes.OrbitalCrewReference: return InventoryReferencePath("orbital-crew");
				// --
				case SaveTypes.LanguageDatabase: return Path.Combine(InternalPath, "languages");
				default: throw new ArgumentOutOfRangeException("saveType", saveType + " is not handled.");
			}
		}

		protected override string GetUniquePath(SaveTypes saveType)
		{
			var path = Path.Combine(GetPath(saveType), Guid.NewGuid().ToString() + Extension);

			// TODO: Check that path doesn't exist.

			return path;
		}

		protected override void OnLoad<M>(SaveModel model, Action<SaveLoadRequest<M>> done)
		{
			var result = Serialization.DeserializeJson<M>(File.ReadAllText(model.Path));
			if (result == null)
			{
				done(SaveLoadRequest<M>.Failure(model, null, "Null result"));
				return;
			}

			result.SupportedVersion.Value = model.SupportedVersion;
			result.Path.Value = model.Path;
			done(SaveLoadRequest<M>.Success(model, result));
		}

		protected override void OnSave<M>(M model, Action<SaveLoadRequest<M>> done = null)
		{
			File.WriteAllText(model.Path, Serialization.Serialize(model, formatting: readableSaves ? Formatting.Indented : Formatting.None));
			done(SaveLoadRequest<M>.Success(model, model));
		}

		protected override void OnList<M>(Action<SaveLoadArrayRequest<SaveModel>> done)
		{
			var path = GetPath(ToEnum(typeof(M)));
			var results = new List<SaveModel>();
			foreach (var file in Directory.GetFiles(path))
			{
				try
				{
					if (Path.GetExtension(file) != Extension) continue;
					var result = Serialization.DeserializeJson<SaveModel>(File.ReadAllText(file));
					if (result == null) continue;

					result.SupportedVersion.Value = IsSupportedVersion(result.SaveType, result.Version);
					result.Path.Value = file;
					results.Add(result);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			var array = results.ToArray();
			done(SaveLoadArrayRequest<SaveModel>.Success(array));
		}

		protected override void OnDelete<M>(M model, Action<SaveLoadRequest<M>> done)
		{
			File.Delete(model.Path);
			done(SaveLoadRequest<M>.Success(model, model));
		}
	}
}