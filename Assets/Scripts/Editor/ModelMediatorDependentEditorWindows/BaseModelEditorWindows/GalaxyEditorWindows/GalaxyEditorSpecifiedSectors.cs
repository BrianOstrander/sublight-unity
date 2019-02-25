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

		void SpecifiedSectorsConstruct()
		{
			var currPrefix = KeyPrefix + "SpecifiedSectors";

			specifiedSectorsScroll = new EditorPrefsFloat(currPrefix + "Scroll");
			specifiedSectorsSelectedSectorName = new EditorPrefsString(currPrefix + "SelectedSectorName");
			specifiedSectorsPreviewSize = new EditorPrefsInt(currPrefix + "PreviewSize");
			specifiedSectorsPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");
			specifiedSectorsSelectedPreview = new EditorPrefsInt(currPrefix + "SelectedPreview");
			specifiedSectorsSystemsScroll = new EditorPrefsFloat(currPrefix + "SystemsScroll");
			specifiedSectorsShowTargets = new EditorPrefsBool(currPrefix + "ShowTargets");
			specifiedSectorsShowDerivedValues = new EditorPrefsBool(currPrefix + "ShowDerivedValues");

			specifiedSectorsSelectedSystemIndex = new EditorPrefsInt(currPrefix + "SelectedSystemIndex");
			specifiedSectorsSystemDetailsScroll = new EditorPrefsFloat(currPrefix + "SystemDetailsScroll");

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
										ModelSelectionModified = true;
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
												GUIUtility.keyboardControl = 0;
											}
										}
										EditorGUILayoutExtensions.PopEnabled();

										if (isDeleting)
										{
											if (EditorGUILayoutExtensions.XButton(true))
											{
												deletedSystem = system.Index.Value;
												ModelSelectionModified = true;
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
			// Lol what is this?
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
				GUI.Box(CenteredScreen(positionInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Sector Position"), SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			if (specifiedSectorsShowTargets.Value) DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetStyleSmall);
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

				if (GUI.Button(selectCurrentArea, new GUIContent(string.Empty, sector.Name.Value), SubLightEditorConfig.Instance.GalaxyEditorSpecifiedSectorTargetStyle))
				{
					SpecifiedSectorsSelectSpecifiedSector(sector);
				}

				specifiedSectorsIsOverAnAllSector = specifiedSectorsIsOverAnAllSector || selectCurrentArea.Contains(Event.current.mousePosition);

				EditorGUILayoutExtensions.PopColor();
			}

			if (specifiedSectorsShowTargets.Value) DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyEditorGalaxyTargetStyleSmall);
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
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
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

					foreach (var definedKeyValue in KeyDefines.CelestialSystem.All)
					{
						SpecifiedSectorsDrawSystemKeyValue(
							system.KeyValues,
							definedKeyValue
						);
					}
				}
				GUILayout.EndScrollView();
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
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
			GUIUtility.keyboardControl = 0;

			ModelSelectionModified = true;
		}
	}
}