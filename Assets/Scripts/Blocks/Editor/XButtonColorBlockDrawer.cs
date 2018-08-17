using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	[CustomPropertyDrawer(typeof(XButtonColorBlock))]
	public class XButtonColorBlockDrawer : PropertyDrawer
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
			var disabledColor = new Rect(position.x, position.y, position.width, RowHeight);
			var normalColor = new Rect(position.x, disabledColor.yMax, position.width, RowHeight);
			var highlightedColor = new Rect(position.x, normalColor.yMax, position.width, RowHeight);
			var pressedColor = new Rect(position.x, highlightedColor.yMax, position.width, RowHeight);

			EditorGUI.PropertyField(disabledColor, property.FindPropertyRelative("DisabledColor"));
			EditorGUI.PropertyField(normalColor, property.FindPropertyRelative("NormalColor"));
			EditorGUI.PropertyField(highlightedColor, property.FindPropertyRelative("HighlightedColor"));
			EditorGUI.PropertyField(pressedColor, property.FindPropertyRelative("PressedColor"));

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}