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
		const int PreviewMinSize = 128;
		const int PreviewMaxSize = 2048;
		const int SelectedLabelCurveSampling = 10;
		const int AllLabelsCurveSamplingQuadrant = 5;
		const int AllLabelsCurveSamplingGalactic = 15;

		enum HomeStates
		{
			Unknown = 0,
			Browsing = 10,
			Selected = 20
		}

		enum LabelStates
		{
			Unknown = 0,
			Idle = 10,
			SelectingBegin = 20,
			SelectingEnd = 30,
			UpdatingBegin = 40,
			UpdatingEnd = 50
		}

		enum LabelColorCodes
		{
			Unknown = 0,
			Label = 10,
			Group = 20
		}

		EditorPrefsBool homeAlwaysAllowSaving;
		EditorPrefsFloat homeLeftBarScroll;
		EditorPrefsString homeSelectedPath;
		EditorPrefsEnum<HomeStates> homeState;
		DevPrefsInt homeSelectedToolbar;

		DevPrefsInt homeGeneralPreviewSize;
		EditorPrefsFloat homeGeneralPreviewBarScroll;

		DevPrefsInt homeTargetsSelectedPreview;

		EditorPrefsString homeLabelsSelectedLabelId;
		DevPrefsInt homeLabelsSelectedScale;
		EditorPrefsFloat homeLabelsListScroll;
		EditorPrefsFloat homeLabelDetailsScroll;
		EditorPrefsEnum<LabelColorCodes> homLabelColorCodeBy;

		EditorPrefsFloat homeGenerationBarScroll;

		bool lastIsPlayingOrWillChangePlaymode;
		int frameDelayRemaining;

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus modelListStatus;
		SaveModel[] modelList = new SaveModel[0];

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus selectedStatus;
		GalaxyInfoModel selected;
		bool selectedModified;

		Rect lastPreviewRect = Rect.zero;

		LabelStates labelState = LabelStates.Idle;
		GalaxyLabelModel lastSelectedLabel;
		GalaxyLabelModel selectedLabel;
		bool isOverAnAllLabel;

		void OnHomeConstruct()
		{
			homeAlwaysAllowSaving = new EditorPrefsBool(KeyPrefix + "AlwaysAllowSaving");
			homeLeftBarScroll = new EditorPrefsFloat(KeyPrefix + "LeftBarScroll");
			homeSelectedPath = new EditorPrefsString(KeyPrefix + "HomeSelected");
			homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);
			homeSelectedToolbar = new DevPrefsInt(KeyPrefix + "HomeSelectedState");

			homeGeneralPreviewSize = new DevPrefsInt(KeyPrefix + "GeneralPreviewSize");
			homeGeneralPreviewBarScroll = new EditorPrefsFloat(KeyPrefix + "GeneralPreviewBarScroll");

			homeTargetsSelectedPreview = new DevPrefsInt(KeyPrefix + "TargetsSelectedPreview");

			homeLabelsSelectedLabelId = new EditorPrefsString(KeyPrefix + "LabelsSelectedLabelId");
			homeLabelsSelectedScale = new DevPrefsInt(KeyPrefix + "LabelsSelectedScale");
			homeLabelsListScroll = new EditorPrefsFloat(KeyPrefix + "LabelsListScroll");
			homeLabelDetailsScroll = new EditorPrefsFloat(KeyPrefix + "LabelDetailsScroll");
			homLabelColorCodeBy = new EditorPrefsEnum<LabelColorCodes>(KeyPrefix + "LabelColorCodeBy");

			homeGenerationBarScroll = new EditorPrefsFloat(KeyPrefix + "HomeGenerationBarScroll");

			Enable += OnHomeEnable;
			Disable += OnHomeDisable;
			Save += SaveSelected;
			EditorUpdate += OnUpdate;
		}

		void OnHomeEnable()
		{
			modelListStatus = RequestStatus.Cancel;
			selectedStatus = RequestStatus.Cancel;

			EditorApplication.modifierKeysChanged += OnHomeModifierKeysChanged;
		}

		void OnHomeDisable()
		{
			EditorApplication.modifierKeysChanged -= OnHomeModifierKeysChanged;
		}

		void OnHomeModifierKeysChanged()
		{
			Repaint();
		}

		void OnHome()
		{
			OnUpdate();
			if (lastIsPlayingOrWillChangePlaymode)
			{
				EditorGUILayout.HelpBox("Cannot edit in playmode.", MessageType.Info);
				return;
			}
			if (0 < frameDelayRemaining) return; // Adding a helpbox messes with this...

			OnCheckStatus();
			switch (homeState.Value)
			{
				case HomeStates.Browsing:
					OnHomeBrowsing();
					break;
				case HomeStates.Selected:
					OnHomeSelected();
					break;
				default:
					OnHomeUnknown();
					break;
			}
		}

		void OnUpdate()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlayingOrWillChangePlaymode != lastIsPlayingOrWillChangePlaymode)
			{
				modelListStatus = RequestStatus.Cancel;
				modelList = new SaveModel[0];
				selectedStatus = RequestStatus.Cancel;
				selected = null;
				frameDelayRemaining = 8;
			}
			lastIsPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;

			if (!lastIsPlayingOrWillChangePlaymode && 0 < frameDelayRemaining)
			{
				frameDelayRemaining--;
				Repaint();
			}
		}

		void OnCheckStatus()
		{
			switch (modelListStatus)
			{
				case RequestStatus.Cancel:
					LoadList();
					return;
				case RequestStatus.Unknown:
					return;
			}
			if (string.IsNullOrEmpty(homeSelectedPath.Value)) return;

			switch (selectedStatus)
			{
				case RequestStatus.Cancel:
					LoadSelected(modelList.FirstOrDefault(m => m.Path == homeSelectedPath.Value));
					return;
			}
		}

		void OnHomeBrowsing()
		{
			GUILayout.BeginHorizontal();
			{
				OnHomeLeftPane();
				OnHomeRightPane();
			}
			GUILayout.EndHorizontal();
		}

		void OnHomeSelected()
		{
			GUILayout.BeginHorizontal();
			{
				OnHomeLeftPane();
				OnHomeRightPane(selected);
			}
			GUILayout.EndHorizontal();
		}

		void OnHomeUnknown()
		{
			GUILayout.BeginVertical();
			{
				GUILayout.Label("Unknown state: " + homeState.Value);
				if (GUILayout.Button("Reset")) homeState.Value = HomeStates.Browsing;
			}
			GUILayout.EndVertical();
		}

		void OnHomeLeftPane()
		{
			GUILayout.BeginVertical(GUILayout.Width(300f));
			{
				GUILayout.Space(4f);
				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("New")) NewModel();
					if (GUILayout.Button("Refresh")) LoadList();
				}
				GUILayout.EndHorizontal();

				homeLeftBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLeftBarScroll), false, true).y;
				{
					var isAlternate = false;
					foreach (var model in modelList) OnDrawModel(model, ref isAlternate);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		void OnHomeRightPane(GalaxyInfoModel model = null)
		{
			if (model == null)
			{
				GUILayout.BeginVertical();
				{
					EditorGUILayout.HelpBox("Select a galaxy to edit.", MessageType.Info);
				}
				GUILayout.EndVertical();
				return;
			}

			string[] names =
			{
				"General",
				"Targets",
				"Labels",
				"Generation"
			};

			GUILayout.BeginVertical();
			{
				var name = string.IsNullOrEmpty(model.Name) ? "< No Name > " : model.Name;
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Editing: " + name);

					GUILayout.Label("Always Allow Saving", GUILayout.ExpandWidth(false));
					homeAlwaysAllowSaving.Value = EditorGUILayout.Toggle(homeAlwaysAllowSaving.Value, GUILayout.Width(14f));
					EditorGUILayoutExtensions.PushEnabled(homeAlwaysAllowSaving.Value || selectedModified);
					{
						if (GUILayout.Button("Save", GUILayout.Width(64f))) Save();
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				GUILayout.EndHorizontal();
				homeSelectedToolbar.Value = GUILayout.Toolbar(Mathf.Min(homeSelectedToolbar, names.Length - 1), names);

				switch (homeSelectedToolbar.Value)
				{
					case 0:
						OnHomeSelectedGeneral(model);
						break;
					case 1:
						OnHomeSelectedTargets(model);
						break;
					case 2:
						OnHomeSelectedLabels(model);
						break;
					case 3:
						OnHomeSelectedGeneration(model);
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized index", MessageType.Error);
						break;
				}
			}
			GUILayout.EndVertical();
		}

		void OnHomeSelectedGeneral(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				model.IsPlayable.Value = EditorGUILayout.Toggle(new GUIContent("Is Playable", "Can the player start a game in this galaxy?"), model.IsPlayable.Value);

				model.GalaxyId.Value = model.SetMetaKey(MetaKeyConstants.GalaxyInfo.GalaxyId, EditorGUILayout.TextField("Galaxy Id", model.GalaxyId.Value));

				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;

				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The internal description for notes and production purposes."), model.Description.Value, leftOffset: false);

				model.EncyclopediaEntryId.Value = EditorGUILayout.TextField(new GUIContent("Encyclopedia Entry Id", "The encyclopedia entry opened when viewing the details of this galaxy."), model.EncyclopediaEntryId.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedModified);

			GUILayout.FlexibleSpace();

			homeGeneralPreviewBarScroll.Value = GUILayout.BeginScrollView(new Vector2(homeGeneralPreviewBarScroll, 0f), true, false, GUILayout.MinHeight(homeGeneralPreviewSize + 46f)).x;
			{
				GUILayout.BeginHorizontal();
				{
					var biggest = 0;
					foreach (var kv in model.Textures)
					{
						if (kv.Value == null) continue;
						biggest = Mathf.Max(biggest, Mathf.Max(kv.Value.width, kv.Value.height));
					}

					foreach (var kv in model.Textures)
					{
						if (kv.Value == null) continue;
						GUILayout.BeginVertical();
						{
							var isSmallerThanMax = Mathf.Min(kv.Value.width, kv.Value.height) < biggest;

							if (isSmallerThanMax) EditorGUILayoutExtensions.PushColor(Color.red);
							GUILayout.Label(kv.Key+" | "+kv.Value.width+" x "+kv.Value.height, EditorStyles.boldLabel);
							if (isSmallerThanMax) EditorGUILayoutExtensions.PopColor();

							if (GUILayout.Button(new GUIContent(kv.Value), GUILayout.MaxWidth(homeGeneralPreviewSize), GUILayout.MaxHeight(homeGeneralPreviewSize)))
							{
								var textureWithExtension = kv.Key + ".png";
								var texturePath = Path.Combine(model.IsInternal ? model.InternalSiblingDirectory : model.SiblingDirectory, textureWithExtension);
								EditorUtility.FocusProjectWindow();
								Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
							}
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}

		void OnHomeSelectedTargets(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.GalaxyRadius = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Galaxy Radius", "Galaxy radius in sectors"), model.GalaxyRadius, GUILayout.Width(250f)));
					GUILayout.Label("( " + UniversePosition.ToLightYearDistance(model.GalaxyRadius).ToString("N0")+" Light Years )");
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				model.ClusterOriginNormal.Value = EditorGUILayout.Vector3Field("Cluster Origin", model.ClusterOriginNormal);
				EditorGUILayoutExtensions.PushBackgroundColor(Color.yellow);
				model.GalaxyOriginNormal.Value = EditorGUILayout.Vector3Field("Galaxy Origin", model.GalaxyOriginNormal);
				EditorGUILayoutExtensions.PopBackgroundColor();
				EditorGUILayoutExtensions.PushBackgroundColor(Color.green);
				model.PlayerStartNormal.Value = EditorGUILayout.Vector3Field("Player Start", model.PlayerStartNormal);
				EditorGUILayoutExtensions.PopBackgroundColor();
				EditorGUILayoutExtensions.PushBackgroundColor(Color.red);
				model.GameEndNormal.Value = EditorGUILayout.Vector3Field("Game End", model.GameEndNormal);
				EditorGUILayoutExtensions.PopBackgroundColor();
				model.UniverseNormal.Value = EditorGUILayout.Vector3Field(new GUIContent("Universe Normal", "The up direction of this galaxy within the universe."), model.UniverseNormal.Value);
				model.AlertHeightMultiplier.Value = EditorGUILayout.FloatField(new GUIContent("Alert Height Multiplier", "The additional offset of any alerts on this galaxy."), model.AlertHeightMultiplier.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedModified);

			GUILayout.FlexibleSpace();

			string[] names =
			{
				"BodyMap",
				"Details",
				"Full Preview"
			};

			homeTargetsSelectedPreview.Value = GUILayout.Toolbar(Mathf.Min(homeTargetsSelectedPreview, names.Length - 1), names);

			GUILayout.BeginHorizontal();
			{
				OnPreviewSizeSlider(homeGeneralPreviewSize);
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			var previewTexture = Texture2D.blackTexture;

			switch (homeTargetsSelectedPreview.Value)
			{
				case 0:
					previewTexture = model.BodyMap;
					break;
				case 1:
					previewTexture = model.Details;
					break;
				case 2:
					previewTexture = model.FullPreview;
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized index", MessageType.Error);
					break;
			}

			var displayArea = DisplayPreview(
				previewTexture,
				homeGeneralPreviewSize,
				clickNormal =>
				{
					OptionDialogPopup.Show(
						"Set Target",
						new OptionDialogPopup.Entry[]
						{
							OptionDialogPopup.Entry.Create(
								"Galaxy Origin",
								() => { model.GalaxyOriginNormal.Value = clickNormal; selectedModified = true; },
								color: Color.yellow
							),
							OptionDialogPopup.Entry.Create(
								"Player Start",
								() => { model.PlayerStartNormal.Value = clickNormal; selectedModified = true; },
								color: Color.green
							),
							OptionDialogPopup.Entry.Create(
								"Game End",
								() => { model.GameEndNormal.Value = clickNormal; selectedModified = true; },
								color: Color.red
							)
						},
						description: "Select the following position to assign the value of ( " + clickNormal.x + " , " + clickNormal.z + " ) to."
					);
				}
			);

			var galacticOriginInPreview = true;
			var playerStartInPreview = true;
			var gameEndInPreview = true;

			var galacticOriginInWindow = NormalToWindow(model.GalaxyOriginNormal, displayArea, out galacticOriginInPreview);
			var playerStartInWindow = NormalToWindow(model.PlayerStartNormal, displayArea, out playerStartInPreview);
			var gameEndInWindow = NormalToWindow(model.GameEndNormal, displayArea, out gameEndInPreview);

			EditorGUILayoutExtensions.PushColor(galacticOriginInPreview ? Color.yellow : Color.yellow.NewA(0.5f));
			{
				GUI.Box(CenteredScreen(galacticOriginInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Galactic Origin"), SubLightEditorConfig.Instance.GalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			EditorGUILayoutExtensions.PushColor(playerStartInPreview ? Color.green : Color.green.NewA(0.5f));
			{
				GUI.Box(CenteredScreen(playerStartInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Player Start"), SubLightEditorConfig.Instance.GalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			EditorGUILayoutExtensions.PushColor(gameEndInPreview ? Color.red : Color.red.NewA(0.5f));
			{
				GUI.Box(CenteredScreen(gameEndInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Game End"), SubLightEditorConfig.Instance.GalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();
		}

		void OnHomeSelectedLabels(GalaxyInfoModel model)
		{
			if (string.IsNullOrEmpty(homeLabelsSelectedLabelId.Value))
			{
				if (selectedLabel != null)
				{
					SelectLabel(null);
				}
			}
			else if (selectedLabel == null)
			{
				SelectLabel(model.GetLabel(homeLabelsSelectedLabelId.Value));
			}

			UniverseScales[] scales =
			{
				UniverseScales.Quadrant,
				UniverseScales.Galactic
			};

			var scaleNames = scales.Select(s => s.ToString()).ToArray();
			var selectedScale = scales[homeLabelsSelectedScale.Value];

			if (selectedLabel != null)
			{
				if (selectedLabel.Scale.Value != selectedScale)
				{
					SelectLabel(null);
				}
			}

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical(GUILayout.Width(350f));
				{
					homeLabelsSelectedScale.Value = GUILayout.Toolbar(Mathf.Min(homeLabelsSelectedScale, scaleNames.Length - 1), scaleNames);
					selectedScale = scales[homeLabelsSelectedScale.Value];

					homeLabelsListScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLabelsListScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
					{
						var labelIndex = -1;
						foreach (var label in model.GetLabels(selectedScale))
						{
							labelIndex++;

							var isSelectedLabel = label.LabelId.Value == homeLabelsSelectedLabelId.Value;
							if (isSelectedLabel) EditorGUILayoutExtensions.PushColor(Color.blue.NewS(0.7f));
							GUILayout.BeginVertical(EditorStyles.helpBox);
							if (isSelectedLabel) EditorGUILayoutExtensions.PopColor();
							{
								GUILayout.BeginHorizontal();
								{
									EditorGUILayoutExtensions.PushColor(EditorUtilityExtensions.ColorFromIndex(labelIndex));
									GUILayout.BeginHorizontal(EditorStyles.helpBox);
									EditorGUILayoutExtensions.PopColor();
									{
										GUILayout.Label(label.Name.Value, EditorStyles.boldLabel);
									}
									GUILayout.EndHorizontal();

									EditorGUILayoutExtensions.PushEnabled(!isSelectedLabel);
									if (GUILayout.Button("Edit Label", GUILayout.Width(100f)))
									{
										SelectLabel(label);
									}
									EditorGUILayoutExtensions.PopEnabled();

									if (EditorGUILayoutExtensions.XButton())
									{
										if (selectedLabel == label)
										{
											SelectLabel(null);
										}
										model.RemoveLabel(label);
										selectedModified = true;
									}
								}
								GUILayout.EndHorizontal();

								GUILayout.BeginHorizontal();
								{
									GUILayout.Label("Group Id");
									EditorGUILayoutExtensions.PushBackgroundColor(EditorUtilityExtensions.ColorFromString(label.GroupId.Value));
									var groupValue = label.GroupId.Value ?? "< No Group Id >";
									if (16 < groupValue.Length) groupValue = groupValue.Substring(0, 16) + "...";
									//if (GUILayout.Button(groupValue))
									if (GUILayout.Button(new GUIContent(groupValue, "Copy group id"), GUILayout.Width(124f)))
									{
										EditorGUIUtility.systemCopyBuffer = label.GroupId.Value;
										ShowNotification(new GUIContent("Copied Group Id to Clipboard"));
									}
									EditorGUILayoutExtensions.PopBackgroundColor();
								}
								GUILayout.EndHorizontal();
							}
							GUILayout.EndVertical();
						}
						
					}
					GUILayout.EndScrollView();

					OnPreviewSizeSlider(homeGeneralPreviewSize);

					GUILayout.BeginHorizontal();
					{
						switch (labelState)
						{
							case LabelStates.Idle:
								EditorGUILayout.HelpBox("Click on preview to begin defining a new label.", MessageType.Info);
								break;
							case LabelStates.SelectingBegin:
								EditorGUILayout.HelpBox("Click on preview to select where to begin.", MessageType.Info);
								break;
							case LabelStates.SelectingEnd:
								EditorGUILayout.HelpBox("Click on preview to select where to end.", MessageType.Info);
								break;
							case LabelStates.UpdatingBegin:
								EditorGUILayout.HelpBox("Click on preview to select a new begin.", MessageType.Info);
								break;
							case LabelStates.UpdatingEnd:
								EditorGUILayout.HelpBox("Click on preview to select a new end.", MessageType.Info);
								break;
						}

						GUILayout.BeginVertical(GUILayout.Width(72f));
						{
							EditorGUILayoutExtensions.PushEnabled(labelState == LabelStates.Idle && selectedLabel != null);
							if (GUILayout.Button("Deselect"))
							{
								SelectLabel(null);
							}
							EditorGUILayoutExtensions.PopEnabled();

							EditorGUILayoutExtensions.PushEnabled(labelState != LabelStates.Idle);
							EditorGUILayoutExtensions.PushColor(Color.red);
							if (GUILayout.Button("Cancel"))
							{
								switch (labelState)
								{
									case LabelStates.SelectingBegin:
									case LabelStates.SelectingEnd:
										SelectLabel(lastSelectedLabel);
										break;
									case LabelStates.UpdatingBegin:
									case LabelStates.UpdatingEnd:
										SelectLabel(selectedLabel);
										break;
								}
								labelState = LabelStates.Idle;
							}
							EditorGUILayoutExtensions.PopColor();
							EditorGUILayoutExtensions.PopEnabled();

						}
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				homeLabelDetailsScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLabelDetailsScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
				{
					if (selectedLabel == null)
					{
						EditorGUILayout.HelpBox("Select a label to edit it.", MessageType.Info);
						GUILayout.FlexibleSpace();
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label("Color Code By");
							homLabelColorCodeBy.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Color Coding -", homLabelColorCodeBy.Value);
							if (homLabelColorCodeBy.Value == LabelColorCodes.Unknown) homLabelColorCodeBy.Value = LabelColorCodes.Group;
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						EditorGUILayoutExtensions.PushEnabled(labelState == LabelStates.Idle);
						{
							EditorGUIExtensions.BeginChangeCheck();
							{
								GUILayout.BeginHorizontal();
								{
									selectedLabel.Name.Value = EditorGUILayout.TextField("Name", selectedLabel.Name.Value);
									selectedLabel.LabelId.Value = EditorGUILayout.TextField(selectedLabel.LabelId.Value, GUILayout.Width(128f));
									selectedLabel.Source.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select a Source -", selectedLabel.Source.Value, GUILayout.Width(90f));
								}
								GUILayout.EndHorizontal();

								selectedLabel.GroupId.Value = EditorGUILayout.TextField("Group Id", selectedLabel.GroupId.Value);

								switch (selectedLabel.Source.Value)
								{
									case GalaxyLabelSources.Static:
										selectedLabel.StaticText.Key.Value = EditorGUILayout.TextField("Static Key", selectedLabel.StaticText.Key.Value);
										EditorGUILayoutExtensions.PushEnabled(false);
										{
											EditorGUILayout.TextField(new GUIContent("Value", "Edit the name until proper language support is added"), selectedLabel.StaticText.Value.Value);
										}
										EditorGUILayoutExtensions.PopEnabled();
										break;
									case GalaxyLabelSources.GameKeyValue:
										selectedLabel.SourceKey.Value = EditorGUILayout.TextField("Source Key", selectedLabel.SourceKey.Value);
										break;
									default:
										EditorGUILayout.HelpBox("Unrecognized source " + selectedLabel.Source.Value, MessageType.Error);
										break;
								}

								EditorGUILayoutValueFilter.Field("Filtering", selectedLabel.Filtering);

								var labelCurve = selectedLabel.CurveInfo.Value;

								labelCurve.LabelStyle = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select a Style -", labelCurve.LabelStyle);
								labelCurve.FontSize = EditorGUILayout.FloatField("Font Size", labelCurve.FontSize);
								labelCurve.Curve = EditorGUILayoutAnimationCurve.Field("Curve", labelCurve.Curve);
								labelCurve.CurveMaximum = EditorGUILayout.FloatField("Curve Maximum", labelCurve.CurveMaximum);
								labelCurve.FlipAnchors = EditorGUILayout.Toggle("Flip Anchors", labelCurve.FlipAnchors);
								labelCurve.FlipCurve = EditorGUILayout.Toggle("Flip Curve", labelCurve.FlipCurve);

								selectedLabel.CurveInfo.Value = labelCurve;

								GUILayout.BeginHorizontal();
								{
									EditorGUILayout.PrefixLabel("Slice Layer");

									var sliceColors = new Color[] { Color.black, Color.red, Color.yellow, Color.white };

									for (var t = 0; t < 4; t++)
									{
										var isCurrSlice = t == selectedLabel.SliceLayer.Value;
										EditorGUILayoutExtensions.PushBackgroundColor(sliceColors[t]);
										if (GUILayout.Button(new GUIContent(isCurrSlice ? "Selected" : string.Empty), GUILayout.MaxWidth(100f))) selectedLabel.SliceLayer.Value = t;
										EditorGUILayoutExtensions.PopBackgroundColor();
									}
								}
								GUILayout.EndHorizontal();
								//selectedLabel.SliceLayer.Value = Mathf.Max(0, EditorGUILayout.IntField("Slice Layer", selectedLabel.SliceLayer.Value));

								GUILayout.BeginHorizontal(EditorStyles.helpBox);
								{
									GUILayout.BeginVertical();
									{
										selectedLabel.BeginAnchorNormal.Value = EditorGUILayout.Vector3Field("Begin Anchor", selectedLabel.BeginAnchorNormal);
									}
									GUILayout.EndVertical();
									EditorGUILayoutExtensions.PushBackgroundColor(Color.cyan);
									if (GUILayout.Button("Update Begin", GUILayout.Width(100f), GUILayout.Height(51f)))
									{
										labelState = LabelStates.UpdatingBegin;
									}
									EditorGUILayoutExtensions.PopBackgroundColor();
								}
								GUILayout.EndHorizontal();

								GUILayout.BeginHorizontal(EditorStyles.helpBox);
								{
									GUILayout.BeginVertical();
									{
										selectedLabel.EndAnchorNormal.Value = EditorGUILayout.Vector3Field("EndAnchor", selectedLabel.EndAnchorNormal);
									}
									GUILayout.EndVertical();
									EditorGUILayoutExtensions.PushBackgroundColor(Color.magenta);
									if (GUILayout.Button("Update End", GUILayout.Width(100f), GUILayout.Height(51f)))
									{
										labelState = LabelStates.UpdatingEnd;
									}
									EditorGUILayoutExtensions.PopBackgroundColor();
								}
								GUILayout.EndHorizontal();
							}
							EditorGUIExtensions.EndChangeCheck(ref selectedModified);
						}
						EditorGUILayoutExtensions.PopEnabled();
					}
				}
				GUILayout.EndScrollView();

			}
			GUILayout.EndHorizontal();

			var previewTexture = Texture2D.blackTexture;

			switch (selectedScale)
			{
				case UniverseScales.Quadrant:
					previewTexture = model.Details;
					break;
				case UniverseScales.Galactic:
					previewTexture = model.FullPreview;
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized scale "+selectedScale, MessageType.Error);
					break;
			}

			var displayArea = DisplayPreview(
				previewTexture,
				homeGeneralPreviewSize,
				clickPosition => OnHomeSelectedLabelsPrimaryClickPreview(model, clickPosition, selectedScale),
				clickPosition => OnHomeSelectedLabelsSecondaryClickPreview(model, clickPosition, selectedScale),
				!isOverAnAllLabel
			);

			isOverAnAllLabel = false;
			if (selectedLabel == null) 
			{
				OnHomeSelectedLabelsShowAll(model, selectedScale, displayArea);
				return;
			}

			var beginInPreview = true;
			var endInPreview = true;

			var beginAnchorInWindow = NormalToWindow(selectedLabel.BeginAnchorNormal.Value, displayArea, out beginInPreview);
			var endAnchorInWindow = NormalToWindow(selectedLabel.EndAnchorNormal.Value, displayArea, out endInPreview);

			var beginColor = labelState == LabelStates.UpdatingBegin ? Color.cyan.NewS(0.25f) : Color.cyan;

			EditorGUILayoutExtensions.PushColor(beginInPreview ? beginColor : beginColor.NewA(beginColor.a * 0.5f));
			{
				GUI.Box(CenteredScreen(beginAnchorInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Anchor Begin"), SubLightEditorConfig.Instance.LabelAnchorStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			switch(labelState)
			{
				case LabelStates.Idle:
				case LabelStates.UpdatingBegin:
				case LabelStates.UpdatingEnd:
					var endColor = labelState == LabelStates.UpdatingEnd ? Color.magenta.NewS(0.25f) : Color.magenta;
					EditorGUILayoutExtensions.PushColor(endInPreview ? endColor : endColor.NewA(endColor.a * 0.5f));
					{
						GUI.Box(CenteredScreen(endAnchorInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Anchor End"), SubLightEditorConfig.Instance.LabelAnchorStyle);
					}
					EditorGUILayoutExtensions.PopColor();
					break;
			}

			if (labelState != LabelStates.Idle) return;

			var previewCurveInfo = selectedLabel.CurveInfo.Value; // We modify this to make changes for rendering to the screen...
			previewCurveInfo.FlipCurve = !previewCurveInfo.FlipCurve;

			var currBegin = new Vector3(beginAnchorInWindow.x, 0f, beginAnchorInWindow.y);
			var currEnd = new Vector3(endAnchorInWindow.x, 0f, endAnchorInWindow.y);

			for (var i = 0; i < SelectedLabelCurveSampling; i++)
			{
				var curvePos = previewCurveInfo.Evaluate(currBegin, currEnd, i / (SelectedLabelCurveSampling - 1f), false);
				var curvePosInWindow = new Vector2(curvePos.x, curvePos.z);

				EditorGUILayoutExtensions.PushColor(Color.yellow);
				{
					GUI.Box(CenteredScreen(curvePosInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Position on Curve"), SubLightEditorConfig.Instance.LabelCurvePointStyle);
				}
				EditorGUILayoutExtensions.PopColor();
			}
		}

		void OnHomeSelectedLabelsShowAll(
			GalaxyInfoModel model,
			UniverseScales scale,
			Rect displayArea
		)
		{
			var labels = model.GetLabels(scale);
			var sampling = scale == UniverseScales.Quadrant ? AllLabelsCurveSamplingQuadrant : AllLabelsCurveSamplingGalactic;
			var labelIndex = -1;

			foreach (var label in labels)
			{
				labelIndex++;

				var color = homLabelColorCodeBy.Value == LabelColorCodes.Label ? EditorUtilityExtensions.ColorFromIndex(labelIndex) : EditorUtilityExtensions.ColorFromString(label.GroupId.Value);

				EditorGUILayoutExtensions.PushColor(color);

				var beginInPreview = true;
				var endInPreview = true;

				var beginAnchorInWindow = NormalToWindow(label.BeginAnchorNormal.Value, displayArea, out beginInPreview);
				var endAnchorInWindow = NormalToWindow(label.EndAnchorNormal.Value, displayArea, out endInPreview);

				var previewCurveInfo = label.CurveInfo.Value; // We modify this to make changes for rendering to the screen...
				previewCurveInfo.FlipCurve = !previewCurveInfo.FlipCurve;

				var currBegin = new Vector3(beginAnchorInWindow.x, 0f, beginAnchorInWindow.y);
				var currEnd = new Vector3(endAnchorInWindow.x, 0f, endAnchorInWindow.y);

				for (var i = 0; i < sampling; i++)
				{
					var progress = i / (sampling - 1f);
					var curvePos = previewCurveInfo.Evaluate(currBegin, currEnd, progress, false);
					var curvePosInWindow = new Vector2(curvePos.x, curvePos.z);
					
					GUI.Box(CenteredScreen(curvePosInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Position on Curve"), SubLightEditorConfig.Instance.LabelCurvePointStyle);
				}

				var centerPos = previewCurveInfo.Evaluate(currBegin, currEnd, 0.5f, false);
				var centerPosInWindow = new Vector2(centerPos.x, centerPos.z);
				var selectCurrentArea = CenteredScreen(centerPosInWindow, new Vector2(16f, 16f));

				if (GUI.Button(selectCurrentArea, new GUIContent(string.Empty, label.Name.Value), SubLightEditorConfig.Instance.LabelCurveCenterStyle))
				{
					SelectLabel(label);
				}

				isOverAnAllLabel = isOverAnAllLabel || selectCurrentArea.Contains(Event.current.mousePosition);

				EditorGUILayoutExtensions.PopColor();
			}
		}

		void OnHomeSelectedLabelsPrimaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition, UniverseScales scale)
		{
			switch(labelState)
			{
				case LabelStates.Idle:
					lastSelectedLabel = selectedLabel;
					selectedLabel = CreateNewLabel(scale);
					homeLabelsSelectedLabelId.Value = selectedLabel.LabelId.Value;
					selectedLabel.BeginAnchorNormal.Value = clickPosition;

					labelState = LabelStates.SelectingBegin;

					TextDialogPopup.Show(
						"New Label",
						value =>
						{
							labelState = LabelStates.SelectingEnd;
							selectedLabel.Name.Value = value;
						},
						() =>
						{
							SelectLabel(lastSelectedLabel);
						}
					);
					break;
				case LabelStates.SelectingBegin:
					selectedLabel.BeginAnchorNormal.Value = clickPosition;
					labelState = LabelStates.SelectingEnd;
					break;
				case LabelStates.SelectingEnd:
					selectedLabel.EndAnchorNormal.Value = clickPosition;
					model.AddLabel(selectedLabel);
					selectedModified = true;
					labelState = LabelStates.Idle;
					break;
				case LabelStates.UpdatingBegin:
					selectedLabel.BeginAnchorNormal.Value = clickPosition;
					selectedModified = true;
					labelState = LabelStates.Idle;
					break;
				case LabelStates.UpdatingEnd:
					selectedLabel.EndAnchorNormal.Value = clickPosition;
					selectedModified = true;
					labelState = LabelStates.Idle;
					break;
				default:
					Debug.LogError("Unrecognized state " + labelState);
					break;
			}
		}

		void OnHomeSelectedLabelsSecondaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition, UniverseScales scale)
		{
			switch (labelState)
			{
				case LabelStates.Idle:
					SelectLabel(null);
					return;
				case LabelStates.SelectingBegin:
				case LabelStates.SelectingEnd:
					SelectLabel(lastSelectedLabel);
					break;
				case LabelStates.UpdatingBegin:
				case LabelStates.UpdatingEnd:
					SelectLabel(selectedLabel);
					break;
			}
			labelState = LabelStates.Idle;
		}

		GalaxyLabelModel CreateNewLabel(UniverseScales scale, string groupId = null)
		{
			var result = new GalaxyLabelModel();
			result.LabelId.Value = Guid.NewGuid().ToString();
			result.GroupId.Value = string.IsNullOrEmpty(groupId) ? Guid.NewGuid().ToString() : groupId;
			result.Scale.Value = scale;

			return result;
		}

		void OnHomeSelectedGeneration(GalaxyInfoModel model)
		{
			homeGenerationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeGenerationBarScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					if (model.MaximumSectorSystemCount < model.MinimumSectorSystemCount) EditorGUILayout.HelpBox("Maximum Sector System Count must be higher than the minimum", MessageType.Error);
					model.MinimumSectorSystemCount.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Minimum Sector System Count", "The minimum bodies ever spawned in a sector."), model.MinimumSectorSystemCount));
					model.MaximumSectorSystemCount.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Maximum Sector System Count", "The maximum bodies ever spawned in a sector."), model.MaximumSectorSystemCount));

					model.SectorSystemChance.Value = EditorGUILayoutAnimationCurve.Field(new GUIContent("Sector System Chance", "The bodymap is a linear gradient that is evaluated along a curve, then remapped between the minimum and maximum sector body count."), model.SectorSystemChance.Value);
				}
				EditorGUIExtensions.EndChangeCheck(ref selectedModified);
			}
			GUILayout.EndScrollView();
		}

		void NewModel()
		{
			var model = SaveLoadService.Create<GalaxyInfoModel>();
			model.GalaxyId.Value = model.SetMetaKey(MetaKeyConstants.GalaxyInfo.GalaxyId, Guid.NewGuid().ToString());
			model.Name.Value = string.Empty;
			model.Meta.Value = model.Name;

			SaveLoadService.Save(model, OnNewModel);
		}

		void LoadList()
		{
			modelListStatus = RequestStatus.Unknown;
			SaveLoadService.List<GalaxyInfoModel>(OnLoadList);
		}

		void LoadSelected(SaveModel model)
		{
			if (model == null)
			{
				homeSelectedPath.Value = null;
				selectedStatus = RequestStatus.Failure;
				return;
			}
			selectedStatus = RequestStatus.Unknown;
			homeSelectedPath.Value = model.Path;
			SaveLoadService.Load<GalaxyInfoModel>(model, OnLoadSelected);
		}

		void SaveSelected()
		{
			if (selected == null) return;
			SaveLoadService.Save(selected, OnSaveSelected, false);
		}

		void OnSaveSelected(SaveLoadRequest<GalaxyInfoModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			selected = result.TypedModel;
			selectedModified = false;
			modelListStatus = RequestStatus.Cancel;
		}

		void OnNewModel(SaveLoadRequest<GalaxyInfoModel> result)
		{
			selectedStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			modelListStatus = RequestStatus.Cancel;
			homeSelectedPath.Value = result.TypedModel.Path;
			selected = result.TypedModel;
		}

		void OnLoadList(SaveLoadArrayRequest<SaveModel> result)
		{
			modelListStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			modelList = result.Models;

			if (selected == null) return;
			if (modelList.FirstOrDefault(e => e.Path.Value == selected.Path.Value) != null) return;
			OnDeselect();
		}

		void OnDeselect()
		{
			homeState.Value = HomeStates.Browsing;
			selectedStatus = RequestStatus.Failure;
			selected = null;
			selectedModified = false;
		}

		void OnDrawModel(SaveModel model, ref bool isAlternate)
		{
			var isSelected = homeSelectedPath.Value == model.Path.Value;
			if (isSelected || isAlternate) EditorGUILayoutExtensions.PushBackgroundColor(isSelected ? Color.blue : Color.grey);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (!isSelected && isAlternate) EditorGUILayoutExtensions.PopBackgroundColor();
			{
				var modelPath = model.IsInternal ? model.InternalPath : model.Path;
				var modelId = model.GetMetaKey(MetaKeyConstants.GalaxyInfo.GalaxyId);
				var modelName = modelId;
				if (string.IsNullOrEmpty(modelName)) modelName = "< No Id >";
				else if (20 < modelName.Length) modelName = modelName.Substring(0, 20) + "...";

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical();
					{
						GUILayout.Label(new GUIContent(string.IsNullOrEmpty(model.Meta) ? "< No Meta >" : model.Meta, "Name is set by Meta field."), EditorStyles.boldLabel, GUILayout.Height(14f));
						if (GUILayout.Button(new GUIContent(modelName, "Copy Id of " + modelPath)))
						{
							EditorGUIUtility.systemCopyBuffer = modelId;
							ShowNotification(new GUIContent("Copied Id to Clipboard"));
						}
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{
						if (GUILayout.Button("Edit"))
						{
							SelectLabel(null);
							LoadSelected(model);
						}
						if (GUILayout.Button("Select In Project"))
						{
							EditorUtility.FocusProjectWindow();
							Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(model.InternalPath);
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			if (isSelected) EditorGUILayoutExtensions.PopBackgroundColor();
			isAlternate = !isAlternate;
		}

		void OnLoadSelected(SaveLoadRequest<GalaxyInfoModel> result)
		{
			GUIUtility.keyboardControl = 0;
			selectedStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			var model = result.TypedModel;
			selectedModified = false;
			homeState.Value = HomeStates.Selected;
			homeSelectedPath.Value = model.Path;
			selected = model;
		}

		#region Utility
		Vector3 ScreenToNormal(Vector2 screenPosition, Rect window, Rect preview)
		{
			var previewOffset = ((screenPosition - window.min) - preview.min);
			return new Vector3(previewOffset.x / preview.width, 0f, 1f - (previewOffset.y / preview.height));
		}

		Vector2 NormalToWindow(Vector3 normalPosition, Rect preview, out bool inPreview)
		{
			var result = preview.min + new Vector2(preview.width * normalPosition.x, preview.height * (1f - normalPosition.z));
			inPreview = preview.Contains(result);
			return new Vector2(Mathf.Clamp(result.x, preview.xMin, preview.xMax), Mathf.Clamp(result.y, preview.yMin, preview.yMax));
		}

		Rect CenteredScreen(Vector2 screenPosition, Vector2 size)
		{
			return new Rect(screenPosition - (size * 0.5f), size);
		}

		Rect DisplayPreview(
			Texture2D texture,
			DevPrefsInt previewSize,
			Action<Vector3> primaryClick,
			Action<Vector3> secondaryClick = null,
			bool isClickable = true
		)
		{
			texture = texture ?? Texture2D.blackTexture;

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (isClickable)
				{
					if (GUILayout.Button(new GUIContent(texture), GUIStyle.none, GUILayout.MaxWidth(previewSize), GUILayout.MaxHeight(previewSize)))
					{
						var universePosition = ScreenToNormal(
							GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
							position,
							lastPreviewRect
						);

						if (secondaryClick != null)
						{
							if (Event.current.button == 1) secondaryClick(universePosition);
							else if (primaryClick != null) primaryClick(universePosition);
						}
						else if (primaryClick != null) primaryClick(universePosition);
					}
				}
				else
				{
					GUILayout.Box(new GUIContent(texture), GUIStyle.none, GUILayout.MaxWidth(previewSize), GUILayout.MaxHeight(previewSize));
				}

				if (Event.current.type == EventType.Repaint) lastPreviewRect = GUILayoutUtility.GetLastRect();

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			return lastPreviewRect;
		}

		void OnPreviewSizeSlider(DevPrefsInt target)
		{
			target.Value = Mathf.Clamp(EditorGUILayout.IntSlider(new GUIContent("Preview Size"), target.Value, PreviewMinSize, PreviewMaxSize), PreviewMinSize, PreviewMaxSize);
		}

		void SelectLabel(GalaxyLabelModel label, LabelStates state = LabelStates.Idle)
		{
			selectedLabel = label;
			homeLabelsSelectedLabelId.Value = selectedLabel == null ? null : selectedLabel.LabelId.Value;
			if (state != LabelStates.Unknown) labelState = state;
			GUIUtility.keyboardControl = 0;
		}
		#endregion
	}
}