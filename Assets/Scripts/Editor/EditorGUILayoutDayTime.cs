using UnityEditor;

using UnityEngine;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutDayTime
	{
		public static DayTime Field(string name, DayTime dayTime)
		{
			return Field(new GUIContent(name), dayTime);
		}

		public static DayTime Field(GUIContent content, DayTime dayTime)
		{
			var day = dayTime.Day;
			var time = dayTime.Time;

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel(content);
				day = EditorGUILayout.IntField(day);
				time = EditorGUILayout.FloatField(time);
			}
			GUILayout.EndHorizontal();

			return new DayTime(day, time);
		}

		public static DayTime FieldYear(string name, DayTime dayTime)
		{
			return Field(new GUIContent(name), dayTime);
		}

		public static DayTime FieldYear(GUIContent content, DayTime dayTime)
		{
			var year = dayTime.TotalYears;

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel(content);
				year = EditorGUILayout.FloatField(year);
				GUILayout.Label("Years", GUILayout.ExpandWidth(false));
			}
			GUILayout.EndHorizontal();

			return DayTime.FromYear(year);
		}
	}
}