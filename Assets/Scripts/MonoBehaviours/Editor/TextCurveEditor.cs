using UnityEngine;
using UnityEditor;

namespace LunraGames.SubLight.Views
{
	[CustomEditor(typeof(TextCurve), true)]
	[CanEditMultipleObjects]
	public class TextCurveEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			{
				base.OnInspectorGUI();
			}
			var changed = EditorGUI.EndChangeCheck();

			if (changed) (target as TextCurve).UpdateText(true);
		}
	}
}