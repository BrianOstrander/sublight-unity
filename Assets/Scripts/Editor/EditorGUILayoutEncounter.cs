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
			public Action Select;
		}

		enum CallbackSources
		{
			Unknown = 0,
			AppendSelectOrBlank = 10,
			Select = 20
		}

		const float DropdownButtonOffsetY = -18f;

		static CallbackSources lastCallbackSource;
		static Rect lastMouseOverRect;
		static Action lastMouseOverCallback;

		/* Feels like I should be able to delete this... but keeping it here for the moment.
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
		*/

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
			if (infoModel == null) throw new ArgumentNullException("infoModel");

			switch (blankHandling)
			{
				case EncounterLogBlankHandling.SpecifiedByModel:
					if (model == null) throw new ArgumentNullException("model");
					break;
			}

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
				var nextLog = model == null ? null : infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);
				content = new GUIContent(
					GetReadableLogName(
						selectedLogId,
						selectedLogNull ? null : selectedLog.Name.Value,
						missingHandling != EncounterLogMissingHandling.None && selectedLogNull,
						nextLog != null && nextLog.LogId.Value == selectedLogId
					)
				);
			}

			var isInHorizontalLayout = hasPrefixContent || jump != null;

			if (isInHorizontalLayout) GUILayout.BeginHorizontal();

			var logIds = logs.Select(l => l.LogId.Value);
			var wasFound = logIds.Contains(selectedLogId);

			Color? dropdownColor = null;
			var issueTooltip = string.Empty;

			if (!string.IsNullOrEmpty(selectedLogId) && !wasFound)
			{
				switch (missingHandling)
				{
					case EncounterLogMissingHandling.Error:
						dropdownColor = Color.red;
						issueTooltip = "The specified LogId is missing: " + selectedLogId;
						break;
				}
			}
			else if (string.IsNullOrEmpty(selectedLogId))
			{
				const string BlankWarning = "Specifying no log may cause unpredictable behaviour.";
				const string BlankError = "A log must be specified.";

				switch (blankHandling)
				{
					case EncounterLogBlankHandling.Error:
						dropdownColor = Color.red;
						issueTooltip = BlankError;
						break;
					case EncounterLogBlankHandling.Warning:
						dropdownColor = Color.yellow;
						issueTooltip = BlankWarning;
						break;
					case EncounterLogBlankHandling.SpecifiedByModel:
						if (model.Ending.Value) break;
						if (model.RequiresFallbackLog)
						{
							dropdownColor = Color.red;
							issueTooltip = BlankError;
						}
						else
						{
							dropdownColor = Color.yellow;
							issueTooltip = BlankWarning;
						}
						break;
					case EncounterLogBlankHandling.FallsThrough:
						dropdownColor = Color.yellow;
						issueTooltip = "Specifying no log will fall through to the current log's \"Next Log\".";
						break;
				}
			}

			if (!string.IsNullOrEmpty(issueTooltip))
			{
				issueTooltip = "* " + issueTooltip + " *";

				if (string.IsNullOrEmpty(content.tooltip)) content.tooltip = issueTooltip;
				else content.tooltip = issueTooltip + "\n" + content.tooltip;

				if (hasPrefixContent)
				{
					if (string.IsNullOrEmpty(prefixContent.tooltip)) prefixContent.tooltip = issueTooltip;
					else prefixContent.tooltip = issueTooltip + "\n" + prefixContent.tooltip;
				}
			}

			if (hasPrefixContent) EditorGUILayout.PrefixLabel(prefixContent);

			if (dropdownColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(dropdownColor.Value.NewS(0.25f), dropdownColor.Value.NewS(0.65f));
			{
				if (EditorGUILayout.DropdownButton(content, FocusType.Keyboard, GUILayout.MaxWidth(200f)))
				{
					lastCallbackSource = CallbackSources.AppendSelectOrBlank;
					lastMouseOverCallback = () =>
					{
						var nextLog = model == null ? null : infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);

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
			}
			if (dropdownColor.HasValue) EditorGUILayoutExtensions.PopColorCombined();

			if (Event.current.type == EventType.Repaint && lastMouseOverCallback != null && lastCallbackSource == CallbackSources.AppendSelectOrBlank)
			{
				var lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					lastMouseOverRect = lastRect.NewY(lastRect.y + DropdownButtonOffsetY);
					var callback = lastMouseOverCallback;
					lastCallbackSource = CallbackSources.Unknown;
					lastMouseOverCallback = null;
					callback();
				}
			}

			if (jump != null)
			{
				if (GUILayout.Button(new GUIContent("Paste Id", "Pastes the contents of your clipboard if it contains a valid Log Id."), EditorStyles.miniButtonLeft, GUILayout.Width(48f)))
				{
					var logIdFromClipboard = EditorGUIUtility.systemCopyBuffer;
					if (string.IsNullOrEmpty(logIdFromClipboard))
					{
						EditorUtility.DisplayDialog("Invalid Log Id", "You specified a null or empty Log Id.", "Okay");
					}
					if (100 < logIdFromClipboard.Length)
					{
						EditorUtility.DisplayDialog("Log Id Too Long", "The Log Id you specified was over the 100 character limit, this cannot be a valid Log Id.", "Okay");
					}
					else if (model != null && logIdFromClipboard == model.LogId.Value)
					{
						EditorUtility.DisplayDialog("Recursive Log Id", "You specified a the Log Id of the current Log, this is not supported.", "Okay");
					}
					else if (!logIds.Contains(logIdFromClipboard))
					{
						EditorUtility.DisplayDialog("Log Id Not Found", "Cannot find any instances of the specified Log Id in this Encounter.\nLog Id: " + logIdFromClipboard, "Okay");
					}
					else existingSelection(logIdFromClipboard);
				}
				EditorGUILayoutExtensions.PushEnabled(!string.IsNullOrEmpty(selectedLogId));
				{
					EditorGUIExtensions.PauseChangeCheck();
					{
						if (GUILayout.Button(new GUIContent("Jump", "Focuses the selected log."), EditorStyles.miniButtonRight, GUILayout.Width(48f))) jump(selectedLogId);
					}
					EditorGUIExtensions.UnPauseChangeCheck();
				}
				EditorGUILayoutExtensions.PopEnabled();
			}

			if (isInHorizontalLayout) GUILayout.EndHorizontal();
		}

		public static void SelectLogPopup(
			string selectedLogId,
			GUIContent content,
			EncounterInfoModel infoModel,
			Action<string> selection,
			Action noneSelection,
			GUIStyle buttonStyle = null
		)
		{
			var pressed = false;
			if (buttonStyle == null) pressed = EditorGUILayout.DropdownButton(content, FocusType.Keyboard, GUILayout.ExpandWidth(false));
			else pressed = EditorGUILayout.DropdownButton(content, FocusType.Keyboard, buttonStyle, GUILayout.ExpandWidth(false));

			if (pressed)
			{
				lastCallbackSource = CallbackSources.Select;
				lastMouseOverCallback = () =>
				{
					ShowSelectMenu(
						selectedLogId,
						infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).ToArray(),
						selection,
						noneSelection
					);
				};
			}

			if (Event.current.type == EventType.Repaint && lastMouseOverCallback != null && lastCallbackSource == CallbackSources.Select)
			{
				var lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					lastMouseOverRect = lastRect.NewY(lastRect.y + DropdownButtonOffsetY);
					var callback = lastMouseOverCallback;
					lastCallbackSource = CallbackSources.Unknown;
					lastMouseOverCallback = null;
					callback();
				}
			}
		}

		static void ShowSelectMenu(
			string selectedLogId,
			EncounterLogModel[] options,
			Action<string> selection,
			Action noneSelection
		)
		{
			var entries = GetMenuEntries(
				options,
				selection,
				selectedLogId,
				null,
				selectedLogId
			);

			var menu = new GenericMenu();
			menu.allowDuplicateNames = true;

			if (string.IsNullOrEmpty(selectedLogId)) menu.AddDisabledItem(new GUIContent("Show All"), true);
			else menu.AddItem(new GUIContent("Show All"), false, () => noneSelection());

			menu.AddSeparator(string.Empty);

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
				current == null ? null : current.LogId.Value
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
						Name = GetReadableLogName(logId, log.Name.Value, isNext: isNext),
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

		public static string GetReadableLogName(
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