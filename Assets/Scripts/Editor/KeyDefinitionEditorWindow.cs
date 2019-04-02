using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public class KeyDefinitionEditorWindow : EditorWindow
	{
		public class DefinedState
		{
			public KeyValueTypes ValueType;
			public KeyValueTargets Target;
			public string Key;
		}

		class DefinedKeyEntry
		{
			public bool IsCurrent;
			public KeyValueTypes ValueType;
			public KeyValueTargets Target;
			public string Key;
			public string Notes;
			public bool Expanded;
			public bool IsReadable;
			public bool IsWritable;
		}

		static Vector2 Size = new Vector2(430f, 400f);

		static Color CurrentContentColor = new Color(0.53f, 0.76f, 0.47f).NewS(0.4f).NewV(1f);

		DefinedState current;
		Action<DefinedState> selection;
		Action cancel;
		string description;
		DefinedKeyEntry[] entries;
		bool requiresWrite;
		bool requiresRead;
		EditorPrefsBool ignoreInvalidDefines;

		Vector2 verticalScroll;
		List<KeyValueTypes> hiddenValueTypes = new List<KeyValueTypes>();
		bool closeHandled;

		public static void Show(
			DefinedState current,
			Action<DefinedState> selection,
			string title = null,
			Action cancel = null,
			bool requiresWrite = false,
			bool requiresRead = true
		)
		{
			if (selection == null) throw new ArgumentNullException("replaced");

			title = string.IsNullOrEmpty(title) ? "Select a Key Definition" : title;

			var window = GetWindow(typeof(KeyDefinitionEditorWindow), true, title, true) as KeyDefinitionEditorWindow;
			window.current = current;
			window.selection = selection;
			window.cancel = cancel;
			window.description = "Key definitions are updated or listened to by the event system.";
			window.requiresWrite = requiresWrite;
			window.requiresRead = requiresRead;
			window.ignoreInvalidDefines = new EditorPrefsBool("KeyDefinitionEditorWindow_IgnoreInvalidDefines");

			window.position = EditorGUIExtensions.GetPositionOnScreen(Size);

			var valueTypeOrder = EnumExtensions.GetValues(KeyValueTypes.Unknown);

			if (current != null && current.ValueType != KeyValueTypes.Unknown)
			{
				valueTypeOrder = valueTypeOrder.ExceptOne(current.ValueType).Prepend(current.ValueType).ToArray();
				window.hiddenValueTypes = EnumExtensions.GetValues(current.ValueType).ToList();
			}

			var currentKey = current == null ? null : current.Key;
			var allDefinedKeys = KeyDefines.All;
			var entryList = new List<DefinedKeyEntry>();

			foreach (var definedKey in valueTypeOrder.SelectMany(v => allDefinedKeys.Where(k => k.ValueType == v).OrderBy(k => k.Key).OrderBy(k => k.Target)))
			{
				entryList.Add(
					new DefinedKeyEntry
					{
						IsCurrent = definedKey.Key == currentKey,
						ValueType = definedKey.ValueType,
						Target = definedKey.Target,
						Key = definedKey.Key,
						Notes = definedKey.Notes,
						IsReadable = definedKey.CanRead,
						IsWritable = definedKey.CanWrite
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

			var ignoredDefines = 0;

			verticalScroll = new Vector2(0f, GUILayout.BeginScrollView(verticalScroll, false, true).y);
			{
				var lastValueType = KeyValueTypes.Unknown;

				foreach (var entry in entries)
				{
					GUI.color = normalColor;

					var isEnabled = true;

					if (requiresRead && !entry.IsReadable) isEnabled = false;
					else if (requiresWrite && !entry.IsWritable) isEnabled = false;

					if (current != null)
					{
						switch (current.ValueType)
						{
							case KeyValueTypes.Integer:
							case KeyValueTypes.Enumeration:
								isEnabled &= entry.ValueType == KeyValueTypes.Integer || entry.ValueType == KeyValueTypes.Enumeration;
								break;
							default:
								isEnabled &= current.ValueType == entry.ValueType;
								break;
						}
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

					GUI.color = isEnabled ? normalColor : Color.gray;


					if (ignoreInvalidDefines.Value && !isEnabled)
					{
						ignoredDefines++;
						continue;
					}
					if (!entryVisible) continue;

					EditorGUILayoutExtensions.PushIndent();
					{
						GUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (entry.IsCurrent) EditorGUILayoutExtensions.PushContentColor(CurrentContentColor);
							{
								EditorGUILayoutExtensions.PushEnabled(isEnabled);
								{
									if (entry.IsCurrent != EditorGUILayout.Toggle(GUIContent.none, entry.IsCurrent, GUILayout.Width(32f)) && !entry.IsCurrent)
									{
										OnSelection(entry);
									}
								}
								EditorGUILayoutExtensions.PopEnabled();

								GUILayout.Space(-20f);

								entry.Expanded = EditorGUILayout.Foldout(entry.Expanded, new GUIContent(ObjectNames.NicifyVariableName(entry.Key.Replace('_', ' ')), entry.Key), true);


								var accessText = string.Empty;
								var accessTooltip = string.Empty;
								var accessError = false;

								if (entry.IsReadable && entry.IsWritable) accessText = "Read & Write";
								else if (entry.IsReadable) accessText = "Read";
								else if (entry.IsWritable) accessText = "Write";
								else 
								{
									accessText = "Invalid Access";
									accessTooltip = "This entry is key definition is neither readable nor writable.";
									accessError = true;
								}

								EditorGUILayoutExtensions.PushContentColor(accessError ? Color.red.NewS(0.65f) : Color.gray);
								{
									GUILayout.Label(new GUIContent(accessText, accessTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
								}
								EditorGUILayoutExtensions.PopContentColor();

								GUILayout.Label(entry.Target.ToString(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
							}
							if (entry.IsCurrent) EditorGUILayoutExtensions.PopContentColor();
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
				if (GUILayout.Button(ignoreInvalidDefines.Value ? "Show Invalid Keys" : "Hide Invalid Keys", EditorStyles.toolbarButton, GUILayout.Width(100f)))
				{
					ignoreInvalidDefines.Value = !ignoreInvalidDefines.Value;
				}

				if (0 < ignoredDefines)
				{
					EditorGUILayoutExtensions.PushColor(Color.gray);
					{
						GUILayout.Label(ignoredDefines + " Hidden", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
					}
					EditorGUILayoutExtensions.PopColor();
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