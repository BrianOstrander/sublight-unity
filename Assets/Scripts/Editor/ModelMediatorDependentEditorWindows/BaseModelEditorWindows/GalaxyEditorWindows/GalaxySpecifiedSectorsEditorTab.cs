﻿using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.NumberDemon;

using LunraGames.SubLight.Models;

using LabelStates = LunraGames.SubLight.GalaxyLabelsEditorTab.LabelStates;

namespace LunraGames.SubLight
{
	public class GalaxySpecifiedSectorsEditorTab : ModelEditorTab<GalaxyEditorWindow, GalaxyInfoModel>
	{
		enum SpecifiedSectorsStates
		{
			Unknown = 0,
			Idle = 10,
			SelectingSector = 20,
			UpdatingSector = 30
		}

		EditorPrefsFloat specifiedSectorsScroll;
		EditorPrefsString specifiedSectorsSelectedSectorName;
		EditorPrefsInt specifiedSectorsPreviewSize;
		EditorPrefsBool specifiedSectorsPreviewMinimized;
		EditorPrefsInt specifiedSectorsSelectedPreview;
		EditorPrefsFloat specifiedSectorsSystemsScroll;
		EditorPrefsBool specifiedSectorsShowTargets;
		EditorPrefsBool specifiedSectorsShowDerivedValues;

		EditorPrefsInt specifiedSectorsSelectedSystemIndex;
		EditorPrefsFloat specifiedSectorsSystemDetailsScroll;

		SpecifiedSectorsStates specifiedSectorsState = SpecifiedSectorsStates.Idle;
		SectorModel specifiedSectorsSelectedSectorLast;
		SectorModel specifiedSectorsSelectedSector;
		SystemModel specifiedSectorSelectedSystem;
		bool specifiedSectorsIsOverAnAllSector;
		
		GalaxyLabelsEditorTab galaxyLabelTab;
		
		public GalaxySpecifiedSectorsEditorTab(
			GalaxyEditorWindow window,
			GalaxyLabelsEditorTab galaxyLabelTab
		) : base(window, "Specified Sectors", "Specified Sectors")
		{
			this.galaxyLabelTab = galaxyLabelTab;
			
			specifiedSectorsScroll = new EditorPrefsFloat(TabKeyPrefix + "Scroll");
			specifiedSectorsSelectedSectorName = new EditorPrefsString(TabKeyPrefix + "SelectedSectorName");
			specifiedSectorsPreviewSize = new EditorPrefsInt(TabKeyPrefix + "PreviewSize");
			specifiedSectorsPreviewMinimized = new EditorPrefsBool(TabKeyPrefix + "PreviewMinimized");
			specifiedSectorsSelectedPreview = new EditorPrefsInt(TabKeyPrefix + "SelectedPreview");
			specifiedSectorsSystemsScroll = new EditorPrefsFloat(TabKeyPrefix + "SystemsScroll");
			specifiedSectorsShowTargets = new EditorPrefsBool(TabKeyPrefix + "ShowTargets");
			specifiedSectorsShowDerivedValues = new EditorPrefsBool(TabKeyPrefix + "ShowDerivedValues");

			specifiedSectorsSelectedSystemIndex = new EditorPrefsInt(TabKeyPrefix + "SelectedSystemIndex");
			specifiedSectorsSystemDetailsScroll = new EditorPrefsFloat(TabKeyPrefix + "SystemDetailsScroll");
		}

		public override void Gui(GalaxyInfoModel model)
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

			var isDeleting = !Event.current.shift && Event.current.alt;

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical(GUILayout.Width(350f));
				{
					specifiedSectorsScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, specifiedSectorsScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
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
								EditorGUILayoutExtensions.PushColor(EditorUtilityExtensions.ColorFromIndex(sectorIndex).NewS(0.65f));
								{
									GUILayout.Label(specifiedSector.Name.Value);
								}
								EditorGUILayoutExtensions.PopColor();

								EditorGUILayoutExtensions.PushEnabled(!isSelectedSector);
								if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(42f)))
								{
									SpecifiedSectorsSelectSpecifiedSector(specifiedSector);
								}
								EditorGUILayoutExtensions.PopEnabled();

