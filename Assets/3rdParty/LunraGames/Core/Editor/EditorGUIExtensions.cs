using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

namespace LunraGamesEditor
{
	public static class EditorGUIExtensions
	{
		static Stack<bool> GuiChangedStack = new Stack<bool>();

		public static void PushGuiChanged()
		{
			var current = false;
			if (0 < GuiChangedStack.Count) current = EditorGUI.EndChangeCheck();
			GuiChangedStack.Push(current);
			GUI.changed = false;
			EditorGUI.BeginChangeCheck();
		}

		public static bool PopGuiChanged(ref bool changed)
		{
			changed |= EditorGUI.EndChangeCheck();
			if (0 < GuiChangedStack.Count) GUI.changed = GuiChangedStack.Pop();
			return changed;
		}

		public static bool PopGuiChanged()
		{
			var result = EditorGUI.EndChangeCheck();
			if (0 < GuiChangedStack.Count) GUI.changed = GuiChangedStack.Pop();
			return result;
		}
	}
}