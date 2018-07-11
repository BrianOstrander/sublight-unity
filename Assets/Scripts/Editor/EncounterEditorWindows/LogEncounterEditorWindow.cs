using System.Linq;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class EncounterEditorWindow
	{
		void OnLog(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			switch (model.LogType)
			{
				case EncounterLogTypes.Text:
					OnTextLog(infoModel, model as TextEncounterLogModel);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized EncounterLogType: " + model.LogType, MessageType.Error);
					break;
			}
		}

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
			return deleted;
		}

		void OnTextLog(EncounterInfoModel infoModel, TextEncounterLogModel model)
		{
			GUILayout.Label("Header");
			model.Header.Value = GUILayout.TextArea(model.Header.Value);
			GUILayout.Label("Message");
			model.Message.Value = GUILayout.TextArea(model.Message.Value);
			OnLinearLog(infoModel, model);
		}

		void OnLinearLog(EncounterInfoModel infoModel, LinearEncounterLogModel model)
		{
			var options = infoModel.Logs.All.Value.Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value).Prepend("- Select Next Log -").ToArray();
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
				index = EditorGUILayout.Popup(index, options);
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