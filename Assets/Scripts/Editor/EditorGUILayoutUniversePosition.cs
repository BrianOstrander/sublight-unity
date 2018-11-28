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
			var sectorPos = position.SectorInteger;
			var localPos = position.Local;

			EditorGUILayout.LabelField(content);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.Label("Sector", GUILayout.Width(48f));
				sectorPos = EditorGUILayout.Vector3IntField(GUIContent.none, sectorPos);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.Label("Local", GUILayout.Width(48f));
				localPos = EditorGUILayout.Vector3Field(GUIContent.none, localPos);
			}
			GUILayout.EndHorizontal();

			return new UniversePosition(sectorPos, localPos);
		}

		public static UniversePosition FieldSector(string name, UniversePosition position)
		{
			return FieldSector(new GUIContent(name), position);
		}

		public static UniversePosition FieldSector(GUIContent content, UniversePosition position)
		{
			var sectorPos = position.SectorInteger;
			var localPos = position.Local;

			EditorGUILayout.LabelField(content);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.Label("Sector", GUILayout.Width(48f));
				sectorPos = EditorGUILayout.Vector3IntField(GUIContent.none, sectorPos);
			}
			GUILayout.EndHorizontal();

			return new UniversePosition(sectorPos, localPos);
		}
	}
}