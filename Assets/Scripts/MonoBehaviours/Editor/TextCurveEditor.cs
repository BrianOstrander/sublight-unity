using UnityEditor;

namespace LunraGames.SubLight.Views
{
	[CustomEditor(typeof(TextCurve), true)]
	[CanEditMultipleObjects]
	public class TextCurveEditor : Editor
	{
		SerializedProperty textProperty;

		void OnEnable()
		{
			textProperty = serializedObject.FindProperty("text");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var lastText = textProperty.stringValue;
			EditorGUILayout.PropertyField(textProperty);

			serializedObject.ApplyModifiedProperties();
			
			if (lastText != textProperty.stringValue)
			{
				(target as TextCurve).UpdateText(true);
			}

			EditorUtility.SetDirty(target);

			base.OnInspectorGUI();
		}
	}
}