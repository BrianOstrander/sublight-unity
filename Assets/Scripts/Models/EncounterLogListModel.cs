﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class EncounterLogListModel : Model
	{
		#region Assigned Values
		[JsonProperty] TextEncounterLogModel[] textLogs = new TextEncounterLogModel[0];
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<EncounterLogModel[]> All;
		#endregion

		public EncounterLogListModel()
		{
			// Derived Values
			All = new ListenerProperty<EncounterLogModel[]>(OnSetLogs, OnGetLogs);
		}

		#region Utility
		public T[] GetLogs<T>(Func<T, bool> predicate = null) where T : EncounterLogModel
		{
			if (predicate == null) return All.Value.OfType<T>().ToArray();
			return All.Value.OfType<T>().Where(predicate).ToArray();
		}

		public T GetLogFirstOrDefault<T>(string instanceId) where T : EncounterLogModel
		{
			return GetLogFirstOrDefault<T>(i => i.InstanceId == instanceId);
		}

		public T GetLogFirstOrDefault<T>(Func<T, bool> predicate = null) where T : EncounterLogModel
		{
			if (predicate == null) return All.Value.OfType<T>().FirstOrDefault();
			return All.Value.OfType<T>().FirstOrDefault(predicate);
		}
		#endregion

		#region Events
		void OnSetLogs(EncounterLogModel[] newLogs)
		{
			var textList = new List<TextEncounterLogModel>();

			foreach (var log in newLogs)
			{
				switch (log.LogType)
				{
					case EncounterLogTypes.Text:
						textList.Add(log as TextEncounterLogModel);
						break;
					default:
						Debug.LogError("Unrecognized EncounterLogType: " + log.LogType);
						break;
				}
			}

			textLogs = textList.ToArray();
		}

		EncounterLogModel[] OnGetLogs()
		{
			return textLogs.Cast<EncounterLogModel>().ToArray();
		}
		#endregion
	}
}