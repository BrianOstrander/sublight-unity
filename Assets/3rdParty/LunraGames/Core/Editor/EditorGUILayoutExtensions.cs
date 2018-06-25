using System;
using System.Linq;
using UnityEditor;

namespace LunraGamesEditor
{
	public static class EditorGUILayoutExtensions
	{
		public static Enum HelpfulEnumPopup(string primaryReplacement, Enum value)
		{
			var name = Enum.GetName(value.GetType(), value);
			var originalNames = Enum.GetNames(value.GetType());
			var names = originalNames.ToArray();
			names[0] = primaryReplacement;
			var selection = 0;
			foreach (var currName in names)
			{
				if (currName == name) break;
				selection++;
			}
			selection = selection == names.Length ? 0 : selection;
			selection = EditorGUILayout.Popup(selection, names);

			return (Enum)Enum.Parse(value.GetType(),  originalNames[selection]);
		}
	}
}