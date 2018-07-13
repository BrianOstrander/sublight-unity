using System.Linq;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class EncounterEditorWindow
	{
		bool OnLogBegin(EncounterInfoModel infoModel, EncounterLogModel model, ref string beginning, ref string ending)
		{
			var deleted = false;
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.BeginHorizontal();
			{
				var header = model.LogType + " Log Id:";
				GUILayout.Label(header, GUILayout.Width(header.Length * 5.5f));
				EditorGUILayout.SelectableLabel(model.LogId);
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