using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	[CustomPropertyDrawer(typeof(CurveStyleBlock))]
	public class CurveStyleBlockDrawer : PropertyDrawer
	{
		const float RowHeight = 16f;
		const float RowSpacing = 2f;

		const float CurveWidth = 80f;
		const float ColumnSpacing = 2f;
		const float CurveTotalWidth = CurveWidth + ColumnSpacing;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (RowHeight * 2f) + RowSpacing;
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
			var globalRect = new Rect(position.x, labelRect.yMax + RowSpacing, position.width - CurveTotalWidth, RowHeight);
			var curveRect = new Rect(globalRect.xMax + ColumnSpacing, globalRect.y, CurveWidth, RowHeight);

			EditorGUI.LabelField(labelRect, label);
			EditorGUI.indentLevel++;

			var styleProperty = property.FindPropertyRelative("globalStyle");

			EditorGUI.PropertyField(globalRect, styleProperty);
			var hasGlobalStyle = styleProperty.objectReferenceValue != null;

			var wasEnabled = GUI.enabled;
			GUI.enabled = !hasGlobalStyle;
			if (hasGlobalStyle) EditorGUI.CurveField(curveRect, (styleProperty.objectReferenceValue as CurveStyleObject).Curve);
			else EditorGUI.PropertyField(curveRect, property.FindPropertyRelative("curve"), GUIContent.none);
			GUI.enabled = wasEnabled;

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}