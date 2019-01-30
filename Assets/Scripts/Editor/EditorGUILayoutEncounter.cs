using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public enum EncounterLogBlankHandling
	{
		Unknown = 0,
		None = 10,
		Warning = 20,
		Error = 30,
		SpecifiedByModel = 40,
		SpecifiedByModelNoWarnings = 50,
		FallsThrough = 60,
	}

	public enum EncounterLogMissingHandling
	{
		Unknown = 0,
		None = 10,
		Error = 20
	}

	public static class EditorGUILayoutEncounter
	{
		class MenuEntry
		{
			public string LogId;
			public string Name;
			public bool IsSelected;
			public bool IsNext;
			public bool IsDisabled;
			public bool IsMissing;
			public Action Select;
		}

		/*
		public static string SelectLogPopup(
			GUIContent content,
			string selection,
			EncounterInfoModel infoModel
		)
		{
			var rawOptions = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Select(l => l.LogId.Value);

			var optionNamesList = new List<string>();
			foreach (var logId in rawOptions)
			{
				var log = infoModel.Logs.GetLogFirstOrDefault(logId);
				if (log == null)
				{
					optionNamesList.Add("* Problem Finding Log " + logId + " *");
					continue;
				}

				var shortened = logId.Substring(0, Mathf.Min(8, logId.Length));

				if (log.HasName) shortened = log.Name.Value + " | " + shortened;

				if (logId == nextId) shortened += currentAppend;
				optionNamesList.Add(shortened);
			}
			var rawOptionNames = optionNamesList.AsEnumerable();
		}
		*/

		public static void AppendOrSelectLogPopup(
			GUIContent prefixContent,
			GUIContent content,
			string selectedLogId,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			EncounterLogBlankHandling blankHandling,
			EncounterLogMissingHandling missingHandling,
			GUIContent noneSelectionContent = null,
			Action noneSelection = null
		)
		{
			var isBlankOrMissing = false;
			AppendOrSelectLogPopup(
				prefixContent,
				content,
				selectedLogId,
				infoModel,
				model,
				existingSelection,
				newSelection,
				blankHandling,
				missingHandling,
				out isBlankOrMissing
			);
		}

		public static void AppendOrSelectLogPopup(
			GUIContent prefixContent,
			GUIContent content,
			string selectedLogId,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			EncounterLogBlankHandling blankHandling,
			EncounterLogMissingHandling missingHandling,
			out bool isBlankOrMissing,
			GUIContent noneSelectionContent = null,
			Action noneSelection = null
		)
		{
			if (infoModel == null) throw new ArgumentNullException("infoModel");
			if (model == null) throw new ArgumentNullException("model");
			if (existingSelection == null) throw new ArgumentNullException("existingSelection");
			if (newSelection == null) throw new ArgumentNullException("newSelection");

			noneSelectionContent = noneSelectionContent ?? GUIContent.none;

			if (noneSelectionContent != GUIContent.none && noneSelection == null) throw new ArgumentNullException("noneSelection", "A noneSelectionContent was specified but noneSelection is null");

			prefixContent = prefixContent ?? GUIContent.none;

			/*
			var nextModel = infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);

			var nextId = nextModel == null ? string.Empty : nextModel.LogId.Value;
			var rawOptions = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value);

			var optionNamesList = new List<string>();
			foreach (var logId in rawOptions)
			{
				var log = infoModel.Logs.GetLogFirstOrDefault(logId);
				var logIsMissing = log == null;
				var logName = logIsMissing ? null : log.Name.Value;

				optionNamesList.Add(
					GetListName(
						logId,
						logName,
						logIsMissing,
						logId == nextId
					)
				);
			}
			var rawOptionNames = optionNamesList.AsEnumerable();

			var rawLogTypes = new List<EncounterLogTypes>().AsEnumerable();

			for (var i = 0; i < rawOptions.Count(); i++) rawLogTypes = rawLogTypes.Append(EncounterLogTypes.Unknown);

			rawOptions = rawOptions.Prepend(null);
			rawLogTypes = rawLogTypes.Prepend(EncounterLogTypes.Unknown);
			rawOptionNames = rawOptionNames.Prepend("--- Existing Logs ---");

			foreach (var logType in EnumExtensions.GetValues<EncounterLogTypes>().ExceptOne(EncounterLogTypes.Unknown).Reverse())
			{
				rawOptions = rawOptions.Prepend(null);
				rawLogTypes = rawLogTypes.Prepend(logType);
				rawOptionNames = rawOptionNames.Prepend(logType.ToString());
			}

			rawOptions = rawOptions.Prepend(null);
			rawLogTypes = rawLogTypes.Prepend(EncounterLogTypes.Unknown);
			rawOptionNames = rawOptionNames.Prepend("--- New Logs ---");

			foreach (var preAppendLabel in preAppend.Reverse())
			{
				rawOptions = rawOptions.Prepend(null);
				rawLogTypes = rawLogTypes.Prepend(EncounterLogTypes.Unknown);
				rawOptionNames = rawOptionNames.Prepend(preAppendLabel);
			}

			var options = rawOptions.ToArray();
			var optionNames = rawOptionNames.ToArray();
			var logTypes = rawLogTypes.ToArray();

			var index = 0;
			var wasFound = false;
			for (var i = 0; i < options.Length; i++)
			{
				if (options[i] == current)
				{
					index = i;
					wasFound = true;
					break;
				}
			}
			var startIndex = index;
			var hasMissingId = !string.IsNullOrEmpty(current) && !wasFound;

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel(prefixContent);
				index = EditorGUILayout.Popup(index, optionNames);
			}
			GUILayout.EndHorizontal();
			*/

			var logs = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value);

			if (EditorGUILayout.DropdownButton(content, FocusType.Keyboard))
			{
				var nextLog = infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);

				ShowSelectOrAppendMenu(
					model,
					logs.ToArray(),
					selectedLogId,
					nextLog == null ? null : nextLog.LogId.Value,
					existingSelection,
					newSelection,
					noneSelectionContent,
					noneSelection
				);
			}

			var logIds = logs.Select(l => l.LogId.Value);
			var wasFound = logIds.Contains(selectedLogId);
			var hasMissingId = !string.IsNullOrEmpty(selectedLogId) && !wasFound;

			isBlankOrMissing = string.IsNullOrEmpty(selectedLogId) || hasMissingId;

			if (missingHandling != EncounterLogMissingHandling.None && hasMissingId)
			{
				var missingMessage = string.Empty;
				var missingType = MessageType.None;

				switch (missingHandling)
				{
					case EncounterLogMissingHandling.Error:
						missingMessage = "The specified LogId is missing: " + selectedLogId;
						missingType = MessageType.Error;
						break;
				}

				if (missingType != MessageType.None) EditorGUILayout.HelpBox(missingMessage, missingType);
			}
			else if (blankHandling != EncounterLogBlankHandling.None && isBlankOrMissing)
			{
				var blankMessage = string.Empty;
				var blankType = MessageType.None;
				
				const string BlankWarning = "Specifying no log may cause unpredictable behaviour.";
				const string BlankError = "A log must be specified.";
				const string FallsThroughInfo = "Specifying no log will fall through to the current log's \"Next Log\".";

				switch (blankHandling)
				{
					case EncounterLogBlankHandling.Warning:
						blankMessage = BlankWarning;
						blankType = MessageType.Warning;
						break;
					case EncounterLogBlankHandling.Error:
						blankMessage = BlankError;
						blankType = MessageType.Error;
						break;
					case EncounterLogBlankHandling.SpecifiedByModel:
					case EncounterLogBlankHandling.SpecifiedByModelNoWarnings:
						if (model.Ending.Value) break;
						if (model.RequiresFallbackLog) 
						{
							blankMessage = BlankError;
							blankType = MessageType.Error;
						}
						else if (blankHandling != EncounterLogBlankHandling.SpecifiedByModelNoWarnings)
						{
							blankMessage = BlankWarning;
							blankType = MessageType.Warning;
						}
						break;
					case EncounterLogBlankHandling.FallsThrough:
						blankMessage = FallsThroughInfo;
						blankType = MessageType.Info;
						break;
				}

				if (blankType != MessageType.None) EditorGUILayout.HelpBox(blankMessage, blankType);
			}
		}

		static void ShowSelectOrAppendMenu(
			EncounterLogModel current,
			EncounterLogModel[] options,
			string selectedLogId,
			string nextLogId,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			GUIContent noneSelectionContent,
			Action noneSelection
		)
		{
			var entries = GetMenuEntries(
				options,
				existingSelection,
				selectedLogId,
				nextLogId,
				current.LogId.Value
			);

			var menu = new GenericMenu();
			menu.allowDuplicateNames = true;

			foreach (var logType in EnumExtensions.GetValues(EncounterLogTypes.Unknown).OrderBy(t => t.ToString()))
			{
				menu.AddItem(new GUIContent("Create/" + logType.ToString(), "does this appear???"), false, () => newSelection(logType));
			}

			menu.AddSeparator(string.Empty);

			if (noneSelectionContent != GUIContent.none)
			{
				menu.AddItem(noneSelectionContent, string.IsNullOrEmpty(selectedLogId), () => noneSelection());
				menu.AddSeparator(string.Empty);
			}

			foreach (var entry in entries)
			{
				if (entry.IsDisabled) menu.AddDisabledItem(new GUIContent(entry.Name), entry.IsSelected);
				else menu.AddItem(new GUIContent(entry.Name), entry.IsSelected, () => entry.Select());
			}

			//menu.DropDown(GUILayoutUtility.GetLastRect());
			menu.ShowAsContext();
		}

		static List<MenuEntry> GetMenuEntries(
			EncounterLogModel[] options,
			Action<string> select,
			string selectedLogId,
			string nextLogId = null,
			params string[] disabledLogIds
		)
		{
			var results = new List<MenuEntry>();

			foreach (var log in options)
			{
				var logId = log.LogId.Value; // Not sure if I have to do this anymore to avoid lambda bugs...
				var isLogIdNullOrEmpty = string.IsNullOrEmpty(logId);
				var isNext = !isLogIdNullOrEmpty && logId == nextLogId;

				results.Add(
					new MenuEntry
					{
						LogId = logId,
						Name = GetMenuEntryName(logId, log.Name.Value, isNext: isNext),
						IsSelected = !isLogIdNullOrEmpty && logId == selectedLogId,
						IsNext = isNext,
						IsDisabled = disabledLogIds.Contains(logId),
						Select = () => select(logId)
					}
				);
			}

			return results;
		}

		static string GetMenuEntryName(
			string logId,
			string name,
			bool missing = false,
			bool isNext = false
		)
		{
			if (missing) return "* Problem Finding Log " + (logId == null ? "< null >" : (string.IsNullOrEmpty(logId) ? "< empty >" : logId)) + " *";

			var result = logId.Substring(0, Mathf.Min(8, logId.Length));

			if (!string.IsNullOrEmpty(name)) result = name + " | " + result;

			if (isNext) result += " | Next";

			return result;
		}
	}
}