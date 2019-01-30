using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using LunraGames.SubLight.Models;

using LunraGamesEditor;

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

	public enum EncounterLogBlankOptionHandling
	{
		Unknown = 0,
		Disabled = 10, // Never show a < blank > option, and show the default content when null.
		Selectable = 20, // Can be selected, and show < blank > instead of default content.
		NotSelectable = 30 // Show the option, but show the default content when null.
	}

	public static class EditorGUILayoutEncounter
	{
		class MenuEntry
		{
			public string LogId;
			public string Name;
			public bool IsNamed;
			public bool IsSelected;
			public bool IsNext;
			public bool IsDisabled;
			public bool IsMissing;
			public Action Select;
		}

		static Rect lastMouseOverRect;
		static Action lastMouseOverCallback;

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
			Action<string> jump
		)
		{
			AppendSelectOrBlankLogPopup(
				prefixContent,
				content,
				selectedLogId,
				infoModel,
				model,
				existingSelection,
				newSelection,
				blankHandling,
				missingHandling,
				EncounterLogBlankOptionHandling.Disabled,
				null,
				null,
				jump
			);
		}

		// TODO: Point dialogs to this as well
		public static void AppendSelectOrBlankLogPopup(
			GUIContent prefixContent,
			GUIContent content,
			string selectedLogId,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			EncounterLogBlankHandling blankHandling,
			EncounterLogMissingHandling missingHandling,
			EncounterLogBlankOptionHandling blankOptionHandling,
			GUIContent noneSelectionContent,
			Action noneSelection,
			Action<string> jump
		)
		{
			var isBlankOrMissing = false;
			AppendSelectOrBlankLogPopup(
				prefixContent,
				content,
				selectedLogId,
				infoModel,
				model,
				existingSelection,
				newSelection,
				blankHandling,
				missingHandling,
				out isBlankOrMissing,
				blankOptionHandling,
				noneSelectionContent,
				noneSelection,
				jump
			);
		}

		public static void AppendSelectOrBlankLogPopup(
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
			EncounterLogBlankOptionHandling blankOptionHandling,
			GUIContent noneSelectionContent,
			Action noneSelection,
			Action<string> jump
		)
		{
			if (infoModel == null) throw new ArgumentNullException("infoModel");
			if (model == null) throw new ArgumentNullException("model");
			if (existingSelection == null) throw new ArgumentNullException("existingSelection");
			if (newSelection == null) throw new ArgumentNullException("newSelection");

			noneSelectionContent = noneSelectionContent ?? GUIContent.none;

			switch (blankOptionHandling)
			{
				case EncounterLogBlankOptionHandling.Disabled: break;
				default:
					if (noneSelectionContent == GUIContent.none) throw new ArgumentException("Cannot have null or none content when blank logs are options", "noneSelectionContent");
					if (noneSelection == null) throw new ArgumentNullException("noneSelection");
					break;
			}

			prefixContent = prefixContent ?? GUIContent.none;
			var hasPrefixContent = prefixContent != GUIContent.none;

			switch (blankOptionHandling)
			{
				case EncounterLogBlankOptionHandling.Selectable:
					content = string.IsNullOrEmpty(selectedLogId) ? noneSelectionContent : content;
					break;
			}

			var logs = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value);

			if (!string.IsNullOrEmpty(selectedLogId))
			{
				var selectedLog = logs.FirstOrDefault(l => l.LogId.Value == selectedLogId);
				var selectedLogNull = selectedLog == null;
				var nextLog = infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);
				content = new GUIContent(
					GetMenuEntryName(
						selectedLogId,
						selectedLogNull ? null : selectedLog.Name.Value,
						selectedLogNull,
						nextLog != null && nextLog.LogId.Value == selectedLogId
					)
				);
			}

			var isInHorizontalLayout = hasPrefixContent || jump != null;

			if (isInHorizontalLayout) GUILayout.BeginHorizontal();

			if (hasPrefixContent) EditorGUILayout.PrefixLabel(prefixContent);

			if (EditorGUILayout.DropdownButton(content, FocusType.Keyboard, GUILayout.MaxWidth(200f)))
			{
				lastMouseOverCallback = () =>
				{
					var nextLog = infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);

					ShowSelectOrAppendMenu(
						model,
						logs.ToArray(),
						selectedLogId,
						nextLog == null ? null : nextLog.LogId.Value,
						existingSelection,
						newSelection,
						blankOptionHandling,
						noneSelectionContent,
						noneSelection
					);
				};
			}

			if (Event.current.type == EventType.Repaint && lastMouseOverCallback != null)
			{
				var lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					lastMouseOverRect = lastRect;
					var callback = lastMouseOverCallback;
					lastMouseOverCallback = null;
					callback();
				}
			}

			if (jump != null)
			{
				EditorGUILayoutExtensions.PushEnabled(!string.IsNullOrEmpty(selectedLogId));
				{
					if (GUILayout.Button(new GUIContent("Jump", "Focuses the selected log."), EditorStyles.miniButton, GUILayout.Width(48f))) jump(selectedLogId);
				}
				EditorGUILayoutExtensions.PopEnabled();
			}

			if (isInHorizontalLayout) GUILayout.EndHorizontal();

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
			EncounterLogBlankOptionHandling blankOptionHandling,
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
				menu.AddItem(new GUIContent("Create/" + logType.ToString()), false, () => newSelection(logType));
			}

			switch (blankOptionHandling)
			{
				case EncounterLogBlankOptionHandling.NotSelectable:
					menu.AddSeparator(string.Empty);
					menu.AddItem(noneSelectionContent, false, () => noneSelection());
					break;
				case EncounterLogBlankOptionHandling.Selectable:
					menu.AddSeparator(string.Empty);
					menu.AddItem(noneSelectionContent, string.IsNullOrEmpty(selectedLogId), () => noneSelection());
					break;
			}

			var namedEntries = entries.Where(e => e.IsNamed);
			var unnamedEntries = entries.Where(e => !e.IsNamed);

			if (namedEntries.Any())
			{
				menu.AddSeparator(string.Empty);
				foreach (var entry in namedEntries)
				{
					if (entry.IsDisabled) menu.AddDisabledItem(new GUIContent(entry.Name), entry.IsSelected);
					else menu.AddItem(new GUIContent(entry.Name), entry.IsSelected, () => entry.Select());
				}
			}

			if (unnamedEntries.Any())
			{
				menu.AddSeparator(string.Empty);
				foreach (var entry in unnamedEntries)
				{
					if (entry.IsDisabled) menu.AddDisabledItem(new GUIContent(entry.Name), entry.IsSelected);
					else menu.AddItem(new GUIContent(entry.Name), entry.IsSelected, () => entry.Select());
				}
			}

			menu.DropDown(lastMouseOverRect);
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
						IsNamed = !string.IsNullOrEmpty(log.Name.Value),
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
			var shortId = string.IsNullOrEmpty(logId) ? "< null or empty >" : logId.Substring(0, Mathf.Min(8, logId.Length));

			if (missing)
			{
				return shortId + " < Missing!";
			}

			var result = string.IsNullOrEmpty(name) ? shortId : name;

			return isNext ? result + " < Next" : result;
		}
	}
}