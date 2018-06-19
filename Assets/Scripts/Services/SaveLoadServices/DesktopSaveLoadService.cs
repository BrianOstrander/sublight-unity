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
		static string ParentPath { get { return Path.Combine(Application.persistentDataPath, "saves"); } }

		protected override Dictionary<SaveTypes, int> MinimumSupportedSaves
		{
			get
			{
				return new Dictionary<SaveTypes, int>
				{
					{ SaveTypes.Game, 0 }
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
				default: throw new ArgumentOutOfRangeException("saveType", saveType + " is not handled.");
			}
		}

		protected override string GetUniquePath(SaveTypes saveType)
		{
			var path = Path.Combine(GetPath(saveType), Guid.NewGuid().ToString());

			// TODO: Check that path doesn't exist.

			return path;
		}

		protected override void OnLoad<M>(SaveModel model, Action<SaveLoadRequest<M>> done)
		{
			var result = Serialization.DeserializeJson<M>(File.ReadAllText(model.Path));
			if (result == null) done(SaveLoadRequest<M>.Failure(model, null, "Null result"));
			else done(SaveLoadRequest<M>.Success(model, result));
		}

		protected override void OnSave<M>(M model, Action<SaveLoadRequest<M>> done = null)
		{
			File.WriteAllText(model.Path, Serialization.Serialize(model));
			done(SaveLoadRequest<M>.Success(model, model));
		}
	}
}