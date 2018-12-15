using UnityEditor;
using UnityEngine;

using LunraGamesEditor;
using LunraGames;

namespace LunraGames.SubLight
{
	[CustomPropertyDrawer(typeof(FloatOverrideBlock))]
	public class FloatOverrideBlockDrawer : PropertyDrawer
	{
		const float RowHeight = 16f;
		const float RowSpacing = 2f;

		const float ToggleWidth = 16f;
		const float ColumnSpacing = 2f;
		const float ToggleTotalWidth = ToggleWidth + ColumnSpacing;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (RowHeight * 1f) + RowSpacing;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			var durationFloat = new Rect(position.x, position.y, position.width - ToggleTotalWidth, RowHeight);

			var durationToggle = new Rect(durationFloat.xMax + ColumnSpacing, durationFloat.y, ToggleWidth, RowHeight);

			DrawField(property, durationToggle, durationFloat, "Override", "Value");

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}

		void DrawField(SerializedProperty property, Rect overrideRect, Rect valueRect, string overrideName, string valueName)
		{
			var overrideProperty = property.FindPropertyRelative(overrideName);

			var wasEnabled = GUI.enabled;
			GUI.enabled = overrideProperty.boolValue;
			EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(valueName), new GUIContent(property.displayName, property.tooltip));
			GUI.enabled = wasEnabled;

			EditorGUI.PropertyField(overrideRect, overrideProperty, GUIContent.none);
		}
	}
}