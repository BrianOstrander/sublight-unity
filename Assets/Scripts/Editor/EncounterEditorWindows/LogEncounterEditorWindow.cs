using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class EncounterEditorWindow
	{
		bool OnLogBegin(int count, EncounterInfoModel infoModel, EncounterLogModel model, ref string beginning, ref string ending)
		{
			var deleted = false;
			var isAlternate = count % 2 == 0;
			if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.gray);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (isAlternate) EditorGUILayoutExtensions.PopColor();
			GUILayout.BeginHorizontal();
			{
				var header = "#"+(count + 1)+" | "+model.LogType + ".LogId:";
				GUILayout.Label(header, EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
				EditorGUILayout.SelectableLabel(model.LogId, EditorStyles.boldLabel);
				if (EditorGUILayout.ToggleLeft("Beginning", model.Beginning.Value, GUILayout.Width(70f)) && !model.Beginning.Value)
				{
					beginning = model.LogId;
				}
				if (EditorGUILayout.ToggleLeft("Ending", model.Ending.Value, GUILayout.Width(60f)) && !model.Ending.Value)
				{
					ending = model.LogId;
				}
				deleted = EditorGUILayoutExtensions.XButton();
			}
			GUILayout.EndHorizontal();

			OnLogDuration(infoModel, model);

			return deleted;
		}

		void OnLog(EncounterInfoModel infoModel, EncounterLogModel model, EncounterLogModel nextModel)
		{
			switch (model.LogType)
			{
				case EncounterLogTypes.Text:
					OnTextLog(infoModel, model as TextEncounterLogModel, nextModel);
					break;
				case EncounterLogTypes.KeyValue:
					OnKeyValueLog(infoModel, model as KeyValueEncounterLogModel, nextModel);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized EncounterLogType: " + model.LogType, MessageType.Error);
					break;
			}
		}

		void OnLogDuration(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			EditorGUILayoutExtensions.PushEnabled(model.EditableDuration);

			if (model.EditableDuration)
			{
				model.Duration.Value = EditorGUILayout.FloatField("Duration", model.Duration);
			}
			else EditorGUILayout.FloatField("Duration", model.TotalDuration);

			EditorGUILayoutExtensions.PopEnabled();
		}

		void OnTextLog(EncounterInfoModel infoModel, TextEncounterLogModel model, EncounterLogModel nextModel)
		{
			GUILayout.Label("Header");
			model.Header.Value = GUILayout.TextArea(model.Header.Value);
			GUILayout.Label("Message");
			model.Message.Value = GUILayout.TextArea(model.Message.Value);
			OnLinearLog(infoModel, model, nextModel);
		}

		void OnKeyValueLog(EncounterInfoModel infoModel, KeyValueEncounterLogModel model, EncounterLogModel nextModel)
		{
			var targets = Enum.GetValues(typeof(KeyValueTargets)).Cast<KeyValueTargets>().ToList();
			var kvTypes = Enum.GetValues(typeof(KeyValueEncounterLogTypes)).Cast<KeyValueEncounterLogTypes>().ToList();

			var labels = new List<string>(new string[] { "- Select Key Value -" });
			var onSelections = new Dictionary<int, Action>();
			var index = 1;

			foreach (var target in targets)
			{
				if (target == KeyValueTargets.Unknown) continue;
				labels.Add("--- " + target + " ---");
				index++;

				foreach (var kvType in kvTypes)
				{
					if (kvType == KeyValueEncounterLogTypes.Unknown) continue;
					labels.Add(target+"."+kvType);
					onSelections.Add(
						index,
						() => OnKeyValueLogSpawn(infoModel, model, target, kvType)
					);
					index++;
				}
			}

			var selection = 0;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Append New Key Value: ", GUILayout.Width(128f));
				selection = EditorGUILayout.Popup(selection, labels.ToArray());
			}
			GUILayout.EndHorizontal();
			Action onSelection;
			if (onSelections.TryGetValue(selection, out onSelection)) onSelection();

			var deleted = string.Empty;
			var isAlternate = false;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					foreach (var kv in model.KeyValues.Value)
					{
						isAlternate = !isAlternate;

						if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.grey.NewV(0.5f));
						GUILayout.BeginVertical(EditorStyles.helpBox);
						if (isAlternate) EditorGUILayoutExtensions.PopColor();

						if (OnKeyValueLogHeader(infoModel, model, kv)) deleted = kv.KeyValueId.Value;
						switch (kv.KeyValueType)
						{
							case KeyValueEncounterLogTypes.SetString:
								OnKeyValueLogSetString(infoModel, model, kv as SetStringEntryEncounterLogModel);
								break;
							default:
								Debug.LogError("Unrecognized KeyValueType: " + kv.KeyValueType);
								break;
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.KeyValues.Value = model.KeyValues.Value.Where(kv => kv.KeyValueId != deleted).ToArray();
			}

			OnLinearLog(infoModel, model, nextModel);
		}

		bool OnKeyValueLogHeader(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueEntryEncounterLogModel keyValue
		)
		{
			var deleted = false;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(keyValue.KeyValueType + ":", GUILayout.ExpandWidth(false));
				keyValue.Target.Value = EditorGUILayoutExtensions.HelpfulEnumPopup("- Select Target -", keyValue.Target.Value);
				deleted = EditorGUILayoutExtensions.XButton();
			}
			GUILayout.EndHorizontal();
			return deleted;
		}

		void OnKeyValueLogSetString(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			SetStringEntryEncounterLogModel keyValue
		)
		{
			keyValue.Key.Value = EditorGUILayout.TextField("Key", keyValue.Key.Value);

			var isField = string.IsNullOrEmpty(keyValue.Value.Value) || keyValue.Value.Value.Length < 32;
			if (isField)
			{
				EditorStyles.textField.wordWrap = true;
				keyValue.Value.Value = EditorGUILayout.TextField("Value", keyValue.Value.Value);
			}
			else
			{
				GUILayout.Label("Value");
				EditorStyles.textArea.wordWrap = true;
				keyValue.Value.Value = EditorGUILayout.TextArea(keyValue.Value.Value);
			}
		}

		void OnKeyValueLogSpawn(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueTargets target,
			KeyValueEncounterLogTypes kvType
		)
		{
			var guid = Guid.NewGuid().ToString();
			switch(kvType)
			{
				case KeyValueEncounterLogTypes.SetString:
					var setString = new SetStringEntryEncounterLogModel();
					setString.KeyValueId.Value = guid;
					setString.Target.Value = target;
					model.KeyValues.Value = model.KeyValues.Value.Append(setString).ToArray();
					break;
				default:
					Debug.LogError("Unrecognized KeyValueEncoutnerLogType: " + kvType);
					break;
			}
		}

		void OnLinearLog(EncounterInfoModel infoModel, LinearEncounterLogModel model, EncounterLogModel nextModel)
		{
			var nextId = nextModel == null ? string.Empty : nextModel.LogId.Value;
			var options = infoModel.Logs.All.Value.Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value).Prepend("- Select Next Log -").ToArray();
			var optionNames = options.Select(l => l == nextId ? (l + " <- Next") : l).ToArray();
			var index = 0;
			if (!string.IsNullOrEmpty(model.NextLogId.Value))
			{
				for (var i = 0; i < options.Length; i++)
				{
					if (options[i] == model.NextLogId.Value)
					{
						index = i;
						break;
					}
				}
			}
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Next Log: ", GUILayout.Width(55f));
				index = EditorGUILayout.Popup(index, optionNames);
			}
			GUILayout.EndHorizontal();
			if (index == 0) model.NextLogId.Value = null;
			else model.NextLogId.Value = options[index];
		}

		void OnLogEnd(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			GUILayout.EndVertical();
		}
	}
}