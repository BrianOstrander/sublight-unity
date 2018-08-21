using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutEncounter
	{
		/*
		public static bool LogPopup(
			string current,
			string label,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			EncounterLogModel nextModel,
			string currentAppend,
			Dictionary<string, string> preAppend,
			out string selection
		)
		{
			var nextId = nextModel == null ? string.Empty : nextModel.LogId.Value;
			var rawOptions = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value);
			var rawOptionNames = rawOptions.Select(l => l == nextId ? (l + currentAppend) : l);
			foreach (var kv in preAppend.Reverse())
			{
				rawOptions = rawOptions.Prepend(kv.Value);
				rawOptionNames = rawOptionNames.Prepend(kv.Key);
			}

			var options = rawOptions.ToArray();
			var optionNames = rawOptionNames.ToArray();

			var index = 0;
			for (var i = 0; i < options.Length; i++)
			{
				if (options[i] == current)
				{
					index = i;
					break;
				}
			}
			var startIndex = index;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(label, GUILayout.ExpandWidth(false)); // was 55
				index = EditorGUILayout.Popup(index, optionNames);
			}
			GUILayout.EndHorizontal();
			selection = options[index];

			return startIndex != index;
		}
		*/

		/*
		public static bool LogPopup(
			string current,
			string label,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			EncounterLogModel nextModel,
			string currentAppend,
			Dictionary<string, string> preAppend,
			out string selection
		)
		*/

		public static void LogPopup(
			string label,
			string current,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			Action<string> existingSelection,
			Action<EncounterLogTypes> newSelection,
			params string[] preAppend
		)
		{
			LogPopup(
				new GUIContent(label),
				current,
				infoModel,
				model,
				existingSelection,
				newSelection,
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
			params string[] preAppend
		)
		{
			if (infoModel == null) throw new ArgumentNullException("infoModel");
			if (model == null) throw new ArgumentNullException("model");
			if (existingSelection == null) throw new ArgumentNullException("existingSelection");
			if (newSelection == null) throw new ArgumentNullException("newSelection");

			content = content ?? GUIContent.none;

			const string currentAppend = " <- Next";
			var nextModel = infoModel.Logs.GetNextLogFirstOrDefault(model.Index.Value);

			var nextId = nextModel == null ? string.Empty : nextModel.LogId.Value;
			var rawOptions = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value);
			var rawOptionNames = rawOptions.Select(l => l == nextId ? (l + currentAppend) : l);

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
			for (var i = 0; i < options.Length; i++)
			{
				if (options[i] == current)
				{
					index = i;
					break;
				}
			}
			var startIndex = index;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(content, GUILayout.ExpandWidth(false)); // was 55
				index = EditorGUILayout.Popup(index, optionNames);
			}
			GUILayout.EndHorizontal();

			if (startIndex == index) return;

			var selectedId = options[index];
			var selectedLogType = logTypes[index];

			if (selectedLogType == EncounterLogTypes.Unknown) existingSelection(selectedId);
			else newSelection(selectedLogType);
		}
	}
}