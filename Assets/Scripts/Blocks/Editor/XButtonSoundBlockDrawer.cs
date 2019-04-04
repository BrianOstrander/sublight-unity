using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	[CustomPropertyDrawer(typeof(XButtonSoundBlock))]
	public class XButtonSoundBlockDrawer : PropertyDrawer
	{
		const float RowHeight = 16f;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return RowHeight * 6f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var mixerRect = new Rect(position.x, position.y, position.width, RowHeight);
			var disabledRect = new Rect(position.x, mixerRect.yMax, position.width, RowHeight);
			var pressedRect = new Rect(position.x, disabledRect.yMax, position.width, RowHeight);
			var enteredRect = new Rect(position.x, pressedRect.yMax, position.width, RowHeight);
			var exitedRect = new Rect(position.x, enteredRect.yMax, position.width, RowHeight);
			var highlightedRect = new Rect(position.x, exitedRect.yMax, position.width, RowHeight);

			EditorGUI.PropertyField(mixerRect, property.FindPropertyRelative("Mixer"));
			EditorGUI.PropertyField(disabledRect, property.FindPropertyRelative("DisabledSound"));
			EditorGUI.PropertyField(pressedRect, property.FindPropertyRelative("PressedSound"));
			EditorGUI.PropertyField(enteredRect, property.FindPropertyRelative("EnteredSound"));
			EditorGUI.PropertyField(exitedRect, property.FindPropertyRelative("ExitedSound"));
			EditorGUI.PropertyField(highlightedRect, property.FindPropertyRelative("HighlightedSound"));

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}