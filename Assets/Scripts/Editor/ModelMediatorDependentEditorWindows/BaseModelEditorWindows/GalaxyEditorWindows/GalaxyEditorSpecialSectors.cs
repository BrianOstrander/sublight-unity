using System;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

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
		DevPrefsInt specifiedSectorsPreviewSize;
		EditorPrefsBool specifiedSectorsPreviewMinimized;
		DevPrefsInt specifiedSectorsSelectedPreview;
		EditorPrefsFloat specifiedSectorsListScroll;
		EditorPrefsFloat specifiedSectorsDetailsScroll;
		EditorPrefsBool specifiedSectorsShowTargets;

		SpecifiedSectorsStates specifiedSectorsState = SpecifiedSectorsStates.Idle;
		SectorModel specifiedSectorsSelectedSectorLast;
		SectorModel specifiedSectorsSelectedSector;
		bool specifiedSectorsIsOverAnAllSector;

		void OnSpecifiedSectorsConstruct()
		{
			var currPrefix = KeyPrefix + "SpecifiedSectors";

			specifiedSectorsSelectedSectorName = new EditorPrefsString(currPrefix + "SelectedSectorName");
			specifiedSectorsPreviewSize = new DevPrefsInt(currPrefix + "PreviewSize");
			specifiedSectorsPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");
			specifiedSectorsSelectedPreview = new DevPrefsInt(currPrefix + "SelectedPreview");
			specifiedSectorsListScroll = new EditorPrefsFloat(currPrefix + "ListScroll");
			specifiedSectorsDetailsScroll = new EditorPrefsFloat(currPrefix + "DetailsScroll");
			specifiedSectorsShowTargets = new EditorPrefsBool(currPrefix + "ShowTargets");

			RegisterToolbar("Specified Sectors", OnSpecifiedSectorsToolbar);

			BeforeLoadSelection += OnBeforeLoadSelectionSpecifiedSectors;
		}

		void OnSpecifiedSectorsToolbar(GalaxyInfoModel model)
		{
			if (string.IsNullOrEmpty(specifiedSectorsSelectedSectorName.Value))
			{
				if (specifiedSectorsSelectedSector != null)
				{
					SelectSpecifiedSector(null);
				}
			}
			else if (specifiedSectorsSelectedSector == null)
			{
				SelectSpecifiedSector(model.GetSpecifiedSector(specifiedSectorsSelectedSectorName.Value));
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
									SelectSpecifiedSector(specifiedSector);
								}
								EditorGUILayoutExtensions.PopEnabled();

								if (EditorGUILayoutExtensions.XButton())
								{
									if (specifiedSectorsSelectedSector == specifiedSector)
									{
										SelectSpecifiedSector(null);
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
								SelectSpecifiedSector(null);
							}
							EditorGUILayoutExtensions.PopEnabled();

							EditorGUILayoutExtensions.PushEnabled(labelsLabelState != LabelStates.Idle);
							EditorGUILayoutExtensions.PushColor(Color.red);
							if (GUILayout.Button("Cancel"))
							{
								switch (specifiedSectorsState)
								{
									case SpecifiedSectorsStates.SelectingSector:
										SelectSpecifiedSector(specifiedSectorsSelectedSectorLast);
										break;
									case SpecifiedSectorsStates.UpdatingSector:
										SelectSpecifiedSector(specifiedSectorsSelectedSector);
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
							EditorGUIExtensions.BeginChangeCheck();
							{
								specifiedSectorsSelectedSector.Name.Value = EditorGUILayout.TextField("Name", specifiedSectorsSelectedSector.Name.Value);

								GUILayout.BeginHorizontal(EditorStyles.helpBox);
								{
									GUILayout.BeginVertical();
									{
										specifiedSectorsSelectedSector.Position.Value = EditorGUILayoutUniversePosition.FieldSector("Sector", specifiedSectorsSelectedSector.Position.Value).LocalZero;
									}
									GUILayout.EndVertical();
									EditorGUILayoutExtensions.PushBackgroundColor(Color.cyan);
									if (GUILayout.Button("Update Sector", GUILayout.Width(100f), GUILayout.Height(51f)))
									{
										specifiedSectorsState = SpecifiedSectorsStates.UpdatingSector;
									}
									EditorGUILayoutExtensions.PopBackgroundColor();
								}
								GUILayout.EndHorizontal();
							}
							EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
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
				clickPosition => OnSpecifiedSectorsPrimaryClickPreview(model, clickPosition),
				clickPosition => OnSpecifiedSectorsSecondaryClickPreview(model, clickPosition),
				() => OnSpecifiedSectorsDrawPreviewToolbarPrefix(model),
				() => OnSpecifiedSectorsDrawPreviewToolbarSuffix(model),
				displayArea => OnSpecifiedSectorsDrawOnPreview(model, displayArea) 
			);
		}

		void OnSpecifiedSectorsDrawPreviewToolbarPrefix(GalaxyInfoModel model)
		{
			specifiedSectorsShowTargets.Value = GUILayout.Toggle(specifiedSectorsShowTargets.Value, "Show Targets", GUILayout.ExpandWidth(false));
		}

		void OnSpecifiedSectorsDrawPreviewToolbarSuffix(GalaxyInfoModel model)
		{

		}

		void OnSpecifiedSectorsDrawOnPreview(GalaxyInfoModel model, Rect displayArea)
		{
			specifiedSectorsIsOverAnAllSector = false;
			if (specifiedSectorsSelectedSector == null)
			{
				// TODO: show all!
				OnSpecifiedSectorsShowAll(model, displayArea);
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

		void OnSpecifiedSectorsShowAll(
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
					SelectSpecifiedSector(sector);
				}

				specifiedSectorsIsOverAnAllSector = specifiedSectorsIsOverAnAllSector || selectCurrentArea.Contains(Event.current.mousePosition);

				EditorGUILayoutExtensions.PopColor();
			}

			if (specifiedSectorsShowTargets.Value) DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyTargetStyleSmall);
		}

		void OnSpecifiedSectorsPrimaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition)
		{
			switch (specifiedSectorsState)
			{
				case SpecifiedSectorsStates.Idle:
					specifiedSectorsSelectedSectorLast = specifiedSectorsSelectedSector;
					specifiedSectorsSelectedSector = OnSpecifiedSectorCreateNewSector();
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
							SelectSpecifiedSector(specifiedSectorsSelectedSectorLast);
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

		void OnSpecifiedSectorsSecondaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition)
		{
			switch (specifiedSectorsState)
			{
				case SpecifiedSectorsStates.Idle:
					SelectSpecifiedSector(null);
					return;
				case SpecifiedSectorsStates.SelectingSector:
					SelectSpecifiedSector(specifiedSectorsSelectedSectorLast);
					break;
				case SpecifiedSectorsStates.UpdatingSector:
					SelectSpecifiedSector(specifiedSectorsSelectedSector);
					break;
			}
			specifiedSectorsState = SpecifiedSectorsStates.Idle;
		}

		SectorModel OnSpecifiedSectorCreateNewSector()
		{
			var result = new SectorModel();
			result.Name.Value = Guid.NewGuid().ToString();
			return result;
		}

		void OnBeforeLoadSelectionSpecifiedSectors()
		{
			SelectSpecifiedSector(null);
		}

		void SelectSpecifiedSector(SectorModel sector, SpecifiedSectorsStates state = SpecifiedSectorsStates.Idle)
		{
			specifiedSectorsSelectedSector = sector;
			specifiedSectorsSelectedSectorName.Value = sector == null ? null : sector.Name.Value;
			if (state != SpecifiedSectorsStates.Unknown) specifiedSectorsState = state;
			GUIUtility.keyboardControl = 0;
		}
	}
}