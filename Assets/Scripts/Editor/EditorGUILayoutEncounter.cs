using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutEncounter
	{
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
	}
}