using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

using LunraGamesEditor;

using DefinedState = LunraGames.SubLight.DefinedKeyValueEditorWindow.DefinedState;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutDefinedKeyValue
	{
		const float KeyValueTargetWidth = 90f;

		public static void OnHandleKeyValueDefinition(
			string tooltipPrefix,
			string filterId,
			KeyValueTypes valueType,
			KeyValueTargets target,
			string key,
			Action<KeyValueTargets> setTarget,
			Action<string> setKey,
			Action define = null,
			float? keyValueKeyWidth = null
		)
		{
			if (setTarget == null) throw new ArgumentNullException("setTarget");
			if (setKey == null) throw new ArgumentNullException("setKey");

			// ----
			var tooltip = tooltipPrefix;
			var style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIconNotLinked;
			var notes = string.Empty;

			// -- begin link area?
			if (target != KeyValueTargets.Unknown && !string.IsNullOrEmpty(key))
			{
				DefinedKeys definitions = null;
				var continueCheck = true;

				switch (target)
				{
					case KeyValueTargets.Encounter: definitions = DefinedKeyInstances.Encounter; break;
					case KeyValueTargets.Game: definitions = DefinedKeyInstances.Game; break;
					case KeyValueTargets.Global: definitions = DefinedKeyInstances.Global; break;
					case KeyValueTargets.Preferences: definitions = DefinedKeyInstances.Preferences; break;
					case KeyValueTargets.CelestialSystem: definitions = DefinedKeyInstances.CelestialSystem; break;
					default:
						Debug.LogError("Unrecognized KeyValueTarget: " + target);
						continueCheck = false;
						break;
				}

				if (continueCheck)
				{
					IDefinedKey[] keys = null;

					switch (valueType)
					{
						case KeyValueTypes.Boolean: keys = definitions.Booleans; break;
						case KeyValueTypes.Integer: keys = definitions.Integers; break;
						case KeyValueTypes.String: keys = definitions.Strings; break;
						case KeyValueTypes.Float: keys = definitions.Floats; break;
						default:
							Debug.LogError("Unrecognized ValueFilterType: " + valueType);
							continueCheck = false;
							break;
					}

					if (keys == null)
					{
						Debug.LogError("Keys should not be null");
						continueCheck = false;
					}

					if (continueCheck)
					{
						var linkedKey = keys.FirstOrDefault(k => k.Key == key);

						if (linkedKey != null)
						{
							style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIconLinked;
							notes = linkedKey.Notes ?? string.Empty;
						}
					}
				}
			}
			// -- end link area?

			tooltip = tooltip + " - Click to link to a defined key value." + (string.IsNullOrEmpty(notes) ? string.Empty : "\n" + notes);

			EditorGUIExtensions.PauseChangeCheck();
			{
				if (GUILayout.Button(new GUIContent(string.Empty, tooltip), style, GUILayout.ExpandWidth(false)))
				{
					DefinedKeyValueEditorWindow.Show(
						new DefinedState
						{
							ValueType = valueType,
							Target = target,
							Key = key
						},
						result =>
						{
							setTarget(result.Target);
							setKey(result.Key);
							if (define != null) define();
						}
					);
				}
			}
			EditorGUIExtensions.UnPauseChangeCheck();
			// ----

			target = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
				"- Target -",
				target,
				Color.red,
				GUILayout.Width(KeyValueTargetWidth)
			);

			var keyIsInvalid = string.IsNullOrEmpty(key);
			EditorGUILayoutExtensions.PushColorValidation(Color.red, keyIsInvalid);
			{
				if (keyValueKeyWidth.HasValue) key = EditorGUILayout.TextField(key, GUILayout.Width(keyValueKeyWidth.Value));
				else key = EditorGUILayout.TextField(key);

			}
			EditorGUILayoutExtensions.PopColorValidation(keyIsInvalid);

			setTarget(target);
			setKey(key);
		}
	}
}