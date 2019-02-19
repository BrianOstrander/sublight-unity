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

		/// <summary>
		/// When this is not null or empty, it means an address has been
		/// modified and the next time we see it the context should be marked
		/// changed. This stops changes being lost when exiting the defines
		/// window.
		/// </summary>
		static string changedAddressId;

		public static void Value<T>(
			Func<KeyValueAddress<T>> get,
			Action<KeyValueAddress<T>> set,
			KeyValueSources force = KeyValueSources.Unknown,
			Action define = null
		)
			where T : IConvertible
		{
			GUILayout.BeginHorizontal();
			{
				var inputSource = get().Source;

				switch (force)
				{
					case KeyValueSources.Unknown:
						inputSource = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
							GUIContent.none,
							"- Source -",
							get().Source,
							Color.red,
							guiOptions: GUILayout.Width(70f)
						);
						break;
					default:
						inputSource = force;
						break;
				}

				if (inputSource != get().Source)
				{
					var value = get();
					value.Source = inputSource;
					value.ForeignTarget = KeyValueTargets.Unknown;
					value.ForeignKey = null;
					value.LocalValue = default(T);
					set(value);
					GUI.changed = true;
				}

				switch (get().Source)
				{
					case KeyValueSources.LocalValue:
						OnValueLocal(get, set);
						break;
					case KeyValueSources.KeyValue:
						OnValueForeign(get, set, define);
						break;
					default:
						EditorGUILayoutExtensions.PushColor(Color.red.NewS(0.65f));
						{
							GUILayout.Label("Unrecognized Source: " + get().Source, GUILayout.ExpandWidth(false));
						}
						EditorGUILayoutExtensions.PopColor();
						break;
				}

				// It's possible we've changed a filter from the DefinedKeyValueEditorWindow, so we check for that here.
				if (!string.IsNullOrEmpty(changedAddressId) && get().AddressId == changedAddressId)
				{
					changedAddressId = null;
					GUI.changed = true;
				}
			}
			GUILayout.EndHorizontal();
		}

		static void OnValueLocal<T>(
			Func<KeyValueAddress<T>> get,
			Action<KeyValueAddress<T>> set
		)
			where T : IConvertible
		{
			var value = get();
			switch (value.KeyValueType)
			{
				case KeyValueTypes.Boolean:
					value.LocalValueRaw = EditorGUILayoutExtensions.ToggleButtonValue((bool)value.LocalValueRaw, style: EditorStyles.miniButton);
					break;
				case KeyValueTypes.Integer:
					value.LocalValueRaw = EditorGUILayout.IntField((int)value.LocalValueRaw);
					break;
				case KeyValueTypes.String:
					value.LocalValueRaw = EditorGUILayout.TextField((string)value.LocalValueRaw);
					break;
				case KeyValueTypes.Float:
					value.LocalValueRaw = EditorGUILayout.FloatField((float)value.LocalValueRaw);
					break;
				default:
					EditorGUILayoutExtensions.PushColor(Color.red.NewS(0.65f));
					{
						GUILayout.Label("Unrecognized Source: " + value.Source, GUILayout.ExpandWidth(false));
					}
					EditorGUILayoutExtensions.PopColor();
					break;
			}
			set(value);
		}

		static void OnValueForeign<T>(
			Func<KeyValueAddress<T>> get,
			Action<KeyValueAddress<T>> set,
			Action define = null
		)
			where T : IConvertible
		{
			var value = get();
			ValueForeign(
				ObjectNames.NicifyVariableName(value.KeyValueType.ToString()),
				value.KeyValueType,
				value.ForeignTarget,
				value.ForeignKey,
				target =>
				{
					var foreignValueTargetChanged = get();
					foreignValueTargetChanged.ForeignTarget = target;
					set(foreignValueTargetChanged);
				},
				key =>
				{
					var foreignValueKeyChanged = get();
					foreignValueKeyChanged.ForeignKey = key;
					set(foreignValueKeyChanged);
				},
				() => 
				{
					if (string.IsNullOrEmpty(value.AddressId)) Debug.LogError("AddressId is null or empty, field will not be notified of changes.");
					else changedAddressId = value.AddressId;
					if (define != null) define();
				}
			);
		}

		public static void ValueForeign(
			string tooltipPrefix,
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