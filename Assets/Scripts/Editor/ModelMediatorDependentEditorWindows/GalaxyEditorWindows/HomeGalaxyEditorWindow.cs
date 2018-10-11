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

		void OnHomeConstruct()
		{
			homeAlwaysAllowSaving = new EditorPrefsBool(KeyPrefix + "AlwaysAllowSaving");
			homeLeftBarScroll = new EditorPrefsFloat(KeyPrefix + "LeftBarScroll");
			homeSelectedPath = new EditorPrefsString(KeyPrefix + "HomeSelected");
			homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);
			homeSelectedToolbar = new EditorPrefsInt(KeyPrefix + "HomeSelectedState");

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
				model.IsPlayable.Value = EditorGUILayout.Toggle(new GUIContent("IsPlayable", "Can the player start a game in this galaxy?"), model.IsPlayable.Value);

				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;

				GUILayout.Label("Todo: rest of these values");
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedModified);
		}

		void OnHomeSelectedGeneration(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Todo: this");
				}
				GUILayout.EndHorizontal();
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedModified);

			homeGenerationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeGenerationBarScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					GUILayout.Label("Todo: this");
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
	}
}