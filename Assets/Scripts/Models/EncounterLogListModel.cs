﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterLogListModel : Model
	{
		#region Assigned Values
		[JsonProperty] TextEncounterLogModel[] textLogs = new TextEncounterLogModel[0];
		[JsonProperty] KeyValueEncounterLogModel[] keyValueLogs = new KeyValueEncounterLogModel[0];
		[JsonProperty] InventoryEncounterLogModel[] inventoryLogs = new InventoryEncounterLogModel[0];
		[JsonProperty] SwitchEncounterLogModel[] switchLogs = new SwitchEncounterLogModel[0];
		[JsonProperty] ButtonEncounterLogModel[] buttonLogs = new ButtonEncounterLogModel[0];
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

		public EncounterLogModel GetLogFirstOrDefault(string logId)
		{
			return GetLogFirstOrDefault<EncounterLogModel>(logId);
		}

		public T GetLogFirstOrDefault<T>(string logId) where T : EncounterLogModel
		{
			return GetLogFirstOrDefault<T>(i => i.LogId == logId);
		}

		public T GetLogFirstOrDefault<T>(Func<T, bool> predicate = null) where T : EncounterLogModel
		{
			if (predicate == null) return All.Value.OfType<T>().FirstOrDefault();
			return All.Value.OfType<T>().FirstOrDefault(predicate);
		}

		[JsonIgnore]
		public EncounterLogModel Beginning
		{
			get
			{
				return GetLogFirstOrDefault<EncounterLogModel>(l => l.Beginning);
			}
		}
		#endregion

		#region Events
		void OnSetLogs(EncounterLogModel[] newLogs)
		{
			var textList = new List<TextEncounterLogModel>();
			var keyValueList = new List<KeyValueEncounterLogModel>();
			var inventoryList = new List<InventoryEncounterLogModel>();
			var switchList = new List<SwitchEncounterLogModel>();
			var buttonList = new List<ButtonEncounterLogModel>();

			foreach (var log in newLogs)
			{
				switch (log.LogType)
				{
					case EncounterLogTypes.Text:
						textList.Add(log as TextEncounterLogModel);
						break;
					case EncounterLogTypes.KeyValue:
						keyValueList.Add(log as KeyValueEncounterLogModel);
						break;
					case EncounterLogTypes.Inventory:
						inventoryList.Add(log as InventoryEncounterLogModel);
						break;
					case EncounterLogTypes.Switch:
						switchList.Add(log as SwitchEncounterLogModel);
						break;
					case EncounterLogTypes.Button:
						buttonList.Add(log as ButtonEncounterLogModel);
						break;
					default:
						Debug.LogError("Unrecognized EncounterLogType: " + log.LogType);
						break;
				}
			}

			textLogs = textList.ToArray();
			keyValueLogs = keyValueList.ToArray();
			inventoryLogs = inventoryList.ToArray();
			switchLogs = switchList.ToArray();
			buttonLogs = buttonList.ToArray();
		}

		EncounterLogModel[] OnGetLogs()
		{
			return textLogs.Cast<EncounterLogModel>().Concat(keyValueLogs)
													 .Concat(inventoryLogs)
													 .Concat(switchLogs)
													 .Concat(buttonLogs)
													 .ToArray();
		}
		#endregion
	}
}