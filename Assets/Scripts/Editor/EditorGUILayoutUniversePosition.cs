using UnityEditor;

using UnityEngine;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutUniversePosition
	{
		public static UniversePosition Field(string name, UniversePosition position)
		{
			return Field(new GUIContent(name), position);
		}

		public static UniversePosition Field(GUIContent content, UniversePosition position)
		{
			var sector = position.Sector;
			var system = position.System;

			EditorGUILayout.LabelField(content);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.Label("Sector", GUILayout.Width(48f));
				sector = EditorGUILayout.Vector3Field(GUIContent.none, sector);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.Label("System", GUILayout.Width(48f));
				system = EditorGUILayout.Vector3Field(GUIContent.none, system);
			}
			GUILayout.EndHorizontal();

			return new UniversePosition(sector, system);
		}

		public static UniversePosition FieldSector(string name, UniversePosition position)
		{
			return FieldSector(new GUIContent(name), position);
		}

		public static UniversePosition FieldSector(GUIContent content, UniversePosition position)
		{
			var sector = position.Sector;
			var system = position.System;

			EditorGUILayout.LabelField(content);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.Label("Sector", GUILayout.Width(48f));
				sector = EditorGUILayout.Vector3Field(GUIContent.none, sector);
			}
			GUILayout.EndHorizontal();

			sector = new Vector3(Mathf.Round(sector.x), Mathf.Round(sector.y), Mathf.Round(sector.z));

			return new UniversePosition(sector, system);
		}
	}
}