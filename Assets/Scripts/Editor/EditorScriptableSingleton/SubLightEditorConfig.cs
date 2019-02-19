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
		SkinColor sharedModelEditorModelsBackgroundColor = SkinColor.Default;
		public Color SharedModelEditorModelsBackgroundColor { get { return sharedModelEditorModelsBackgroundColor; } }
		[SerializeField]
		GUIStyle sharedModelEditorModelsBackground;
		public GUIStyle SharedModelEditorModelsBackground { get { return sharedModelEditorModelsBackground; } }

		[SerializeField]
		SkinColor sharedModelEditorModelsEntryBackgroundColor = SkinColor.Default;
		public Color SharedModelEditorModelsEntryBackgroundColor { get { return sharedModelEditorModelsEntryBackgroundColor; } }
		[SerializeField]
		GUIStyle sharedModelEditorModelsEntryBackground;
		public GUIStyle SharedModelEditorModelsEntryBackground { get { return sharedModelEditorModelsEntryBackground; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsEntrySelectedBackground;
		public GUIStyle SharedModelEditorModelsEntrySelectedBackground { get { return sharedModelEditorModelsEntrySelectedBackground; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsFilterEntryIcon;
		public GUIStyle SharedModelEditorModelsFilterEntryIcon { get { return sharedModelEditorModelsFilterEntryIcon; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsFilterEntryIconLinked;
		public GUIStyle SharedModelEditorModelsFilterEntryIconLinked { get { return sharedModelEditorModelsFilterEntryIconLinked; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsFilterEntryIconNotLinked;
		public GUIStyle SharedModelEditorModelsFilterEntryIconNotLinked { get { return sharedModelEditorModelsFilterEntryIconNotLinked; } }

		[SerializeField]
		GUIStyle sharedModelEditorModelsFilterEntryDescription;
		public GUIStyle SharedModelEditorModelsFilterEntryDescription { get { return sharedModelEditorModelsFilterEntryDescription; } }

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
		SkinColor encounterEditorLogEntryIndexTextColor = SkinColor.Default;
		[SerializeField]
		GUIStyle encounterEditorLogEntryIndex;
		public GUIStyle EncounterEditorLogEntryIndex
		{
			get
			{
				var result = new GUIStyle(encounterEditorLogEntryIndex);
				result.normal.textColor = encounterEditorLogEntryIndexTextColor;
				return result;
			}
		}

		[SerializeField]
		Texture encounterEditorLogToolbarLastImage;
		public Texture EncounterEditorLogToolbarLastImage { get { return encounterEditorLogToolbarLastImage; } }

		[SerializeField]
		Texture encounterEditorLogToolbarNextImage;
		public Texture EncounterEditorLogToolbarNextImage { get { return encounterEditorLogToolbarNextImage; } }

		[SerializeField]
		SkinColor encounterEditorLogKeyValueOperationLabelsTextColor = SkinColor.Default;
		[SerializeField]
		GUIStyle encounterEditorLogKeyValueOperationLabels;
		public GUIStyle EncounterEditorLogKeyValueOperationLabels
		{
			get
			{
				var result = new GUIStyle(encounterEditorLogKeyValueOperationLabels);
				result.normal.textColor = encounterEditorLogKeyValueOperationLabelsTextColor;
				return result;
			}
		}

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