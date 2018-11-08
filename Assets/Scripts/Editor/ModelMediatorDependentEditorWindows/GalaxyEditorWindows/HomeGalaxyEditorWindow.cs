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
		const int PreviewMaxSize = 1024;

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

		GalaxyLabelModel selectedLabel;

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
				model.ClusterOrigin.Value = EditorGUILayoutUniversePosition.FieldSector("Cluster Origin", model.ClusterOrigin);
				EditorGUILayoutExtensions.PushBackgroundColor(Color.yellow);
				model.GalaxyOrigin.Value = EditorGUILayoutUniversePosition.FieldSector("Galaxy Origin", model.GalaxyOrigin);
				EditorGUILayoutExtensions.PopBackgroundColor();
				EditorGUILayoutExtensions.PushBackgroundColor(Color.green);
				model.PlayerStart.Value = EditorGUILayoutUniversePosition.FieldSector("Player Start", model.PlayerStart);
				EditorGUILayoutExtensions.PopBackgroundColor();
				EditorGUILayoutExtensions.PushBackgroundColor(Color.red);
				model.GameEnd.Value = EditorGUILayoutUniversePosition.FieldSector("Game End", model.GameEnd);
				EditorGUILayoutExtensions.PopBackgroundColor();
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

			Vector2 universeSize;

			var displayArea = DisplayPreview(
				previewTexture,
				homeGeneralPreviewSize,
				out universeSize,
				clickPosition =>
				{
					OptionDialogPopup.Show(
						"Set Target",
						new OptionDialogPopup.Entry[]
						{
							OptionDialogPopup.Entry.Create(
								"Galaxy Origin",
								() => model.GalaxyOrigin.Value = clickPosition,
								color: Color.yellow
							),
							OptionDialogPopup.Entry.Create(
								"Player Start",
								() => model.PlayerStart.Value = clickPosition,
								color: Color.green
							),
							OptionDialogPopup.Entry.Create(
								"Game End",
								() => model.GameEnd.Value = clickPosition,
								color: Color.red
							)
						},
						description: "Select the following position to assign the value of ( " + clickPosition.Sector.x + " , " + clickPosition.Sector.z + " ) to."
					);
				}
			);

			var galacticOriginInWindow = UniverseToWindow(model.GalaxyOrigin, displayArea, universeSize, homeGeneralPreviewSize);
			var playerStartInWindow = UniverseToWindow(model.PlayerStart, displayArea, universeSize, homeGeneralPreviewSize);
			var gameEndInWindow = UniverseToWindow(model.GameEnd, displayArea, universeSize, homeGeneralPreviewSize);

			EditorGUILayoutExtensions.PushColor(Color.yellow);
			{
				GUI.Box(CenteredScreen(galacticOriginInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Galactic Origin"), SubLightEditorConfig.Instance.GalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			EditorGUILayoutExtensions.PushColor(Color.green);
			{
				GUI.Box(CenteredScreen(playerStartInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Player Start"), SubLightEditorConfig.Instance.GalaxyTargetStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			EditorGUILayoutExtensions.PushColor(Color.red);
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
					selectedLabel = null;
					labelState = LabelStates.Idle;
				}
			}
			else if (selectedLabel == null)
			{
				selectedLabel = model.GetLabel(homeLabelsSelectedLabelId.Value);
				if (selectedLabel == null)
				{
					homeLabelsSelectedLabelId.Value = null;
				}
				labelState = LabelStates.Idle;
			}

			UniverseScales[] scales =
			{
				UniverseScales.Quadrant,
				UniverseScales.Galactic
			};

			var scaleNames = scales.Select(s => s.ToString()).ToArray();
			var selectedScale = scales[homeLabelsSelectedScale.Value];

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical(GUILayout.Width(350f));
				{
					homeLabelsSelectedScale.Value = GUILayout.Toolbar(Mathf.Min(homeLabelsSelectedScale, scaleNames.Length - 1), scaleNames);
					selectedScale = scales[homeLabelsSelectedScale.Value];

					homeLabelsListScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLabelsListScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
					{
						foreach (var label in model.GetLabels(selectedScale))
						{
							var isSelectedLabel = label.LabelId.Value == homeLabelsSelectedLabelId.Value;
							if (isSelectedLabel) EditorGUILayoutExtensions.PushColor(Color.blue.NewS(0.7f));
							GUILayout.BeginVertical(EditorStyles.helpBox);
							if (isSelectedLabel) EditorGUILayoutExtensions.PopColor();
							{
								GUILayout.BeginHorizontal();
								{
									GUILayout.Label(label.Name.Value, EditorStyles.boldLabel);
									EditorGUILayoutExtensions.PushEnabled(!isSelectedLabel);
									if (GUILayout.Button("Edit Label", GUILayout.Width(100f)))
									{
										selectedLabel = label;
										homeLabelsSelectedLabelId.Value = selectedLabel.LabelId.Value;
										labelState = LabelStates.Idle;
									}
									EditorGUILayoutExtensions.PopEnabled();

									if (EditorGUILayoutExtensions.XButton())
									{
										if (selectedLabel == label)
										{
											selectedLabel = null;
											homeLabelsSelectedLabelId.Value = null;
											labelState = LabelStates.Idle;
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
									if (20 < groupValue.Length) groupValue = groupValue.Substring(0, 16) + "...";
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


					switch(labelState)
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

					if (labelState != LabelStates.Idle)
					{
						EditorGUILayoutExtensions.PushColor(Color.red);
						if (GUILayout.Button("Cancel"))
						{
							switch(labelState)
							{
								case LabelStates.SelectingBegin:
								case LabelStates.SelectingEnd:
									selectedLabel = null;
									homeLabelsSelectedLabelId.Value = null;
									break;
							}
							labelState = LabelStates.Idle;
						}
						EditorGUILayoutExtensions.PopColor();
					}
				}
				GUILayout.EndVertical();

				homeLabelDetailsScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLabelDetailsScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
				{
					if (selectedLabel == null)
					{
						EditorGUILayout.HelpBox("Select a label to edit it.", MessageType.Info);
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

								selectedLabel.BeginAnchor.Value = EditorGUILayoutUniversePosition.Field("Begin Anchor", selectedLabel.BeginAnchor.Value);
								selectedLabel.EndAnchor.Value = EditorGUILayoutUniversePosition.Field("EndAnchor", selectedLabel.EndAnchor.Value);
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

			Vector2 universeSize;

			var displayArea = DisplayPreview(
				previewTexture,
				homeGeneralPreviewSize,
				out universeSize,
				clickPosition => OnHomeSelectedLabelsClickPreview(model, clickPosition, selectedScale)
			);
		}

		void OnHomeSelectedLabelsClickPreview(GalaxyInfoModel model, UniversePosition clickPosition, UniverseScales scale)
		{
			switch(labelState)
			{
				case LabelStates.Idle:
					selectedLabel = CreateNewLabel(scale);
					homeLabelsSelectedLabelId.Value = selectedLabel.LabelId.Value;
					selectedLabel.BeginAnchor.Value = clickPosition;

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
							labelState = LabelStates.Idle;
							selectedLabel = null;
						}
					);
					break;
				case LabelStates.SelectingBegin:
					selectedLabel.BeginAnchor.Value = clickPosition;
					labelState = LabelStates.SelectingEnd;
					break;
				case LabelStates.SelectingEnd:
					selectedLabel.EndAnchor.Value = clickPosition;
					model.AddLabel(selectedLabel);
					labelState = LabelStates.Idle;
					break;
				case LabelStates.UpdatingBegin:
					selectedLabel.BeginAnchor.Value = clickPosition;
					labelState = LabelStates.Idle;
					break;
				case LabelStates.UpdatingEnd:
					selectedLabel.EndAnchor.Value = clickPosition;
					labelState = LabelStates.Idle;
					break;
				default:
					Debug.LogError("Unrecognized state " + labelState);
					break;
			}
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
			/*
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Todo: this");
				}
				GUILayout.EndHorizontal();
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedModified);
			*/

			homeGenerationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeGenerationBarScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					if (model.MaximumSectorBodies < model.MinimumSectorBodies) EditorGUILayout.HelpBox("Maximum Sector Bodies must be higher than the minimum", MessageType.Error);
					model.MinimumSectorBodies.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Minimum Sector Bodies", "The minimum bodies ever spawned in a sector."), model.MinimumSectorBodies));
					model.MaximumSectorBodies.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Maximum Sector Bodies", "The maximum bodies ever spawned in a sector."), model.MaximumSectorBodies));

					model.BodyAdjustment.Value = EditorGUILayoutAnimationCurve.Field(new GUIContent("Body Adjustment", "The bodymap is a linear gradient that is evaluated along a curve, then remapped between the minimum and maximum sector body count."), model.BodyAdjustment.Value);
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
		UniversePosition ScreenToUniverse(Vector2 screenPosition, Rect window, Rect preview, Vector2 universeSize, float shownSize)
		{
			var inUniverse = ((screenPosition - window.min) - preview.min) * (universeSize.y / shownSize);
			return new UniversePosition(new Vector3(inUniverse.x, 0f, universeSize.y - inUniverse.y), Vector3.zero);
		}

		Vector2 UniverseToWindow(UniversePosition universePosition, Rect preview, Vector2 universeSize, float shownSize)
		{
			var universeScaled = new Vector2(universePosition.Sector.x, universeSize.y - universePosition.Sector.z) * (shownSize / universeSize.y);
			return preview.min + universeScaled;
		}

		Rect CenteredScreen(Vector2 screenPosition, Vector2 size)
		{
			return new Rect(screenPosition - (size * 0.5f), size);
		}

		Rect DisplayPreview(
			Texture2D texture,
			DevPrefsInt previewSize,
			out Vector2 universeSize,
			Action<UniversePosition> click
		)
		{
			texture = texture ?? Texture2D.blackTexture;

			universeSize = new Vector2(texture.width, texture.height);

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				EditorGUIExtensions.BeginChangeCheck();
				{
					if (GUILayout.Button(new GUIContent(texture), GUIStyle.none, GUILayout.MaxWidth(previewSize), GUILayout.MaxHeight(previewSize)))
					{
						var universePosition = ScreenToUniverse(
							GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
							position,
							lastPreviewRect,
							universeSize,
							previewSize
						);

						if (click != null) click(universePosition);
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref selectedModified);

				if (Event.current.type == EventType.Repaint) lastPreviewRect = GUILayoutUtility.GetLastRect();

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			OnPreviewSizeSlider(previewSize);

			return lastPreviewRect;
		}

		void OnPreviewSizeSlider(DevPrefsInt target)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				target.Value = Mathf.Clamp(EditorGUILayout.IntSlider(new GUIContent("Preview Size"), target.Value, PreviewMinSize, PreviewMaxSize), PreviewMinSize, PreviewMaxSize);
			}
			GUILayout.EndHorizontal();
		}
		#endregion
	}
}