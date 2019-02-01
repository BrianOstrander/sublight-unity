using UnityEngine;

using LunraGamesEditor.Singletonnes;

namespace LunraGames.SubLight
{
	public class SubLightEditorConfig : EditorScriptableSingleton<SubLightEditorConfig>
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[Header("Shared Model Editor Styles")]
		[SerializeField]
		Texture sharedModelEditorOpenModelsImage;
		public Texture SharedModelEditorOpenModelsImage { get { return sharedModelEditorOpenModelsImage; } }

		[SerializeField]
		Texture sharedModelEditorCloseModelsImage;
		public Texture SharedModelEditorCloseModelsImage { get { return sharedModelEditorCloseModelsImage; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsBackground;
		public GUIStyle SharedModelEditorModelsBackground { get { return sharedModelEditorModelsBackground; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsEntryBackground;
		public GUIStyle SharedModelEditorModelsEntryBackground { get { return sharedModelEditorModelsEntryBackground; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsEntrySelectedBackground;
		public GUIStyle SharedModelEditorModelsEntrySelectedBackground { get { return sharedModelEditorModelsEntrySelectedBackground; } }

		[Header("Encounter Editor Styles")]
		[SerializeField]
		SkinColor encounterEditorLogBackgroundColor = SkinColor.Default;
		public Color EncounterEditorLogBackgroundColor { get { return encounterEditorLogBackgroundColor; } }
		[SerializeField]
		GUIStyle encounterEditorLogBackground;
		public GUIStyle EncounterEditorLogBackground { get { return encounterEditorLogBackground; } }

		[SerializeField]
		SkinColor encounterEditorLogEntryBackgroundColor = SkinColor.Default;
		public Color EncounterEditorLogEntryBackgroundColor { get { return encounterEditorLogEntryBackgroundColor; } }
		[SerializeField]
		GUIStyle encounterEditorLogEntryBackground;
		public GUIStyle EncounterEditorLogEntryBackground { get { return encounterEditorLogEntryBackground; } }
		[SerializeField]
		GUIStyle encounterEditorLogEntryCollapsedBackground;
		public GUIStyle EncounterEditorLogEntryCollapsedBackground { get { return encounterEditorLogEntryCollapsedBackground; } }

		[SerializeField]
		GUIStyle encounterEditorLogEntryIndex;
		public GUIStyle EncounterEditorLogEntryIndex { get { return encounterEditorLogEntryIndex; } }

		[SerializeField]
		Texture encounterEditorLogToolbarLastImage;
		public Texture EncounterEditorLogToolbarLastImage { get { return encounterEditorLogToolbarLastImage; } }

		[SerializeField]
		Texture encounterEditorLogToolbarNextImage;
		public Texture EncounterEditorLogToolbarNextImage { get { return encounterEditorLogToolbarNextImage; } }

		[Header("Galaxy Editor Styles")]
		[SerializeField]
		GUIStyle galaxyEditorGalaxyTargetStyle;
		public GUIStyle GalaxyEditorGalaxyTargetStyle { get { return galaxyEditorGalaxyTargetStyle; } }

		[SerializeField]
		GUIStyle galaxyEditorLabelAnchorStyle;
		public GUIStyle GalaxyEditorLabelAnchorStyle { get { return galaxyEditorLabelAnchorStyle; } }

		[SerializeField]
		GUIStyle galaxyEditorLabelCurvePointStyle;
		public GUIStyle GalaxyEditorLabelCurvePointStyle { get { return galaxyEditorLabelCurvePointStyle; } }

		[SerializeField]
		GUIStyle galaxyEditorLabelCurveCenterStyle;
		public GUIStyle GalaxyEditorLabelCurveCenterStyle { get { return galaxyEditorLabelCurveCenterStyle; } }
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public GUIStyle GalaxyEditorSpecifiedSectorTargetStyle { get { return galaxyEditorGalaxyTargetStyle; } }
		public GUIStyle GalaxyEditorGalaxyTargetStyleSmall { get { return galaxyEditorLabelCurvePointStyle; } }
	}
}