using UnityEngine;
using UnityEditor;
using LunraGamesEditor.Singletonnes;

namespace LunraGames.SubLight
{
	public class SubLightEditorConfig : EditorScriptableSingleton<SubLightEditorConfig>
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		GUIStyle galaxyTargetStyle;
		public GUIStyle GalaxyTargetStyle { get { return galaxyTargetStyle; } }

		[SerializeField]
		GUIStyle labelAnchorStyle;
		public GUIStyle LabelAnchorStyle { get { return labelAnchorStyle; } }

		[SerializeField]
		GUIStyle labelCurvePointStyle;
		public GUIStyle LabelCurvePointStyle { get { return labelCurvePointStyle; } }

		[SerializeField]
		GUIStyle labelCurveCenterStyle;
		public GUIStyle LabelCurveCenterStyle { get { return labelCurveCenterStyle; } }
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public GUIStyle SpecifiedSectorTargetStyle { get { return galaxyTargetStyle; } }
		public GUIStyle GalaxyTargetStyleSmall { get { return labelCurvePointStyle; } }
	}
}