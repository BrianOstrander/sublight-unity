using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public class DefinedKeyValueEditorWindow : EditorWindow
	{
		public class DefinedState
		{
			public KeyValueTypes ValueType;
			public ValueFilterTypes FilterType;
			public KeyValueTargets Target;
			public string Key;
		}

		class DefinedKeyEntry
		{
			public bool IsCurrent;
			public KeyValueTypes ValueType;
			public ValueFilterTypes FilterType;
			public KeyValueTargets Target;
			public string Key;
			public string Notes;
			public bool Expanded;
		}

		static Vector2 Size = new Vector2(430f, 400f);

		static Color CurrentContentColor = new Color(0.53f, 0.76f, 0.47f).NewS(0.4f).NewV(1f);

		DefinedState current;
		Action<DefinedState> selection;
		Action cancel;
		string description;
		DefinedKeyEntry[] entries;

		Vector2 verticalScroll;
		List<KeyValueTypes> hiddenValueTypes = new List<KeyValueTypes>();
		bool closeHandled;

		public static void Show(
			DefinedState current,
			Action<DefinedState> selection,
			string title = null,
			Action cancel = null
		)
		{
			if (selection == null) throw new ArgumentNullException("replaced");

			title = string.IsNullOrEmpty(title) ? "Select a Defined Key Value" : title;

			var window = GetWindow(typeof(DefinedKeyValueEditorWindow), true, title, true) as DefinedKeyValueEditorWindow;
			window.current = current;
			window.selection = selection;
			window.cancel = cancel;
			window.description = "Defined key values are updated or listened to by the event system.";

			window.position = EditorGUIExtensions.GetPositionOnScreen(Size);

			var valueTypeOrder = DefinedKeyInstances.SupportedFilterTypes;

			if (current != null && current.FilterType != ValueFilterTypes.Unknown)
			{
				valueTypeOrder = valueTypeOrder.ExceptOne(current.FilterType).Prepend(current.FilterType).ToArray();
				window.hiddenValueTypes = EnumExtensions.GetValues(current.ValueType).ToList();
			}

			var currentKey = current == null ? null : current.Key;
			var allDefinedKeys = DefinedKeyInstances.All;
			var entryList = new List<DefinedKeyEntry>();

			foreach (var definedKey in valueTypeOrder.SelectMany(v => allDefinedKeys.Where(k => k.FilterType == v).OrderBy(k => k.Key).OrderBy(k => k.Target)))
			{
				entryList.Add(
					new DefinedKeyEntry
					{
						IsCurrent = definedKey.Key == currentKey,
						ValueType = definedKey.ValueType,
						FilterType = definedKey.FilterType,
						Target = definedKey.Target,
						Key = definedKey.Key,
						Notes = definedKey.Notes
					}
				);
			}

			window.entries = entryList.ToArray();

			window.Show();
		}

		void OnGUI()
		{
			if (!string.IsNullOrEmpty(description)) GUILayout.Label(description);

			var normalColor = GUI.color;

			verticalScroll = new Vector2(0f, GUILayout.BeginScrollView(verticalScroll, false, true).y);
			{
				var lastValueType = KeyValueTypes.Unknown;
				foreach (var entry in entries)
				{
					var isEnabled = true;
					if (current != null)
					{
						isEnabled = current.ValueType == entry.ValueType;
						GUI.color = isEnabled ? normalColor : Color.gray;
					}

					var entryVisible = !hiddenValueTypes.Contains(entry.ValueType);

					if (lastValueType != entry.ValueType)
					{
						lastValueType = entry.ValueType;
						EditorGUILayoutExtensions.BeginHorizontal(EditorStyles.toolbar, Color.gray.NewV(0.8f));
						{
							if (entryVisible != EditorGUILayout.Foldout(entryVisible, lastValueType + "s", true))
							{
								entryVisible = !entryVisible;
								if (entryVisible) hiddenValueTypes.Remove(lastValueType);
								else hiddenValueTypes.Add(lastValueType);
							}
						}
						EditorGUILayoutExtensions.EndHorizontal();

						GUILayout.Space(2f);
					}

					if (!entryVisible) continue;

					EditorGUILayoutExtensions.PushIndent();
					{
						GUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (entry.IsCurrent) EditorGUILayoutExtensions.PushContentColor(CurrentContentColor);
							{
								entry.Expanded = EditorGUILayout.Foldout(entry.Expanded, new GUIContent(ObjectNames.NicifyVariableName(entry.Key.Replace('_', ' ')), entry.Key), true);
								GUILayout.Label(entry.Target.ToString(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
							}
							if (entry.IsCurrent) EditorGUILayoutExtensions.PopContentColor();

							GUILayout.Space(-10f);

							EditorGUILayoutExtensions.PushEnabled(isEnabled);
							{
								if (entry.IsCurrent != EditorGUILayout.Toggle(GUIContent.none, entry.IsCurrent, GUILayout.Width(32f)) && !entry.IsCurrent)
								{
									OnSelection(entry);
								}
							}
							EditorGUILayoutExtensions.PopEnabled();
						}
						GUILayout.EndHorizontal();

						if (entry.Expanded)
						{
							EditorGUILayoutExtensions.PushIndent();
							{
								GUILayout.Box(
									new GUIContent(string.IsNullOrEmpty(entry.Notes) ? "No description is available for this key." : entry.Notes),
									SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryDescription
								);
							}
							EditorGUILayoutExtensions.PopIndent();
						}
					}
					EditorGUILayoutExtensions.PopIndent();

					GUILayout.Space(2f);
				}
			}
			GUILayout.EndScrollView();

			GUI.color = normalColor;

			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton))
				{
					hiddenValueTypes = EnumExtensions.GetValues(KeyValueTypes.Unknown).ToList();
					foreach (var entry in entries) entry.Expanded = false;
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Cancel", EditorStyles.toolbarButton))
				{
					if (cancel != null) cancel();
					closeHandled = true;
					Close();
				}
			}
			GUILayout.EndHorizontal();
		}

		void OnSelection(DefinedKeyEntry entry)
		{
			selection(
				new DefinedState
				{
					ValueType = entry.ValueType,
					FilterType = entry.FilterType,
					Target = entry.Target,
					Key = entry.Key
				}
			);
			closeHandled = true;
			Close();
			GUIUtility.keyboardControl = 0;
		}

		void OnDestroy()
		{
			if (!closeHandled && cancel != null) cancel();
		}

		void OnLostFocus()
		{
			if (cancel != null) cancel();
			closeHandled = true;
			Close();
		}
	}
}