using UnityEditor;
using UnityEngine;

namespace LunraGames.SpaceFarm
{
	[CustomPropertyDrawer(typeof(XButtonStyleBlock))]
	public class XButtonStyleBlockDrawer : PropertyDrawer
	{
		const float RowHeight = 16f;
		const float ToggleWidth = 16f;
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return RowHeight * 7f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			var distScaleIntensityRect = new Rect(position.x, position.y, position.width, RowHeight);
			var clickScaleIntensityRect = new Rect(position.x, distScaleIntensityRect.yMax, position.width, RowHeight);
			var colorsRect = new Rect(position.x, clickScaleIntensityRect.yMax, position.width - ToggleWidth, RowHeight);
			var togglesRect = new Rect(colorsRect.xMax, clickScaleIntensityRect.yMax, ToggleWidth, RowHeight); 
			var clickDistanceRect = new Rect(position.x, colorsRect.yMax + (RowHeight * 3f), position.width, RowHeight); 

			EditorGUI.PropertyField(distScaleIntensityRect, property.FindPropertyRelative("DistanceScaleIntensity"));
			EditorGUI.PropertyField(clickScaleIntensityRect, property.FindPropertyRelative("ClickScaleDelta"));
			EditorGUI.PropertyField(togglesRect, property.FindPropertyRelative("Toggles"));
			EditorGUI.PropertyField(colorsRect, property.FindPropertyRelative("Colors"));
			EditorGUI.PropertyField(clickDistanceRect, property.FindPropertyRelative("ClickDistanceDelta"));

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}