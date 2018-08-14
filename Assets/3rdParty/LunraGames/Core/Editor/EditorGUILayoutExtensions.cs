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
		static Stack<Color> BackgroundColorStack = new Stack<Color>();
		static Stack<bool> EnabledStack = new Stack<bool>();
		static Stack<bool> TextAreaWordWrapStack = new Stack<bool>();

		/// <summary>
		/// Renames the first enum entry, useful for adding a "Select X" option.
		/// </summary>
		/// <returns>The enum popup.</returns>
		/// <param name="primaryReplacement">Primary replacement.</param>
		/// <param name="value">Value.</param>
		public static T HelpfulEnumPopup<T>(
			string primaryReplacement, 
			T value,
			T[] options = null,
			params GUILayoutOption[] guiOptions
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
			selection = EditorGUILayout.Popup(selection, names, guiOptions);

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

		public static void PushBackgroundColor(Color backgroundColor)
		{
			BackgroundColorStack.Push(GUI.backgroundColor);
			GUI.backgroundColor = backgroundColor;
		}

		public static void PopBackgroundColor()
		{
			if (BackgroundColorStack.Count == 0) return;
			GUI.backgroundColor = BackgroundColorStack.Pop();
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

		public static void PushTextAreaWordWrap(bool enabled)
		{
			TextAreaWordWrapStack.Push(EditorStyles.textArea.wordWrap);
			EditorStyles.textArea.wordWrap = enabled;
		}

		public static void PopTextAreaWordWrap()
		{
			if (TextAreaWordWrapStack.Count == 0) return;
			var popped = TextAreaWordWrapStack.Pop();
			EditorStyles.textArea.wordWrap = popped;
		}

		public static void PushIndent()
		{
			EditorGUI.indentLevel++;
		}

		public static void PopIndent()
		{
			EditorGUI.indentLevel--;
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
						values[i] = EditorGUILayout.TextField(values[i]);
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
			return EnumArray(
				new GUIContent(name),
				values,
				primaryReplacemnt,
				defaultValue,
				options
			);
		}

		public static T[] EnumArray<T>(
			GUIContent content, 
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
				GUILayout.Label(content);
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

		public static string TextDynamic(string value, int lengthLimit = 32)
		{
			return TextDynamic(GUIContent.none, value, lengthLimit);
		}

		public static string TextDynamic(string label, string value, int lengthLimit = 32)
		{
			return TextDynamic(new GUIContent(label), value, lengthLimit);
		}

		public static string TextDynamic(GUIContent content, string value, int lengthLimit = 32)
		{
			var nullContent = GUIContentExtensions.IsNullOrNone(content);
			lengthLimit = nullContent ? lengthLimit * 2 : lengthLimit;
			if (string.IsNullOrEmpty(value) || value.Length < lengthLimit)
			{
				// Is Field
				if (nullContent) value = EditorGUILayout.TextField(value);
				else value = EditorGUILayout.TextField(content, value);
			}
			else
			{
				// Is Area
				PushTextAreaWordWrap(true);
				{
					if (!nullContent) GUILayout.Label(content);
					value = EditorGUILayout.TextArea(value, EditorStyles.textArea);
				}
				PopTextAreaWordWrap();
			}
			return value;
		}

		public static bool ToggleButton(bool value, string trueText = "True", string falseText = "False", params GUILayoutOption[] options)
		{
			options = options.Prepend(GUILayout.Width(48f)).ToArray();
			if (GUILayout.Button(value ? trueText : falseText, options)) value = !value;
			return value;
		}

		public static bool ToggleButtonArray(bool value, string trueText = "True", string falseText = "False", float width = 48f)
		{
			GUILayout.BeginHorizontal();
			{
				PushEnabled(!value);
				if (GUILayout.Button(trueText, GUILayout.Width(width))) value = true;
				PopEnabled();

				PushEnabled(value);
				if (GUILayout.Button(falseText, GUILayout.Width(width))) value = false;
				PopEnabled();
			}
			GUILayout.EndHorizontal();
			return value;
		}

		public static void BeginVertical(GUIStyle style, Color? color, bool useColor = true)
		{
			BeginVertical(style, color.HasValue ? color.Value : GUI.color, useColor);
		}

		public static void BeginVertical(GUIStyle style, Color color, bool useColor = true)
		{
			if (useColor) PushColor(color);
			GUILayout.BeginVertical(style);
			if (useColor) PopColor();
		}

		public static void EndVertical()
		{
			GUILayout.EndVertical();
		}

		public static void BeginHorizontal(GUIStyle style, Color color, bool useColor = true)
		{
			if (useColor) PushColor(color);
			GUILayout.BeginHorizontal(style);
			if (useColor) PopColor();
		}

		public static void EndHorizontal()
		{
			GUILayout.EndHorizontal();
		}
	}
}