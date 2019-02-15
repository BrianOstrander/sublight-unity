using System;

using UnityEngine;
using UnityEditor;

using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutDevPrefsToggle
	{
		public static void Field<P, T>(DevPrefsToggle<P, T> instance, Action<P> draw, bool? enabling = null)
			where P : DevPrefsKv<T>
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(instance.Enabled.Value);
				{
					draw(instance.Property);
				}
				EditorGUILayoutExtensions.PopEnabled();

				var wasIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					instance.Enabled.Value = EditorGUILayout.Toggle(instance.Enabled.Value, GUILayout.Width(14f));
					if (enabling.HasValue) instance.Enabled.Value = enabling.Value;
				}
				EditorGUI.indentLevel = wasIndent;
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}