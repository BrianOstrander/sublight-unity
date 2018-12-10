using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.NumberDemon;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GalaxyEditorWindow
	{
		enum SpecifiedSectorsStates
		{
			Unknown = 0,
			Idle = 10,
			SelectingSector = 20,
			UpdatingSector = 30
		}

		EditorPrefsString specifiedSectorsSelectedSectorName;
		EditorPrefsInt specifiedSectorsPreviewSize;
		EditorPrefsBool specifiedSectorsPreviewMinimized;
		EditorPrefsInt specifiedSectorsSelectedPreview;
		EditorPrefsFloat specifiedSectorsListScroll;
		EditorPrefsFloat specifiedSectorsDetailsScroll;
		EditorPrefsBool specifiedSectorsShowTargets;
		EditorPrefsBool specifiedSectorsShowDerivedValues;

		SpecifiedSectorsStates specifiedSectorsState = SpecifiedSectorsStates.Idle;
		SectorModel specifiedSectorsSelectedSectorLast;
		SectorModel specifiedSectorsSelectedSector;
		bool specifiedSectorsIsOverAnAllSector;

		void SpecifiedSectorsConstruct()
		{
			var currPrefix = KeyPrefix + "SpecifiedSectors";

			specifiedSectorsSelectedSectorName = new EditorPrefsString(currPrefix + "SelectedSectorName");
			specifiedSectorsPreviewSize = new EditorPrefsInt(currPrefix + "PreviewSize");
			specifiedSectorsPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");
			specifiedSectorsSelectedPreview = new EditorPrefsInt(currPrefix + "SelectedPreview");
			specifiedSectorsListScroll = new EditorPrefsFloat(currPrefix + "ListScroll");
			specifiedSectorsDetailsScroll = new EditorPrefsFloat(currPrefix + "DetailsScroll");
			specifiedSectorsShowTargets = new EditorPrefsBool(currPrefix + "ShowTargets");
			specifiedSectorsShowDerivedValues = new EditorPrefsBool(currPrefix + "ShowDerivedValues");

			RegisterToolbar("Specified Sectors", SpecifiedSectorsToolbar);

			BeforeLoadSelection += SpecifiedSectorsBeforeLoadSelection;
		}

		void SpecifiedSectorsToolbar(GalaxyInfoModel model)
		{
			if (string.IsNullOrEmpty(specifiedSectorsSelectedSectorName.Value))
			{
				if (specifiedSectorsSelectedSector != null)
				{
					SpecifiedSectorsSelectSpecifiedSector(null);
				}
			}
			else if (specifiedSectorsSelectedSector == null)
			{
				SpecifiedSectorsSelectSpecifiedSector(model.GetSpecifiedSector(specifiedSectorsSelectedSectorName.Value));
			}

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical(GUILayout.Width(350f));
				{
					labelsListScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, labelsListScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
					{
						var sectorIndex = -1;
						foreach (var specifiedSector in model.GetSpecifiedSectors())
						{
							sectorIndex++;

							var isSelectedSector = specifiedSector.Name.Value == specifiedSectorsSelectedSectorName.Value;
							if (isSelectedSector) EditorGUILayoutExtensions.PushColor(Color.blue.NewS(0.7f));
							GUILayout.BeginHorizontal(EditorStyles.helpBox);
							if (isSelectedSector) EditorGUILayoutExtensions.PopColor();
							{
								EditorGUILayoutExtensions.PushColor(EditorUtilityExtensions.ColorFromIndex(sectorIndex));
								GUILayout.BeginHorizontal(EditorStyles.helpBox);
								EditorGUILayoutExtensions.PopColor();
								{
									GUILayout.Label(specifiedSector.Name.Value, EditorStyles.boldLabel);
								}
								GUILayout.EndHorizontal();

								EditorGUILayoutExtensions.PushEnabled(!isSelectedSector);
								if (GUILayout.Button("Edit Sector", GUILayout.Width(100f)))
								{
									SpecifiedSectorsSelectSpecifiedSector(specifiedSector);
								}
								EditorGUILayoutExtensions.PopEnabled();

								if (EditorGUILayoutExtensions.XButton())
								{
									if (specifiedSectorsSelectedSector == specifiedSector)
									{
										SpecifiedSectorsSelectSpecifiedSector(null);
									}
									model.RemoveSpecifiedSector(specifiedSector);
									ModelSelectionModified = true;
								}
							}
							GUILayout.EndHorizontal();
						}

					}
					GUILayout.EndScrollView();

					GUILayout.BeginHorizontal();
					{
						switch (specifiedSectorsState)
						{
							case SpecifiedSectorsStates.Idle:
								EditorGUILayout.HelpBox("Click on preview to specify a new sector.", MessageType.Info);
								break;
							case SpecifiedSectorsStates.SelectingSector:
								EditorGUILayout.HelpBox("Click on preview to select where the sector will be.", MessageType.Info);
								break;
							case SpecifiedSectorsStates.UpdatingSector:
								EditorGUILayout.HelpBox("Click on preview to select a new sector.", MessageType.Info);
								break;
						}

						GUILayout.BeginVertical(GUILayout.Width(72f));
						{
							EditorGUILayoutExtensions.PushEnabled(specifiedSectorsState == SpecifiedSectorsStates.Idle && specifiedSectorsSelectedSector != null);
							if (GUILayout.Button("Deselect"))
							{
								SpecifiedSectorsSelectSpecifiedSector(null);
							}
							EditorGUILayoutExtensions.PopEnabled();

							EditorGUILayoutExtensions.PushEnabled(labelsLabelState != LabelStates.Idle);
							EditorGUILayoutExtensions.PushColor(Color.red);
							if (GUILayout.Button("Cancel"))
							{
								switch (specifiedSectorsState)
								{
									case SpecifiedSectorsStates.SelectingSector:
										SpecifiedSectorsSelectSpecifiedSector(specifiedSectorsSelectedSectorLast);
										break;
									case SpecifiedSectorsStates.UpdatingSector:
										SpecifiedSectorsSelectSpecifiedSector(specifiedSectorsSelectedSector);
										break;
								}
								labelsLabelState = LabelStates.Idle;
							}
							EditorGUILayoutExtensions.PopColor();
							EditorGUILayoutExtensions.PopEnabled();

						}
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				specifiedSectorsDetailsScroll.VerticalScroll = GUILayout.BeginScrollView(specifiedSectorsDetailsScroll.VerticalScroll, false, true, GUILayout.ExpandHeight(true));
				{
					if (specifiedSectorsSelectedSector == null)
					{
						EditorGUILayout.HelpBox("Select a sector to edit it.", MessageType.Info);
					}
					else
					{
						EditorGUILayoutExtensions.PushEnabled(specifiedSectorsState == SpecifiedSectorsStates.Idle);
						{
							SpecifiedSectorsDetails(model, specifiedSectorsSelectedSector);
						}
						EditorGUILayoutExtensions.PopEnabled();
					}
				}
				GUILayout.EndScrollView();

			}
			GUILayout.EndHorizontal();

			DrawPreviews(
				model,
				specifiedSectorsSelectedPreview,
				specifiedSectorsPreviewSize,
				specifiedSectorsPreviewMinimized,
				!specifiedSectorsIsOverAnAllSector,
				clickPosition => SpecifiedSectorsPrimaryClickPreview(model, clickPosition),
				clickPosition => SpecifiedSectorsSecondaryClickPreview(model, clickPosition),
				() => SpecifiedSectorsDrawPreviewToolbarPrefix(model),
				() => SpecifiedSectorsDrawPreviewToolbarSuffix(model),
				displayArea => SpecifiedSectorsDrawOnPreview(model, displayArea) 
			);
		}

		void SpecifiedSectorsDrawPreviewToolbarPrefix(GalaxyInfoModel model)
		{
			specifiedSectorsShowTargets.Value = GUILayout.Toggle(specifiedSectorsShowTargets.Value, "Show Targets", GUILayout.ExpandWidth(false));
		}

		void SpecifiedSectorsDrawPreviewToolbarSuffix(GalaxyInfoModel model)
		{

		}

		void SpecifiedSectorsDrawOnPreview(GalaxyInfoModel model, Rect displayArea)
		{
			specifiedSectorsIsOverAnAllSector = false;
			if (specifiedSectorsSelectedSector == null)
			{
				// TODO: show all!
				SpecifiedSectorsShowAll(model, displayArea);
				return;
			}

			var positionInPreview = true;
			var normalizedSectorPosition = UniversePosition.NormalizedSector(specifiedSectorsSelectedSector.Position.Value, model.GalaxySize);

			var positionInWindow = NormalToWindow(normalizedSectorPosition, displayArea, out positionInPreview);

			var positionColor = specifiedSectorsState == SpecifiedSectorsStates.UpdatingSector ? Color.cyan.NewS(0.25f) : Color.cyan;

			EditorGUILayoutExtensions.PushColor(positionInPreview ? positionColor : positionColor.NewA(positionColor.a * 0.5f));
			{
				GUI.Box(CenteredScreen(positionInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Sector Position"), SubLightEditorConfig.Instance.GalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			if (specifiedSectorsShowTargets.Value) DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyTargetStyleSmall);
		}

		void SpecifiedSectorsShowAll(
			GalaxyInfoModel model,
			Rect displayArea
		)
		{
			var sectors = model.GetSpecifiedSectors();
			var sectorIndex = -1;

			foreach (var sector in sectors)
			{
				sectorIndex++;

				var color = EditorUtilityExtensions.ColorFromIndex(sectorIndex);

				EditorGUILayoutExtensions.PushColor(color);

				var positionInPreview = true;
				var normalizedSectorPosition = UniversePosition.NormalizedSector(sector.Position.Value, model.GalaxySize);

				var positionInWindow = NormalToWindow(normalizedSectorPosition, displayArea, out positionInPreview);

				var selectCurrentArea = CenteredScreen(positionInWindow, new Vector2(16f, 16f));

				if (GUI.Button(selectCurrentArea, new GUIContent(string.Empty, sector.Name.Value), SubLightEditorConfig.Instance.SpecifiedSectorTargetStyle))
				{
					SpecifiedSectorsSelectSpecifiedSector(sector);
				}

				specifiedSectorsIsOverAnAllSector = specifiedSectorsIsOverAnAllSector || selectCurrentArea.Contains(Event.current.mousePosition);

				EditorGUILayoutExtensions.PopColor();
			}

			if (specifiedSectorsShowTargets.Value) DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyTargetStyleSmall);
		}

		void SpecifiedSectorsPrimaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition)
		{
			switch (specifiedSectorsState)
			{
				case SpecifiedSectorsStates.Idle:
					specifiedSectorsSelectedSectorLast = specifiedSectorsSelectedSector;
					specifiedSectorsSelectedSector = SpecifiedSectorsCreateSector();
					specifiedSectorsSelectedSectorName.Value = specifiedSectorsSelectedSector.Name.Value;
					specifiedSectorsSelectedSector.Position.Value = UniversePosition.Lerp(clickPosition, UniversePosition.Zero, model.GalaxySize).LocalZero;

					specifiedSectorsState = SpecifiedSectorsStates.SelectingSector;

					TextDialogPopup.Show(
						"New Specified Sector",
						value =>
						{
							specifiedSectorsSelectedSector.Name.Value = value;
							model.AddSpecifiedSector(specifiedSectorsSelectedSector);
							ModelSelectionModified = true;
							specifiedSectorsState = SpecifiedSectorsStates.Idle;
						},
						() =>
						{
							SpecifiedSectorsSelectSpecifiedSector(specifiedSectorsSelectedSectorLast);
						}
					);
					break;
				case SpecifiedSectorsStates.SelectingSector:
					specifiedSectorsSelectedSector.Position.Value = UniversePosition.Lerp(clickPosition, UniversePosition.Zero, model.GalaxySize).LocalZero;
					model.AddSpecifiedSector(specifiedSectorsSelectedSector);
					ModelSelectionModified = true;
					specifiedSectorsState = SpecifiedSectorsStates.Idle;
					break;
				case SpecifiedSectorsStates.UpdatingSector:
					specifiedSectorsSelectedSector.Position.Value = UniversePosition.Lerp(clickPosition, UniversePosition.Zero, model.GalaxySize).LocalZero;
					ModelSelectionModified = true;
					specifiedSectorsState = SpecifiedSectorsStates.Idle;
					break;
				default:
					Debug.LogError("Unrecognized state " + labelsLabelState);
					break;
			}
		}

		void SpecifiedSectorsSecondaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition)
		{
			switch (specifiedSectorsState)
			{
				case SpecifiedSectorsStates.Idle:
					SpecifiedSectorsSelectSpecifiedSector(null);
					return;
				case SpecifiedSectorsStates.SelectingSector:
					SpecifiedSectorsSelectSpecifiedSector(specifiedSectorsSelectedSectorLast);
					break;
				case SpecifiedSectorsStates.UpdatingSector:
					SpecifiedSectorsSelectSpecifiedSector(specifiedSectorsSelectedSector);
					break;
			}
			specifiedSectorsState = SpecifiedSectorsStates.Idle;
		}

		SectorModel SpecifiedSectorsCreateSector()
		{
			var result = new SectorModel();
			result.Name.Value = Guid.NewGuid().ToString();
			result.Seed.Value = DemonUtility.NextInteger;
			result.Specified.Value = true;

			return result;
		}

		void SpecifiedSectorsBeforeLoadSelection()
		{
			SpecifiedSectorsSelectSpecifiedSector(null);
		}

		void SpecifiedSectorsSelectSpecifiedSector(SectorModel sector, SpecifiedSectorsStates state = SpecifiedSectorsStates.Idle)
		{
			specifiedSectorsSelectedSector = sector;
			specifiedSectorsSelectedSectorName.Value = sector == null ? null : sector.Name.Value;
			if (state != SpecifiedSectorsStates.Unknown) specifiedSectorsState = state;
			GUIUtility.keyboardControl = 0;
		}

		void SpecifiedSectorsDetails(GalaxyInfoModel model, SectorModel sector)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					sector.Name.Value = EditorGUILayout.TextField("Name", sector.Name.Value);
					EditorGUILayout.LabelField("Show Derived Values", GUILayout.Width(115f));
					EditorGUIExtensions.PauseChangeCheck();
					specifiedSectorsShowDerivedValues.Value = EditorGUILayout.Toggle(specifiedSectorsShowDerivedValues.Value, GUILayout.Width(16f));
					EditorGUIExtensions.UnPauseChangeCheck();
				}
				GUILayout.EndHorizontal();

				sector.Seed.Value = EditorGUILayout.IntField("Seed", sector.Seed.Value);
				sector.Visited.Value = EditorGUILayout.Toggle("Visited", sector.Visited.Value);

				if (specifiedSectorsShowDerivedValues.Value)
				{
					EditorGUILayoutExtensions.PushEnabled(false);
					{
						EditorGUILayout.Toggle("Specified", sector.Specified.Value);
						EditorGUILayout.IntField("System Count", sector.SystemCount.Value);
					}
					EditorGUILayoutExtensions.PopEnabled();
				}

				GUILayout.BeginHorizontal(EditorStyles.helpBox);
				{
					GUILayout.BeginVertical();
					{
						sector.Position.Value = EditorGUILayoutUniversePosition.FieldSector("Sector", sector.Position.Value).LocalZero;
					}
					GUILayout.EndVertical();
					EditorGUILayoutExtensions.PushBackgroundColor(Color.cyan);
					if (GUILayout.Button("Update Sector", GUILayout.Width(100f), GUILayout.Height(36f)))
					{
						specifiedSectorsState = SpecifiedSectorsStates.UpdatingSector;
					}
					EditorGUILayoutExtensions.PopBackgroundColor();
				}
				GUILayout.EndHorizontal();

				SpecifiedSectorsListSystems(model, sector);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
		}

		void SpecifiedSectorsListSystems(GalaxyInfoModel model, SectorModel sector)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Systems");
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Add System", GUILayout.Width(100f))) SpecifiedSectorsCreateSystem(model, sector);
			}
			GUILayout.EndHorizontal();

			SystemModel deletedSystem = null;
			SystemModel beginSystem = null;
			SystemModel endSystem = null;

			foreach (var system in sector.Systems.Value.OrderBy(s => s.Index.Value))
			{
				var isDeleted = false;
				var setToPlayerBegin = false;
				var setToPlayerEnd = false;

				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					SpecifiedSectorsDrawSystem(model, sector, system, out isDeleted, out setToPlayerBegin, out setToPlayerEnd);
				}
				GUILayout.EndHorizontal();

				if (isDeleted) deletedSystem = system;
				if (setToPlayerBegin) beginSystem = system;
				if (setToPlayerEnd) endSystem = system;
			}

			if (deletedSystem != null)
			{
				sector.Systems.Value = sector.Systems.Value.ExceptOne(deletedSystem).ToArray();
				sector.SystemCount.Value = sector.Systems.Value.Count();

				var newIndex = 0;
				foreach (var system in sector.Systems.Value.OrderBy(s => s.Index.Value))
				{
					system.Index.Value = newIndex;
					newIndex++;
				}
			}

			if (beginSystem != null || endSystem != null)
			{
				var changingBegin = beginSystem != null;
				var changingEnd = endSystem != null;
				foreach (var otherSector in model.GetSpecifiedSectors())
				{
					foreach (var otherSystem in otherSector.Systems.Value)
					{
						if (changingBegin && otherSystem != beginSystem) otherSystem.PlayerBegin.Value = false;
						if (changingEnd && otherSystem != endSystem) otherSystem.PlayerEnd.Value = false;
					}
				}
			}
		}

		void SpecifiedSectorsDrawSystem(
			GalaxyInfoModel model,
			SectorModel sector,
			SystemModel system,
			out bool deleted,
			out bool setToPlayerBegin,
			out bool setToPlayerEnd
		)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("#" + system.Index.Value + " | " + (string.IsNullOrEmpty(system.Name.Value) ? "< Null or Empty Name >" : system.Name.Value), EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				deleted = EditorGUILayoutExtensions.XButton();
			}
			GUILayout.EndHorizontal();

			system.Name.Value = EditorGUILayout.TextField(new GUIContent("Name"), system.Name.Value);

			system.Visited.Value = EditorGUILayout.Toggle("Visited", system.Visited.Value);

			GUILayout.BeginHorizontal();
			{
				system.SecondaryClassification.Value = EditorGUILayout.TextField("Classification", system.SecondaryClassification.Value);
				system.PrimaryClassification.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select a Primary Classification -", system.PrimaryClassification.Value, GUILayout.Width(150f));
			}
			GUILayout.EndHorizontal();

			system.IconColor.Value = EditorGUILayout.ColorField("Icon Color", system.IconColor.Value);
			system.IconScale.Value = EditorGUILayout.Slider("Icon Scale", system.IconScale.Value, 0f, 1f);

			// --- Begin / End
			var wasPlayerBegin = system.PlayerBegin.Value;
			var wasPlayerEnd = system.PlayerEnd.Value;

			system.PlayerBegin.Value = EditorGUILayout.Toggle("Player Begin", system.PlayerBegin.Value);
			system.PlayerEnd.Value = EditorGUILayout.Toggle("Player End", system.PlayerEnd.Value);

			setToPlayerBegin = !wasPlayerBegin && system.PlayerBegin.Value;
			setToPlayerEnd = !wasPlayerEnd && system.PlayerEnd.Value;
			// ---

			if (specifiedSectorsShowDerivedValues.Value)
			{
				EditorGUILayoutExtensions.PushEnabled(false);
				{
					EditorGUILayout.Toggle("Specified", system.Specified.Value);
					EditorGUILayout.Vector3IntField("Sector", system.Position.Value.SectorInteger);
				}
				EditorGUILayoutExtensions.PopEnabled();
			}

			var localPos = new UniversePosition(EditorGUILayout.Vector3Field("Local Position", system.Position.Value.Local));
			system.Position.Value = new UniversePosition(sector.Position.Value.SectorInteger, localPos.Local);
		}

		void SpecifiedSectorsCreateSystem(GalaxyInfoModel model, SectorModel sector)
		{
			var currSystems = sector.Systems.Value.OrderBy(s => s.Index.Value);
			var result = new SystemModel();
			result.Index.Value = currSystems.None() ? 0 : currSystems.Last().Index.Value + 1;
			result.Seed.Value = DemonUtility.NextInteger;
			result.Specified.Value = true;
			result.Position.Value = sector.Position.Value;

			sector.Systems.Value = sector.Systems.Value.Append(result).ToArray();
			sector.SystemCount.Value = sector.Systems.Value.Count();
		}
	}
}