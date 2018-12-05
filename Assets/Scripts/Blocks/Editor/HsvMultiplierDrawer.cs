using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	[CustomPropertyDrawer(typeof(HsvMultiplier))]
	public class HsvMultiplierDrawer : PropertyDrawer
	{
		const float RowHeight = 16f;
		const float RowSpacing = 2f;

		const float ToggleWidth = 28f;
		const float ColumnSpacing = 2f;
		const float ToggleTotalWidth = ToggleWidth + ColumnSpacing;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (RowHeight * 4f) + (RowSpacing * 3f);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			label = EditorGUI.BeginProperty(position, label, property);

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			var labelRect = new Rect(position.x, position.y, position.width, RowHeight);
			var hRect = new Rect(position.x, labelRect.yMax + RowSpacing, position.width - ToggleTotalWidth, RowHeight);
			var sRect = new Rect(position.x, hRect.yMax + RowSpacing, hRect.width, RowHeight);
			var vRect = new Rect(position.x, sRect.yMax + RowSpacing, hRect.width, RowHeight);

			var hToggleRect = new Rect(hRect.xMax + ColumnSpacing - 12f, hRect.y, ToggleWidth, RowHeight);
			var sToggleRect = new Rect(sRect.xMax + ColumnSpacing - 12f, sRect.y, ToggleWidth, RowHeight);
			var vToggleRect = new Rect(vRect.xMax + ColumnSpacing - 12f, vRect.y, ToggleWidth, RowHeight);

			EditorGUI.LabelField(labelRect, label);
			EditorGUI.indentLevel++;

			DrawMultiplier(hRect, hToggleRect, property, "H");
			DrawMultiplier(sRect, sToggleRect, property, "S");
			DrawMultiplier(vRect, vToggleRect, property, "V");

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;


			EditorGUI.EndProperty();
		}

		void DrawMultiplier(Rect valueRect, Rect toggleRect, SerializedProperty property, string namePrefix)
		{
			var toggleProperty = property.FindPropertyRelative("Enabled" + namePrefix);
			EditorGUI.PropertyField(toggleRect, toggleProperty, GUIContent.none);

			var wasEnabled = GUI.enabled;
			GUI.enabled = toggleProperty.boolValue;
			EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(namePrefix));
			GUI.enabled = wasEnabled;
		}
	}
}