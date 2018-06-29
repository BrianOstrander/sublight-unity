using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class DesktopSaveLoadService : SaveLoadService
	{
		const string Extension = ".json";
		static string ParentPath { get { return Path.Combine(Application.persistentDataPath, "saves"); } }

		protected override Dictionary<SaveTypes, int> MinimumSupportedSaves
		{
			get
			{
				return new Dictionary<SaveTypes, int>
				{
					{ SaveTypes.Game, -1 },
					{ SaveTypes.Preferences, -1 }
				};
			}
		}


		public override void Initialize(Action<RequestStatus> done)
		{
			try
			{
				foreach (var curr in Enum.GetValues(typeof(SaveTypes)).Cast<SaveTypes>())
				{
					if (curr == SaveTypes.Unknown) continue;
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
			File.WriteAllText(model.Path, Serialization.Serialize(model));
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
			done(SaveLoadArrayRequest<SaveModel>.Success(array, array));
		}

	}
}