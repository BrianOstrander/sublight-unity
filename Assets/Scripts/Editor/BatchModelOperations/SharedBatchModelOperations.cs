using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class SharedBatchModelOperations
	{
		#region Update Versions
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void UpdateVersions(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done,
			bool write
		)
		{
			UpdateVersionsShared(
				model,
				done,
				write
			);
		}

		[BatchModelOperation(typeof(GalaxyInfoModel))]
		static void UpdateVersions(
			GalaxyInfoModel model,
			Action<GalaxyInfoModel, RequestResult> done,
			bool write
		)
		{
			UpdateVersionsShared(
				model,
				done,
				write
			);
		}

		[BatchModelOperation(typeof(GamemodeInfoModel))]
		static void UpdateVersions(
			GamemodeInfoModel model,
			Action<GamemodeInfoModel, RequestResult> done,
			bool write
		)
		{
			UpdateVersionsShared(
				model,
				done,
				write
			);
		}

		static void UpdateVersionsShared<T>(
			T model,
			Action<T, RequestResult> done,
			bool write
		)
			where T : SaveModel
		{
			var result = GetUnmodifiedResult(model);

			if (model.Version.Value != BuildPreferences.Instance.Info.Version)
			{
				result = GetModifiedResult(
					model,
					1,
					1,
					ModificationPrefix + "Updated version from " + model.Version.Value + " to " + BuildPreferences.Instance.Info.Version
				);
			}

			done(model, RequestResult.Success(result));
		}
		#endregion

		#region Consolidate Ids
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void ConsolidateIds(EncounterInfoModel model, Action<EncounterInfoModel, RequestResult> done, bool write)
		{
			ConsolidateIdsShared(
				model,
				done,
				write,
				model.Id.Value
			);
		}

		[BatchModelOperation(typeof(GalaxyInfoModel))]
		static void ConsolidateIds(GalaxyInfoModel model, Action<GalaxyInfoModel, RequestResult> done, bool write)
		{
			ConsolidateIdsShared(
				model,
				done,
				write,
				model.Id.Value
			);
		}

		[BatchModelOperation(typeof(GamemodeInfoModel))]
		static void ConsolidateIds(GamemodeInfoModel model, Action<GamemodeInfoModel, RequestResult> done, bool write)
		{
			ConsolidateIdsShared(
				model,
				done,
				write,
				model.Id.Value
			);
		}

		static void ConsolidateIdsShared<T>(
			T model,
			Action<T, RequestResult> done,
			bool write,
			string id
		)
			where T : SaveModel
		{
			var result = GetUnmodifiedResult(model);

			var modifications = new List<string>();
			var errors = new List<string>();

			void CheckMetaId(string key)
			{
				if (model.MetaKeyValues.Value.ContainsKey(key))
				{
					modifications.Add(ModificationPrefix + "Removing Id MetaKey: " + model.GetMetaKey(key));
					if (write) model.SetMetaKey(key, null);
				}
			}

			switch (model.SaveType)
			{
				case SaveTypes.EncounterInfo:
					CheckMetaId("EncounterId");
					break;
				case SaveTypes.GalaxyInfo:
					CheckMetaId("GalaxyId");
					break;
				case SaveTypes.GamemodeInfo:
					CheckMetaId("GamemodeId");
					break;
				default:
					errors.Add(ModificationPrefix + "Unrecognized SaveType: " + model.SaveType);
					break;
			}

			if (model.Id.Value != id)
			{
				modifications.Add(ModificationPrefix + "Updated id to " + id);

				if (write) model.Id.Value = id;
			}

			if (modifications.Any() || errors.Any())
			{
				var allModifications = string.Empty;
				foreach (var modification in modifications) allModifications += modification;
				var allErrors = string.Empty;
				foreach (var error in errors) allErrors += error;

				result = GetModifiedResult(
					model,
					modifications.Count,
					2,
					allModifications,
					allErrors
				);
			}

			if (errors.None()) done(model, RequestResult.Success(result));
			else done(model, RequestResult.Failure(result));
		}
		#endregion
		
		#region Consolidate Filenames
		
		private const string ConsolidateFilenamesDescription = "Renames files so their id is equal to their filename. WARNING: This is possibly destructive!";
		
		[BatchModelOperation(typeof(EncounterInfoModel), description: ConsolidateFilenamesDescription)]
		static void ConsolidateFilenames(EncounterInfoModel model, Action<EncounterInfoModel, RequestResult> done, bool write)
		{
			ConsolidateFilenamesShared(
				model,
				done,
				write
			);
		}

		[BatchModelOperation(typeof(GalaxyInfoModel), description: ConsolidateFilenamesDescription)]
		static void ConsolidateFilenames(GalaxyInfoModel model, Action<GalaxyInfoModel, RequestResult> done, bool write)
		{
			ConsolidateFilenamesShared(
				model,
				done,
				write
			);
		}

		[BatchModelOperation(typeof(GamemodeInfoModel), description: ConsolidateFilenamesDescription)]
		static void ConsolidateFilenames(GamemodeInfoModel model, Action<GamemodeInfoModel, RequestResult> done, bool write)
		{
			ConsolidateFilenamesShared(
				model,
				done,
				write
			);
		}

		static void ConsolidateFilenamesShared<T>(
			T model,
			Action<T, RequestResult> done,
			bool write
		)
			where T : SaveModel
		{
			var result = GetUnmodifiedResult(model);

			
			var modifications = new List<string>();
			var errors = new List<string>();
			
			string SanitizeName(string name)
			{
				return name
					.Replace(":", "")
					.Replace("-", "")
					.Replace("  ", " ")
					.Replace(' ', '_')
					.ToLower();
			}
			
			string GetName()
			{
				switch (model.SaveType)
				{
					case SaveTypes.EncounterInfo:
						return (model as EncounterInfoModel).Name;
					case SaveTypes.GalaxyInfo:
						return (model as GalaxyInfoModel).Name;
					case SaveTypes.GamemodeInfo:
//						return (model as GamemodeInfoModel).Name;
						return null;
					default:
						errors.Add(ModificationPrefix + "Unrecognized SaveType: " + model.SaveType);
						return null;
				}
			}
			
			void SetName(string name)
			{
				switch (model.SaveType)
				{
					case SaveTypes.EncounterInfo:
						(model as EncounterInfoModel).Name.Value = name;
						break;
					case SaveTypes.GalaxyInfo:
						(model as GalaxyInfoModel).Name.Value = name;
						break;
					case SaveTypes.GamemodeInfo:
//						(model as GamemodeInfoModel).Name.Value = name;
						break;
					default:
						errors.Add(ModificationPrefix + "Unrecognized SaveType: " + model.SaveType);
						break;
				}
			}

			var oldName = GetName();
			var oldId = model.Id.Value;
			var newName = SanitizeName(oldName);
			var newId = newName;

			if (oldName != newName)
			{
				var currModification = ModificationPrefix + "Renaming...";
				currModification += ModificationPrefix + "\tFrom: \t" + oldName;
				currModification += ModificationPrefix + "\tTo: \t" + newName;
				modifications.Add(currModification);
//				modifications.Add(ModificationPrefix + "Renaming from \"" + oldName + "\" to \"" + newName + "\"");
				if (write) SetName(newName);
			}

			if (oldId != newId)
			{
				var oldPath = model.Path.Value;
				var newPath = Path.Combine(Directory.GetParent(model.Path.Value).FullName, newId + Path.GetExtension(oldPath));

				var oldSiblingDirectory = model.SiblingDirectory;
				var newSiblingDirectory = oldSiblingDirectory == null
					? null
					: Path.Combine(
						  Directory.GetParent(newPath).FullName,
						  Path.GetFileNameWithoutExtension(newPath)
					  ) + Path.DirectorySeparatorChar;

				var currModification = ModificationPrefix + "Reassigning Id...";
				currModification += ModificationPrefix + "\tFrom: \t" + oldId;
				currModification += ModificationPrefix + "\tTo: \t" + newId;
				
				modifications.Add(currModification);

				currModification = ModificationPrefix + "Moving file...";
				currModification += ModificationPrefix + "\tFrom: \t" + oldPath;
				currModification += ModificationPrefix + "\tTo: \t" + newPath;
				
				modifications.Add(currModification);
				
//				modifications.Add(ModificationPrefix + "Reassigning Id from \"" + oldId + "\" to \"" + newId + "\"");
//				modifications.Add(ModificationPrefix + "Moving file from \"" + oldPath + "\" to \"" + newPath + "\"");
				
				if (write)
				{
					model.Id.Value = newId;
					model.Path.Value = newPath; 
					File.Delete(oldPath);
				}

				if (oldSiblingDirectory != newSiblingDirectory)
				{
					currModification = ModificationPrefix + "Moving sibling directory...";
					currModification += ModificationPrefix + "\tFrom: \t" + oldSiblingDirectory;
					currModification += ModificationPrefix + "\tTo: \t" + newSiblingDirectory;
				
					modifications.Add(currModification);
					
//					modifications.Add(ModificationPrefix + "Moving sibling directory from \"" + oldSiblingDirectory + "\" to \"" + newSiblingDirectory + "\"");
					if (write)
					{
						Directory.Move(oldSiblingDirectory, newSiblingDirectory);
					}
				}
			}

			if (modifications.Any() || errors.Any())
			{
				var allModifications = string.Empty;
				foreach (var modification in modifications) allModifications += modification;
				var allErrors = string.Empty;
				foreach (var error in errors) allErrors += error;

				result = GetModifiedResult(
					model,
					modifications.Count,
					model.HasSiblingDirectory ? 4 : 3,
					allModifications,
					allErrors
				);
			}
			
			if (errors.None()) done(model, RequestResult.Success(result));
			else done(model, RequestResult.Failure(result));
		}
		#endregion

		#region Shared
		public const string ModificationPrefix = "\n\t";

		public static string GetUnmodifiedResult(SaveModel model)
		{
			return GetName(model) + " was unmodified...";
		}

		public static string GetModifiedResult(
			SaveModel model,
			int modificationCount,
			int possibleModificationCount,
			string modifications,
			string errors = null
		)
		{
			if (modificationCount == 0 && possibleModificationCount == 0) return GetUnmodifiedResult(model);
			var result = GetName(model) + " had " + modificationCount + " of " + possibleModificationCount + " matches modified..." + modifications;
			if (string.IsNullOrEmpty(errors)) return result;
			return result += "\n\n\tErrors:" + errors;
		}

		public static string GetName(SaveModel model)
		{
			return "\"" + (string.IsNullOrEmpty(model.Meta.Value) ? ShortenValue(model.Path.Value) : model.Meta.Value) + "\"";
		}

		public static string ShortenValue(
			string value,
			int maximumLength = 64
		)
		{
			maximumLength = Mathf.Max(maximumLength, 2);

			if (string.IsNullOrEmpty(value)) return value;
			if (value.Length < maximumLength) return value;

			var begin = value.Substring(0, maximumLength / 2);
			var end = value.Substring(value.Length - (maximumLength / 2));

			return begin + " . . . " + end;
		}
		#endregion
	}
}