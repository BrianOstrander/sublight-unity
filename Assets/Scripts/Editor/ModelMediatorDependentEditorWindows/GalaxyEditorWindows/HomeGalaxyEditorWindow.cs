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

		EditorPrefsBool homeAlwaysAllowSaving;
		EditorPrefsFloat homeLeftBarScroll;
		EditorPrefsString homeSelectedPath;
		EditorPrefsEnum<HomeStates> homeState;
		EditorPrefsInt homeSelectedToolbar;

		EditorPrefsInt homeGeneralPreviewSize;
		EditorPrefsFloat homeGeneralPreviewBarScroll;

		EditorPrefsInt homeTargetsSelectedPreview;

		EditorPrefsFloat homeGenerationBarScroll;

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
		GalaxyInfoModel _selected;
		GalaxyInfoModel selected
		{
			get { return _selected; }
			set
			{
				_selected = value;
			}
		}
		bool selectedModified;

		Rect lastTargetsPreviewRect = Rect.zero;

		void OnHomeConstruct()
		{
			homeAlwaysAllowSaving = new EditorPrefsBool(KeyPrefix + "AlwaysAllowSaving");
			homeLeftBarScroll = new EditorPrefsFloat(KeyPrefix + "LeftBarScroll");
			homeSelectedPath = new EditorPrefsString(KeyPrefix + "HomeSelected");
			homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);
			homeSelectedToolbar = new EditorPrefsInt(KeyPrefix + "HomeSelectedState");

			homeGeneralPreviewSize = new EditorPrefsInt(KeyPrefix + "GeneralPreviewSize");
			homeGeneralPreviewBarScroll = new EditorPrefsFloat(KeyPrefix + "GeneralPreviewBarScroll");

			homeTargetsSelectedPreview = new EditorPrefsInt(KeyPrefix + "TargetsSelectedPreview");

			homeGenerationBarScroll = new EditorPrefsFloat(KeyPrefix + "HomeGenerationBarScroll");

			Enable += OnHomeEnable;
			Disable += OnHomeDisable;
			Save += SaveSelected;
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

			OnPreviewSizeSlider();
		}

		void OnHomeSelectedTargets(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				model.PlayerStart.Value = EditorGUILayoutUniversePosition.FieldSector("Player Start", model.PlayerStart);
				model.GameEnd.Value = EditorGUILayoutUniversePosition.FieldSector("Game End", model.GameEnd);
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

			previewTexture = previewTexture ?? Texture2D.blackTexture;

			var universeSize = new Vector2(previewTexture.width, previewTexture.height);

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				EditorGUIExtensions.BeginChangeCheck();
				{
					if (GUILayout.Button(new GUIContent(previewTexture), GUIStyle.none, GUILayout.MaxWidth(homeGeneralPreviewSize), GUILayout.MaxHeight(homeGeneralPreviewSize)))
					{
						var universePosition = ScreenToUniverse(
							GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
							position,
							lastTargetsPreviewRect,
							universeSize,
							homeGeneralPreviewSize
						);

						OptionDialogPopup.Show(
							"Set Target",
							new OptionDialogPopup.Entry[]
							{
								OptionDialogPopup.Entry.Create(
									"Player Start",
									() => model.PlayerStart.Value = universePosition
								),
								OptionDialogPopup.Entry.Create(
									"Game End",
									() => model.GameEnd.Value = universePosition
								)
							},
							description: "Select the following position to assign the value of ( "+universePosition.Sector.x+" , "+universePosition.Sector.z+" ) to."
						);
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref selectedModified);

				if (Event.current.type == EventType.Repaint) lastTargetsPreviewRect = GUILayoutUtility.GetLastRect();

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			var playerStartInWindow = UniverseToWindow(model.PlayerStart, lastTargetsPreviewRect, universeSize, homeGeneralPreviewSize);
			var gameEndInWindow = UniverseToWindow(model.GameEnd, lastTargetsPreviewRect, universeSize, homeGeneralPreviewSize);

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

			OnPreviewSizeSlider();
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

					model.BodyAdjustment.Value = EditorGUILayout.CurveField(new GUIContent("Body Adjustment", "The bodymap is a linear gradient that is evaluated along a curve, then remapped between the minimum and maximum sector body count."), model.BodyAdjustment);
				}
				EditorGUIExtensions.EndChangeCheck(ref selectedModified);
			}
			GUILayout.EndScrollView();
		}

		void OnPreviewSizeSlider()
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				homeGeneralPreviewSize.Value = Mathf.Clamp(EditorGUILayout.IntSlider(new GUIContent("Preview Size"), homeGeneralPreviewSize.Value, PreviewMinSize, PreviewMaxSize), PreviewMinSize, PreviewMaxSize);
			}
			GUILayout.EndHorizontal();
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
			return new UniversePosition(new Vector3(inUniverse.x, 0f, inUniverse.y), Vector3.zero);
		}

		Vector2 UniverseToWindow(UniversePosition universePosition, Rect preview, Vector2 universeSize, float shownSize)
		{
			var universeScaled = new Vector2(universePosition.Sector.x, universePosition.Sector.z) * (shownSize / universeSize.y);
			return preview.min + universeScaled;
		}

		Rect CenteredScreen(Vector2 screenPosition, Vector2 size)
		{
			return new Rect(screenPosition - (size * 0.5f), size);
		}
		#endregion
	}
}