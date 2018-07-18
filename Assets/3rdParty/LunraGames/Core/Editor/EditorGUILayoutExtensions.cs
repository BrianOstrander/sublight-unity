using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using LunraGames;

namespace LunraGamesEditor
{
	public static class EditorGUILayoutExtensions
	{
		static Stack<Color> ColorStack = new Stack<Color>();
		static Stack<bool> EnabledStack = new Stack<bool>();

		/// <summary>
		/// Renames the first enum entry, useful for adding a "Select X" option.
		/// </summary>
		/// <returns>The enum popup.</returns>
		/// <param name="primaryReplacement">Primary replacement.</param>
		/// <param name="value">Value.</param>
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

		public static T HelpfulEnumPopup<T>(
			string primaryReplacement, 
			T value,
			T[] options = null
		) where T : struct, IConvertible
		{
			var name = Enum.GetName(value.GetType(), value);
			var originalNames = options == null ? Enum.GetNames(value.GetType()) : options.Select(o => Enum.GetName(value.GetType(), o)).ToArray();
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

			return (T)Enum.Parse(value.GetType(), originalNames[selection]);
		}

		public static void PushColor(Color color)
		{
			ColorStack.Push(GUI.color);
			GUI.color = color;
		}

		public static void PopColor()
		{
			if (ColorStack.Count == 0) return;
			GUI.color = ColorStack.Pop();
		}

		public static void PushEnabled(bool enabled)
		{
			EnabledStack.Push(GUI.enabled);
			GUI.enabled &= enabled;
		}

		public static void PopEnabled()
		{
			if (EnabledStack.Count == 0) return;
			GUI.enabled = EnabledStack.Pop();
		}

		public static bool XButton()
		{
			PushColor(Color.red);
			var clicked = GUILayout.Button("x", GUILayout.Width(20f));
			PopColor();
			return clicked;
		}

		public static string[] StringArray(string name, string[] values, string defaultValue = null)
		{
			if (values == null) throw new ArgumentNullException("values");

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name);
				if (GUILayout.Button("Preappend", GUILayout.Width(90f))) values = values.Prepend(defaultValue).ToArray();
				if (GUILayout.Button("Append", GUILayout.Width(90f))) values = values.Append(defaultValue).ToArray();
			}
			GUILayout.EndHorizontal();

			if (values.Length == 0) return values;

			int? deletedIndex = null;
			GUILayout.BeginVertical();
			{
				GUILayout.Space(4f);
				for (var i = 0; i < values.Length; i++)
				{
					GUILayout.BeginHorizontal();
					{
						GUILayout.Space(16f);
						GUILayout.Label("[ "+i+" ]", GUILayout.Width(32f));
						values[i] = GUILayout.TextField(values[i]);
						if (XButton()) deletedIndex = i;
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();
			if (deletedIndex.HasValue)
			{
				var list = values.ToList();
				list.RemoveAt(deletedIndex.Value);
				values = list.ToArray();
			}
			return values;
		}

		public static T[] EnumArray<T>(
			string name, 
			T[] values, 
			string primaryReplacemnt = null, 
			T defaultValue = default(T),
			T[] options = null
		) where T : struct, IConvertible
		{
			if (values == null) throw new ArgumentNullException("values");

			if (primaryReplacemnt == null) primaryReplacemnt = defaultValue.ToString();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name);
				if (GUILayout.Button("Preappend", GUILayout.Width(90f))) values = values.Prepend(defaultValue).ToArray();
				if (GUILayout.Button("Append", GUILayout.Width(90f))) values = values.Append(defaultValue).ToArray();
			}
			GUILayout.EndHorizontal();

			if (values.Length == 0) return values;

			int? deletedIndex = null;
			GUILayout.BeginVertical();
			{
				GUILayout.Space(4f);
				for (var i = 0; i < values.Length; i++)
				{
					GUILayout.BeginHorizontal();
					{
						GUILayout.Space(16f);
						GUILayout.Label("[ " + i + " ]", GUILayout.Width(32f));
						values[i] = HelpfulEnumPopup(primaryReplacemnt, values[i], options);
						PushColor(Color.red);
						if (GUILayout.Button("X", GUILayout.Width(18f))) deletedIndex = i;
						PopColor();
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();
			if (deletedIndex.HasValue)
			{
				var list = values.ToList();
				list.RemoveAt(deletedIndex.Value);
				values = list.ToArray();
			}
			return values;
		}
	}
}