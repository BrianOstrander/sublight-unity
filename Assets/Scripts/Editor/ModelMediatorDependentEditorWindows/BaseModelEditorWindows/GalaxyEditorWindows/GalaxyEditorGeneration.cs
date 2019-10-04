using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class GenerationGalaxyEditorTab : ModelEditorTab<GalaxyEditorWindow, GalaxyInfoModel>
	{
		EditorPrefsFloat generationBarScroll;

		public GenerationGalaxyEditorTab(GalaxyEditorWindow window) : base(window, "Generation")
		{
			generationBarScroll = new EditorPrefsFloat(TabKeyPrefix + "BarScroll");
		}

		public override void Gui(GalaxyInfoModel model)
		{
			generationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, generationBarScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					if (model.MaximumSectorSystemCount < model.MinimumSectorSystemCount) EditorGUILayout.HelpBox("Maximum Sector System Count must be higher than the minimum", MessageType.Error);
					model.MinimumSectorSystemCount.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Minimum Sector System Count", "The minimum bodies ever spawned in a sector."), model.MinimumSectorSystemCount));
					model.MaximumSectorSystemCount.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Maximum Sector System Count", "The maximum bodies ever spawned in a sector."), model.MaximumSectorSystemCount));

					model.SectorSystemChance.Value = EditorGUILayoutAnimationCurve.Field(new GUIContent("Sector System Chance", "The bodymap is a linear gradient that is evaluated along a curve, then remapped between the minimum and maximum sector body count."), model.SectorSystemChance.Value);
				}
				EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
			}
			GUILayout.EndScrollView();
		}

	}
}