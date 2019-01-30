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
		FallsThrough = 50,
	}

	public enum EncounterLogMissingHandling
	{
		Unknown = 0,
		None = 10,
		Error = 20
	}

	public static class EditorGUILayoutEncounter
	{
		public static void LogPopup(
			GUIContent content,
			string current,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			EncounterLogBlankHandling blankHandling,
			EncounterLogMissingHandling missingHandling,
			params string[] preAppend
		)
		{
			var isBlankOrMissing = false;
			LogPopup(
				content,
				current,
				infoModel,
				model,
				existingSelection,
				newSelection,
				blankHandling,
				missingHandling,
				out isBlankOrMissing,
				preAppend
			);
		}

		public static void LogPopup(
			GUIContent content,
			string current,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			EncounterLogBlankHandling blankHandling,
			EncounterLogMissingHandling missingHandling,
			out bool isBlankOrMissing,
			params string[] preAppend
		)
		{
			if (infoModel == null) throw new ArgumentNullException("infoModel");
			if (model == null) throw new ArgumentNullException("model");
			if (existingSelection == null) throw new ArgumentNullException("existingSelection");
			if (newSelection == null) throw new ArgumentNullException("newSelection");

			content = content ?? GUIContent.none;

			const string currentAppend = " | Next";
			var nextModel = infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);

			var nextId = nextModel == null ? string.Empty : nextModel.LogId.Value;
			var rawOptions = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value);

			var optionNamesList = new List<string>();
			foreach (var logId in rawOptions)
			{
				var log = infoModel.Logs.GetLogFirstOrDefault(logId);
				if (log == null)
				{
					optionNamesList.Add("* Problem Finding Log "+logId+" *");
					continue;
				}

				var shortened = logId.Substring(0, Mathf.Min(8, logId.Length));

				if (log.HasName) shortened = log.Name.Value + " | " + shortened;

				if (logId == nextId) shortened += currentAppend;
				optionNamesList.Add(shortened);
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
				EditorGUILayout.PrefixLabel(content);
				index = EditorGUILayout.Popup(index, optionNames);
			}
			GUILayout.EndHorizontal();

			isBlankOrMissing = string.IsNullOrEmpty(current) || hasMissingId;

			if (missingHandling != EncounterLogMissingHandling.None && hasMissingId)
			{
				var missingMessage = string.Empty;
				var missingType = MessageType.None;

				switch (missingHandling)
				{
					case EncounterLogMissingHandling.Error:
						missingMessage = "The specified LogId is missing: " + current;
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
						if (model.Ending.Value) break;
						if (model.RequiresFallbackLog) 
						{
							blankMessage = BlankError;
							blankType = MessageType.Error;
						}
						else
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

			if (startIndex == index) return;

			var selectedId = options[index];
			var selectedLogType = logTypes[index];

			if (selectedLogType == EncounterLogTypes.Unknown) existingSelection(selectedId);
			else newSelection(selectedLogType);
		}
	}
}