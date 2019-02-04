﻿using System;
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
					// -- Meta Key Values
					{ SaveTypes.GlobalKeyValues, -1 },
					{ SaveTypes.PreferencesKeyValues, -1 },
					// -- Galaxies
					{ SaveTypes.GalaxyPreview, 7 },
					{ SaveTypes.GalaxyDistant, 7 },
					{ SaveTypes.GalaxyInfo, 7 }
					// --
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
					// -- Meta Key Values
					{ SaveTypes.GlobalKeyValues, true },
					{ SaveTypes.PreferencesKeyValues, true },
					// -- Galaxies
					{ SaveTypes.GalaxyPreview, false },
					{ SaveTypes.GalaxyDistant, false },
					{ SaveTypes.GalaxyInfo, false }
					// --
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
				// -- Meta Key Values
				case SaveTypes.GlobalKeyValues: return Path.Combine(ParentPath, "global-kv");
				case SaveTypes.PreferencesKeyValues: return Path.Combine(ParentPath, "preferences-kv");
				// -- Galaxies
				case SaveTypes.GalaxyPreview:
				case SaveTypes.GalaxyDistant:
				case SaveTypes.GalaxyInfo:
					return Path.Combine(InternalPath, "galaxies");
				// -- Interacted
				case SaveTypes.InteractedEncounterInfoList: return Path.Combine(ParentPath, "interacted-encounters");
				// --
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

			if (result.HasSiblingDirectory) LoadSiblingFiles(model, result, done);
			else done(SaveLoadRequest<M>.Success(model, result));
		}

		protected override void OnSave<M>(M model, Action<SaveLoadRequest<M>> done = null)
		{
			File.WriteAllText(model.Path, Serialization.Serialize(model, formatting: readableSaves ? Formatting.Indented : Formatting.None));
			if (model.HasSiblingDirectory) Directory.CreateDirectory(model.SiblingDirectory);
			done(SaveLoadRequest<M>.Success(model, model));
		}

		protected override void OnList<M>(Action<SaveLoadArrayRequest<SaveModel>> done)
		{
			var path = GetPath(ToEnum(typeof(M)).FirstOrDefault());
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

		protected override void OnRead(string path, Action<ReadWriteRequest> done)
		{
			var data = File.ReadAllBytes(path);

			if (data == null)
			{
				done(ReadWriteRequest.Failure(path, "Null result"));
				return;
			}

			done(ReadWriteRequest.Success(path, data));
		}

		#region Sibling Loading
		void LoadSiblingFiles<M>(SaveModel model, M result, Action<SaveLoadRequest<M>> done)
			where M : SaveModel
		{
			OnLoadSiblingFiles(Directory.GetFiles(result.SiblingDirectory).ToList(), model, result, done);
		}

		void OnLoadSiblingFiles<M>(List<string> remainingFiles, SaveModel model, M result, Action<SaveLoadRequest<M>> done)
			where M : SaveModel
		{
			if (remainingFiles.None())
			{
				done(SaveLoadRequest<M>.Success(model, result));
				return;
			}

			var nextFile = remainingFiles.First();
			remainingFiles.RemoveAt(0);

			Action onDone = () => OnLoadSiblingFiles(remainingFiles, model, result, done);

			switch (Path.GetExtension(nextFile))
			{
				case ".png":
					Read(nextFile, textureResult => OnReadSiblingTexture(textureResult, result, onDone));
					break;
				default:
					onDone();
					break;
			}
		}

		void OnReadSiblingTexture<M>(ReadWriteRequest textureResult, M result, Action done)
			where M : SaveModel
		{
			var textureName = Path.GetFileNameWithoutExtension(textureResult.Path);

			Action<string> onError = error =>
			{
				Debug.LogError(error);
				result.Textures.Add(textureName, null);
				done();
			};

			if (textureResult.Status != RequestStatus.Success)
			{
				onError("Unable to read bytes at \"" + textureResult.Path + "\", returning null.");
				return;
			}

			try
			{
				var target = new Texture2D(1, 1);
				result.PrepareTexture(textureName, target);
				target.LoadImage(textureResult.Bytes);
				result.Textures.Add(textureName, target);
				done();
			}
			catch (Exception e)
			{
				onError("Encountered the following exception while loading bytes into texture:\n" + e.Message);
			}
		}
		#endregion
	}
}