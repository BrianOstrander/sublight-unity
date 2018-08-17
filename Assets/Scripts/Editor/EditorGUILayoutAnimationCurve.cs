using UnityEditor;

using UnityEngine;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutAnimationCurve
	{
		public static AnimationCurve Field(string name, AnimationCurve curve)
		{
			return Field(new GUIContent(name), curve);
		}

		public static AnimationCurve Field(GUIContent content, AnimationCurve curve)
		{
			// I dunno but it likes to exit gui when you click on it.
			try
			{
				return EditorGUILayout.CurveField(content, curve);
			}
			catch (ExitGUIException)
			{
				return curve;
			}
		}
	}
}