								if (isDeleting)
								{
									if (EditorGUILayoutExtensions.XButton(true))
									{
										if (specifiedSectorsSelectedSector == specifiedSector)
										{
											SpecifiedSectorsSelectSpecifiedSector(null);
										}
										model.RemoveSpecifiedSector(specifiedSector);
										Window.ModelSelectionModified = true;
									}
								}
							}
							GUILayout.EndHorizontal();
						}

					}
					GUILayout.EndScrollView();


					var listingSystems = specifiedSectorsSelectedSector != null;
					EditorGUILayoutExtensions.PushEnabled(listingSystems && specifiedSectorsState == SpecifiedSectorsStates.Idle);
					{
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label("Systems", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
							GUILayout.FlexibleSpace();
							EditorGUILayoutExtensions.PushColor(Color.gray);
							{
								GUILayout.Label("Hold 'alt' to delete", GUILayout.ExpandWidth(false));
							}
							EditorGUILayoutExtensions.PopColor();
							if (GUILayout.Button("Add System", EditorStyles.miniButton, GUILayout.Width(64f))) SpecifiedSectorsCreateSystem(model, specifiedSectorsSelectedSector);
						}
						GUILayout.EndHorizontal();

						if (listingSystems)
						{
							specifiedSectorsSystemsScroll.VerticalScroll = GUILayout.BeginScrollView(specifiedSectorsSystemsScroll.VerticalScroll, false, true, GUILayout.ExpandHeight(true));
							{
								int? deletedSystem = null;
								foreach (var system in specifiedSectorsSelectedSector.Systems.Value.OrderBy(s => s.Name.Value))
								{
									var isSelectedSystem = system.Index.Value == specifiedSectorsSelectedSystemIndex.Value;
									if (isSelectedSystem) EditorGUILayoutExtensions.PushColor(Color.blue.NewS(0.7f));
									GUILayout.BeginHorizontal(EditorStyles.helpBox);
									if (isSelectedSystem) EditorGUILayoutExtensions.PopColor();
									{
										GUILayout.Label(string.IsNullOrEmpty(system.Name) ? "< #" + system.Index.Value + " no name >" : system.Name.Value);
										EditorGUILayoutExtensions.PushEnabled(system.Index.Value != specifiedSectorsSelectedSystemIndex.Value);
										{
											var systemTag = string.Empty;
											if (system.PlayerBegin.Value) systemTag = "begin";
											else if (system.PlayerEnd.Value) systemTag = "end";

											if (!string.IsNullOrEmpty(systemTag))
											{
												EditorGUILayoutExtensions.PushColor(Color.gray);
												{
													GUILayout.Label(systemTag, GUILayout.ExpandWidth(false));
												}
												EditorGUILayoutExtensions.PopColor();
											}

											if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(42f)))
											{
												specifiedSectorSelectedSystem = system;
												specifiedSectorsSelectedSystemIndex.Value = system.Index.Value;
												EditorGUIExtensions.ResetControls();
											}
										}
										EditorGUILayoutExtensions.PopEnabled();

										if (isDeleting)
										{
											if (EditorGUILayoutExtensions.XButton(true))
											{
												deletedSystem = system.Index.Value;
												Window.ModelSelectionModified = true;
											}
										}
									}
									GUILayout.EndHorizontal();
								}

								if (deletedSystem.HasValue)
								{
									if (specifiedSectorsSelectedSystemIndex.Value == deletedSystem.Value)
									{
										specifiedSectorSelectedSystem = null;
										specifiedSectorsSelectedSystemIndex.Value = -1;

										specifiedSectorsSelectedSector.Systems.Value = specifiedSectorsSelectedSector.Systems.Value.Where(s => s.Index.Value != deletedSystem.Value).ToArray();
										specifiedSectorsSelectedSector.SystemCount.Value = specifiedSectorsSelectedSector.Systems.Value.Count();

										var newIndex = 0;
										foreach (var system in specifiedSectorsSelectedSector.Systems.Value.OrderBy(s => s.Index.Value))
										{
											system.Index.Value = newIndex;
											newIndex++;
										}
									}

								}
							}
							GUILayout.EndScrollView();
						}
						else
						{
							EditorGUILayout.HelpBox("Select a sector to see systems.", MessageType.Info);
						}
					}
					EditorGUILayoutExtensions.PopEnabled();

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

							EditorGUILayoutExtensions.PushEnabled(galaxyLabelTab.labelsLabelState != LabelStates.Idle);
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
								galaxyLabelTab.labelsLabelState = LabelStates.Idle;
							}
							EditorGUILayoutExtensions.PopColor();
							EditorGUILayoutExtensions.PopEnabled();

						}
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
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

							if (specifiedSectorSelectedSystem == null)
							{
								EditorGUILayout.HelpBox("Select a system to edit it.", MessageType.Info);
							}
							else
							{
								var setToPlayerBegin = false;
								var setToPlayerEnd = false;

								SpecifiedSectorsDrawSystem(
									model,
									specifiedSectorsSelectedSector,
									specifiedSectorSelectedSystem,
									out setToPlayerBegin,
									out setToPlayerEnd
								);

								if (setToPlayerBegin || setToPlayerEnd)
								{
									foreach (var otherSector in model.GetSpecifiedSectors())
									{
										foreach (var otherSystem in otherSector.Systems.Value)
										{
											if (setToPlayerBegin && otherSystem != specifiedSectorSelectedSystem) otherSystem.PlayerBegin.Value = false;
											if (setToPlayerEnd && otherSystem != specifiedSectorSelectedSystem) otherSystem.PlayerEnd.Value = false;
										}
									}
								}
							}
						}
						EditorGUILayoutExtensions.PopEnabled();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			Window.DrawPreviews(
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
			// Lol what is this?
		}

		void SpecifiedSectorsDrawOnPreview(GalaxyInfoModel model, Rect displayArea)
		{
			specifiedSectorsIsOverAnAllSector = false;
			if (specifiedSectorsSelectedSector == null)
			{
				// TODO: show all! <- is this done???
				SpecifiedSectorsShowAll(model, displayArea);
				return;
			}

			var normalizedSectorPosition = UniversePosition.NormalizedSector(specifiedSectorsSelectedSector.Position.Value, model.GalaxySize);
			var positionInWindow = Window.NormalToWindow(normalizedSectorPosition, displayArea, out var positionInPreview);

			var positionColor = specifiedSectorsState == SpecifiedSectorsStates.UpdatingSector ? Color.cyan.NewS(0.25f) : Color.cyan;

			EditorGUILayoutExtensions.PushColor(positionInPreview ? positionColor : positionColor.NewA(positionColor.a * 0.5f));
			{
				GUI.Box(
					Window.CenteredScreen(positionInWindow, new Vector2(16f, 16f)),
					new GUIContent(string.Empty, "Sector Position"),
					SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetStyle
				);

				switch (specifiedSectorsSelectedSector.SpecifiedPlacement.Value)
				{
					case SectorModel.SpecifiedPlacements.Unknown:
					case SectorModel.SpecifiedPlacements.Position:
						break;
					case SectorModel.SpecifiedPlacements.PositionList:
						for (var i = 0; i < specifiedSectorsSelectedSector.PositionList.Value.Length; i++)
						{
							if (i == 0) continue;

							var currentNormalizedSectorPosition = UniversePosition.NormalizedSector(specifiedSectorsSelectedSector.PositionList.Value[i], model.GalaxySize);
							var currentPositionInWindow = Window.NormalToWindow(currentNormalizedSectorPosition, displayArea, out var currentPositionInPreview);

							GUI.Box(
								Window.CenteredScreen(currentPositionInWindow, new Vector2(16f, 16f)),
								new GUIContent(string.Empty, "Sector Position"),
								SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetMinorStyle
							);
						}
						break;
					default:
						Debug.LogError("Unrecognized Placement: " + specifiedSectorsSelectedSector.SpecifiedPlacement.Value);
						break;
				}

			}
			EditorGUILayoutExtensions.PopColor();

			if (specifiedSectorsShowTargets.Value) Window.DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetStyleSmall);
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

				var positionInWindow = Window.NormalToWindow(normalizedSectorPosition, displayArea, out positionInPreview);

				var selectCurrentArea = Window.CenteredScreen(positionInWindow, new Vector2(16f, 16f));

				if (GUI.Button(selectCurrentArea, new GUIContent(string.Empty, sector.Name.Value), SubLightEditorConfig.Instance.GalaxyEditorSpecifiedSectorTargetStyle))
				{
					SpecifiedSectorsSelectSpecifiedSector(sector);
				}

				specifiedSectorsIsOverAnAllSector = specifiedSectorsIsOverAnAllSector || selectCurrentArea.Contains(Event.current.mousePosition);

				EditorGUILayoutExtensions.PopColor();
			}

			if (specifiedSectorsShowTargets.Value) Window.DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetStyleSmall);
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
							Window.ModelSelectionModified = true;
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
					Window.ModelSelectionModified = true;
					specifiedSectorsState = SpecifiedSectorsStates.Idle;
					break;
				case SpecifiedSectorsStates.UpdatingSector:
					specifiedSectorsSelectedSector.Position.Value = UniversePosition.Lerp(clickPosition, UniversePosition.Zero, model.GalaxySize).LocalZero;
					Window.ModelSelectionModified = true;
					specifiedSectorsState = SpecifiedSectorsStates.Idle;
					break;
				default:
					Debug.LogError("Unrecognized state " + galaxyLabelTab.labelsLabelState);
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

		public override void BeforeLoadSelection()
		{
			SpecifiedSectorsSelectSpecifiedSector(null);
		}

		void SpecifiedSectorsSelectSpecifiedSector(SectorModel sector)
		{
			var selectedSectorChanged = sector == null || sector.Name.Value != specifiedSectorsSelectedSectorName.Value;

			specifiedSectorsSelectedSector = sector;
			specifiedSectorsSelectedSectorName.Value = sector == null ? null : sector.Name.Value;
			specifiedSectorsState = SpecifiedSectorsStates.Idle;

			if (selectedSectorChanged)
			{
				specifiedSectorsSelectedSystemIndex.Value = -1;
				specifiedSectorSelectedSystem = null;
			}
			else
			{
				specifiedSectorSelectedSystem = sector.Systems.Value.FirstOrDefault(s => s.Index.Value == specifiedSectorsSelectedSystemIndex.Value);
				specifiedSectorsSelectedSystemIndex.Value = specifiedSectorSelectedSystem == null ? -1 : specifiedSectorSelectedSystem.Index.Value;
			}

			EditorGUIExtensions.ResetControls();
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

				var previousPlacement = sector.SpecifiedPlacement.Value;

				GUILayout.BeginHorizontal();
				{
					sector.SpecifiedPlacement.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
						new GUIContent("Placement"),
						"- Select Placement -",
						sector.SpecifiedPlacement.Value,
						Color.red,
						guiOptions: new GUILayoutOption[] { GUILayout.MaxWidth(300f) }
					);

					switch (previousPlacement)
					{
						case SectorModel.SpecifiedPlacements.PositionList:
							if (GUILayout.Button("Append Position", GUILayout.ExpandWidth(false)))
							{
								sector.PositionList.Value = sector.PositionList.Value.Append(sector.PositionList.Value[0]).ToArray();
							}

							GUILayout.FlexibleSpace();
							break;
					}
				}
				GUILayout.EndHorizontal();

				if (previousPlacement != sector.SpecifiedPlacement.Value)
				{
					switch (sector.SpecifiedPlacement.Value)
					{
						case SectorModel.SpecifiedPlacements.Unknown: break;
						case SectorModel.SpecifiedPlacements.Position:
							sector.PositionList.Value = new UniversePosition[0];
							break;
						case SectorModel.SpecifiedPlacements.PositionList:
							sector.PositionList.Value = new UniversePosition[] { sector.Position.Value };
							break;
						default:
							Debug.LogError("Unrecognized Placement: " + sector.SpecifiedPlacement.Value);
							break;
					}
				}

				switch (sector.SpecifiedPlacement.Value)
				{
					case SectorModel.SpecifiedPlacements.Position:
						sector.Position.Value = SpecifiedSectorsSectorPosition(
							sector.Position.Value,
							"Sector",
							out bool unusedDeletion,
							true
						);
						break;
					case SectorModel.SpecifiedPlacements.PositionList:
						var deletionEnabled = 1 < sector.PositionList.Value.Length;
						int? positionListDeletion = null;

						var positionList = sector.PositionList.Value;

						for (var i = 0; i < sector.PositionList.Value.Length; i++)
						{
							positionList[i] = SpecifiedSectorsSectorPosition(
								positionList[i],
								i == 0 ? "Default Sector" : "Sector",
								out bool positionDeleted,
								i == 0,
								deletionEnabled
							);

							if (positionDeleted)
							{
								positionListDeletion = i;
							}
						}

						if (positionListDeletion.HasValue)
						{
							var deletionResult = positionList.ToList();
							deletionResult.RemoveAt(positionListDeletion.Value);
							positionList = deletionResult.ToArray();
						}

						sector.PositionList.Value = positionList;
						sector.Position.Value = positionList[0];

						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized Placement: " + sector.SpecifiedPlacement.Value, MessageType.Error);
						break;
				}
			}
			EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
		}

		UniversePosition SpecifiedSectorsSectorPosition(
			UniversePosition sectorPosition,
			string fieldName,
			out bool deleted,
			bool updatable = false,
			bool deletable = false
		)
		{
			const float UpdateHeight = 36f;
			const float DeletionWidth = 18f;

			deleted = false;

			GUILayout.BeginHorizontal(EditorStyles.helpBox);
			{
				GUILayout.BeginVertical();
				{
					sectorPosition = EditorGUILayoutUniversePosition.FieldSector(fieldName, sectorPosition).LocalZero;
				}
				GUILayout.EndVertical();

				EditorGUILayoutExtensions.PushBackgroundColor(Color.cyan);
				{
					var updateStyle = EditorStyles.miniButtonLeft;
					var updateWidth = 100f;

					if (!deletable)
					{
						updateStyle = EditorStyles.miniButton;
						updateWidth += DeletionWidth;
					}

					EditorGUILayoutExtensions.PushEnabled(updatable);
					{
						if (GUILayout.Button("Update Sector", updateStyle, GUILayout.Width(updateWidth), GUILayout.Height(UpdateHeight)))
						{
							specifiedSectorsState = SpecifiedSectorsStates.UpdatingSector;
						}
					}
					EditorGUILayoutExtensions.PopEnabled();

					if (deletable)
					{
						EditorGUILayoutExtensions.PushBackgroundColor(Color.red);
						{
							if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(DeletionWidth), GUILayout.Height(UpdateHeight))) deleted = true;
						}
						EditorGUILayoutExtensions.PopBackgroundColor();
					}
				}
				EditorGUILayoutExtensions.PopBackgroundColor();
			}
			GUILayout.EndHorizontal();

			return sectorPosition;
		}

		void SpecifiedSectorsDrawSystem(
			GalaxyInfoModel model,
			SectorModel sector,
			SystemModel system,
			out bool setToPlayerBegin,
			out bool setToPlayerEnd
		)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				specifiedSectorsSystemDetailsScroll.VerticalScroll = GUILayout.BeginScrollView(specifiedSectorsSystemDetailsScroll.VerticalScroll, false, true, GUILayout.ExpandHeight(true));
				{
					GUILayout.Label("#" + system.Index.Value + " | " + (string.IsNullOrEmpty(system.Name.Value) ? "< Null or Empty Name >" : system.Name.Value), EditorStyles.boldLabel);

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

					var navigationSelectEncounter = system.SpecifiedEncounters.Value.FirstOrDefault(s => s.Trigger == EncounterTriggers.NavigationSelect).EncounterId;
					var transitCompleteEncounter = system.SpecifiedEncounters.Value.FirstOrDefault(s => s.Trigger == EncounterTriggers.TransitComplete).EncounterId;

					GUILayout.Label("Encounters", EditorStyles.boldLabel);
					navigationSelectEncounter = EditorGUILayout.TextField("Navigation Select", navigationSelectEncounter);
					transitCompleteEncounter = EditorGUILayout.TextField("Transit Complete", transitCompleteEncounter);

					system.SpecifiedEncounters.Value = new SpecifiedEncounterEntry[]
					{
						new SpecifiedEncounterEntry { EncounterId = navigationSelectEncounter, Trigger = EncounterTriggers.NavigationSelect },
						new SpecifiedEncounterEntry { EncounterId = transitCompleteEncounter, Trigger = EncounterTriggers.TransitComplete },
					};

					GUILayout.Label("Key Values", EditorStyles.boldLabel);

					var keyValuesExceptions = new IKeyDefinition[]
					{
						KeyDefines.CelestialSystem.Index,
						KeyDefines.CelestialSystem.Seed,
						KeyDefines.CelestialSystem.Name,
						KeyDefines.CelestialSystem.Visited,
						KeyDefines.CelestialSystem.ClassificationPrimary,
						KeyDefines.CelestialSystem.ClassificationSecondary,
						KeyDefines.CelestialSystem.IconColor.Hue,
						KeyDefines.CelestialSystem.IconColor.Saturation,
						KeyDefines.CelestialSystem.IconColor.Value,
						KeyDefines.CelestialSystem.IconColor.Alpha,
						KeyDefines.CelestialSystem.IconScale,
						KeyDefines.CelestialSystem.PlayerBegin,
						KeyDefines.CelestialSystem.PlayerEnd,
						KeyDefines.CelestialSystem.Specified
					}.Select(k => k.Key);

					foreach (var definedKeyValue in KeyDefines.CelestialSystem.All.Where(k => !keyValuesExceptions.Contains(k.Key)))
					{
						SpecifiedSectorsDrawSystemKeyValue(
							system.KeyValues,
							definedKeyValue
						);
					}
				}
				GUILayout.EndScrollView();
			}
			EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
		}

		void SpecifiedSectorsDrawSystemKeyValue(
			KeyValueListModel keyValues,
			IKeyDefinition definition
		)
		{
			var content = new GUIContent(ObjectNames.NicifyVariableName(definition.Key.Replace('_', ' ')), definition.Notes);
			switch (definition.ValueType)
			{
				case KeyValueTypes.Boolean:
					SpecifiedSectorsDrawSystemKeyValue(
						content,
						keyValues,
						definition as KeyDefinitions.Boolean
					);
					break;
				case KeyValueTypes.Integer:
					SpecifiedSectorsDrawSystemKeyValue(
						content,
						keyValues,
						definition as KeyDefinitions.Integer
					);
					break;
				case KeyValueTypes.String:
					SpecifiedSectorsDrawSystemKeyValue(
						content,
						keyValues,
						definition as KeyDefinitions.String
					);
					break;
				case KeyValueTypes.Float:
					SpecifiedSectorsDrawSystemKeyValue(
						content,
						keyValues,
						definition as KeyDefinitions.Float
					);
					break;
				case KeyValueTypes.Enumeration:
					SpecifiedSectorsDrawSystemKeyValue(
						content,
						keyValues,
						definition as KeyDefinitions.IEnumeration
					);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized KeyValueType: " + definition.ValueType, MessageType.Error);
					break;
			}
		}

		void SpecifiedSectorsDrawSystemKeyValue(
			GUIContent content,
			KeyValueListModel keyValues,
			KeyDefinitions.Boolean definition
		)
		{
			keyValues.SetBoolean(
				definition.Key,
				EditorGUILayout.Toggle(content, keyValues.GetBoolean(definition.Key))
			);
		}

		void SpecifiedSectorsDrawSystemKeyValue(
			GUIContent content,
			KeyValueListModel keyValues,
			KeyDefinitions.Integer definition
		)
		{
			keyValues.SetInteger(
				definition.Key,
				EditorGUILayout.IntField(content, keyValues.GetInteger(definition.Key))
			);
		}

		void SpecifiedSectorsDrawSystemKeyValue(
			GUIContent content,
			KeyValueListModel keyValues,
			KeyDefinitions.String definition
		)
		{
			keyValues.SetString(
				definition.Key,
				EditorGUILayoutExtensions.TextDynamic(content, keyValues.GetString(definition.Key))
			);
		}

		void SpecifiedSectorsDrawSystemKeyValue(
			GUIContent content,
			KeyValueListModel keyValues,
			KeyDefinitions.Float definition
		)
		{
			keyValues.SetFloat(
				definition.Key,
				EditorGUILayout.FloatField(content, keyValues.GetFloat(definition.Key))
			);
		}

		void SpecifiedSectorsDrawSystemKeyValue(
			GUIContent content,
			KeyValueListModel keyValues,
			KeyDefinitions.IEnumeration definition
		)
		{
			keyValues.SetInteger(
				definition.Key,
				EditorGUILayoutExtensions.IntegerEnumPopup(content, keyValues.GetInteger(definition.Key), definition.EnumerationType)
			);
		}

		void SpecifiedSectorsCreateSystem(GalaxyInfoModel model, SectorModel sector)
		{
			var currSystems = sector.Systems.Value.OrderBy(s => s.Index.Value);
			var result = new SystemModel();

			result.Index.Value = currSystems.None() ? 0 : currSystems.Last().Index.Value + 1;
			result.Seed.Value = DemonUtility.NextInteger;
			result.Specified.Value = true;
			result.Position.Value = sector.Position.Value;
			result.IconColor.Value = Color.white;

			sector.Systems.Value = sector.Systems.Value.Append(result).ToArray();
			sector.SystemCount.Value = sector.Systems.Value.Count();

			specifiedSectorSelectedSystem = result;
			specifiedSectorsSelectedSystemIndex.Value = result.Index.Value;
			EditorGUIExtensions.ResetControls();

			Window.ModelSelectionModified = true;
		}
	}
}