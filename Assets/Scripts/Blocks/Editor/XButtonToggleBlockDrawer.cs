using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	[CustomPropertyDrawer(typeof(XButtonToggleBlock))]
	public class XButtonToggleBlockDrawer : PropertyDrawer
	{
		const float RowHeight = 16f;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return RowHeight * 4f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			var disabledToggle = new Rect(position.x, position.y, position.width, RowHeight);
			var normalToggle = new Rect(position.x, disabledToggle.yMax, position.width, RowHeight);
			var highlightedToggle = new Rect(position.x, normalToggle.yMax, position.width, RowHeight);
			var pressedToggle = new Rect(position.x, highlightedToggle.yMax, position.width, RowHeight);

			EditorGUI.PropertyField(disabledToggle, property.FindPropertyRelative("ActiveOnDisabled"), GUIContent.none);
			EditorGUI.PropertyField(normalToggle, property.FindPropertyRelative("ActiveOnNormal"), GUIContent.none);
			EditorGUI.PropertyField(highlightedToggle, property.FindPropertyRelative("ActiveOnHighlighted"), GUIContent.none);
			EditorGUI.PropertyField(pressedToggle, property.FindPropertyRelative("ActiveOnPressed"), GUIContent.none);

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}