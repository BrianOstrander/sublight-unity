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
		[JsonProperty] KeyValueEncounterLogModel[] keyValueLogs = new KeyValueEncounterLogModel[0];
		[JsonProperty] SwitchEncounterLogModel[] switchLogs = new SwitchEncounterLogModel[0];
		[JsonProperty] ButtonEncounterLogModel[] buttonLogs = new ButtonEncounterLogModel[0];
		[JsonProperty] EncyclopediaEncounterLogModel[] encyclopediaLogs = new EncyclopediaEncounterLogModel[0];
		[JsonProperty] EncounterEventEncounterLogModel[] eventLogs = new EncounterEventEncounterLogModel[0];
		[JsonProperty] DialogEncounterLogModel[] dialogLogs = new DialogEncounterLogModel[0];
		[JsonProperty] BustEncounterLogModel[] bustLogs = new BustEncounterLogModel[0];
		[JsonProperty] ConversationEncounterLogModel[] conversationLogs = new ConversationEncounterLogModel[0];
		[JsonProperty] ModuleTraitEncounterLogModel[] moduleTraitLogs = new ModuleTraitEncounterLogModel[0];
		[JsonProperty] ModuleSwapEncounterLogModel[] moduleSwapLogs = new ModuleSwapEncounterLogModel[0];
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

		/// <summary>
		/// Gets the next log after the provided index, or null if there are
		/// none.
		/// </summary>
		/// <returns>The next log first or default.</returns>
		/// <param name="index">Index.</param>
		/// <param name="predicate">Predicate.</param>
		public EncounterLogModel GetNextLogFirstOrDefault(int index, Func<EncounterLogModel, bool> predicate = null)
		{
			return GetNextLogFirstOrDefault<EncounterLogModel>(index, predicate);
		}

		/// <summary>
		/// Gets the next log after the provided index, or null if there are
		/// none.
		/// </summary>
		/// <returns>The next log first or default.</returns>
		/// <param name="index">Index.</param>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetNextLogFirstOrDefault<T>(int index, Func<T, bool> predicate = null) where T : EncounterLogModel
		{
			return GetLogs(predicate).Where(l => index < l.Index.Value).OrderBy(l => l.Index.Value).FirstOrDefault();
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
			var keyValueList = new List<KeyValueEncounterLogModel>();
			var switchList = new List<SwitchEncounterLogModel>();
			var buttonList = new List<ButtonEncounterLogModel>();
			var encyclopediaList = new List<EncyclopediaEncounterLogModel>();
			var eventList = new List<EncounterEventEncounterLogModel>();
			var dialogList = new List<DialogEncounterLogModel>();
			var bustList = new List<BustEncounterLogModel>();
			var conversationList = new List<ConversationEncounterLogModel>();
			var moduleTraitList = new List<ModuleTraitEncounterLogModel>();
			var moduleSwapList = new List<ModuleSwapEncounterLogModel>();

			foreach (var log in newLogs)
			{
				switch (log.LogType)
				{
					case EncounterLogTypes.KeyValue:
						keyValueList.Add(log as KeyValueEncounterLogModel);
						break;
					case EncounterLogTypes.Switch:
						switchList.Add(log as SwitchEncounterLogModel);
						break;
					case EncounterLogTypes.Button:
						buttonList.Add(log as ButtonEncounterLogModel);
						break;
					case EncounterLogTypes.Encyclopedia:
						encyclopediaList.Add(log as EncyclopediaEncounterLogModel);
						break;
					case EncounterLogTypes.Event:
						eventList.Add(log as EncounterEventEncounterLogModel);
						break;
					case EncounterLogTypes.Dialog:
						dialogList.Add(log as DialogEncounterLogModel);
						break;
					case EncounterLogTypes.Bust:
						bustList.Add(log as BustEncounterLogModel);
						break;
					case EncounterLogTypes.Conversation:
						conversationList.Add(log as ConversationEncounterLogModel);
						break;
					case EncounterLogTypes.ModuleTrait:
						moduleTraitList.Add(log as ModuleTraitEncounterLogModel);
						break;
					case EncounterLogTypes.ModuleSwap:
						moduleSwapList.Add(log as ModuleSwapEncounterLogModel);
						break;
					default:
						Debug.LogError("Unrecognized EncounterLogType: " + log.LogType);
						break;
				}
			}

			keyValueLogs = keyValueList.ToArray();
			switchLogs = switchList.ToArray();
			buttonLogs = buttonList.ToArray();
			encyclopediaLogs = encyclopediaList.ToArray();
			eventLogs = eventList.ToArray();
			dialogLogs = dialogList.ToArray();
			bustLogs = bustList.ToArray();
			conversationLogs = conversationList.ToArray();
			moduleTraitLogs = moduleTraitList.ToArray();
			moduleSwapLogs = moduleSwapList.ToArray();
		}

		EncounterLogModel[] OnGetLogs()
		{
			return keyValueLogs.Cast<EncounterLogModel>().Concat(switchLogs)
														 .Concat(buttonLogs)
														 .Concat(encyclopediaLogs)
					           							 .Concat(eventLogs)
					           							 .Concat(dialogLogs)
					           							 .Concat(bustLogs)
					           							 .Concat(conversationLogs)
														 .Concat(moduleTraitLogs)
														 .Concat(moduleSwapLogs)
														 .ToArray();
		}
		#endregion
	}
}