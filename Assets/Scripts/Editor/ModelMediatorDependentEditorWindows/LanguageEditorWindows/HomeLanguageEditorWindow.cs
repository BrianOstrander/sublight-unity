using System;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class LanguageEditorWindow
	{
		enum HomeStates
		{
			Unknown = 0,
			Browsing = 10,
			Selected = 20
		}

		EditorPrefsFloat homeLeftBarScroll;
		EditorPrefsFloat homeEntriesScroll;
		EditorPrefsString homeSelectedPath;
		EditorPrefsEnum<HomeStates> homeState;
		EditorPrefsInt homeSelectedToolbar;

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus saveListStatus;
		SaveModel[] saveList = new SaveModel[0];

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus selectedStatus;
		LanguageDatabaseModel selected;
		bool selectedModified;

		void OnHomeConstruct()
		{
			homeLeftBarScroll = new EditorPrefsFloat(KeyPrefix + "LeftBarScroll");
			homeEntriesScroll = new EditorPrefsFloat(KeyPrefix + "EntriesScroll");
			homeSelectedPath = new EditorPrefsString(KeyPrefix + "HomeSelected");
			homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);
			homeSelectedToolbar = new EditorPrefsInt(KeyPrefix + "HomeSelectedState");

			Enable += OnHomeEnable;
			Disable += OnHomeDisable;
			Save += SaveSelected;
		}

		void OnHomeEnable()
		{
			saveListStatus = RequestStatus.Cancel;
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
			switch (saveListStatus)
			{
				case RequestStatus.Cancel:
					LoadSaveList();
					return;
				case RequestStatus.Unknown:
					return;
			}
			if (string.IsNullOrEmpty(homeSelectedPath.Value)) return;

			switch (selectedStatus)
			{
				case RequestStatus.Cancel:
					LoadSelected(saveList.FirstOrDefault(m => m.Path == homeSelectedPath.Value));
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
				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("New")) CreateNew();
					if (GUILayout.Button("Refresh")) LoadSaveList();
				}
				GUILayout.EndHorizontal();

				homeLeftBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLeftBarScroll), false, true).y;
				{
					var isAlternate = false;
					foreach (var info in saveList) OnDrawSaveInfo(info, ref isAlternate);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		void OnHomeRightPane(LanguageDatabaseModel model = null)
		{
			if (model == null)
			{
				GUILayout.BeginVertical();
				{
					EditorGUILayout.HelpBox("Select a language to edit.", MessageType.Info);
				}
				GUILayout.EndVertical();
				return;
			}

			string[] names =
			{
				"General",
				"Entries"
			};

			GUILayout.BeginVertical();
			{
				var entryName = string.IsNullOrEmpty(model.Name) ? "< No Name > " : model.Name;
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Editing: " + entryName);
					GUI.enabled = selectedModified;
					if (GUILayout.Button("Save", GUILayout.Width(64f))) Save();
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				homeSelectedToolbar.Value = GUILayout.Toolbar(Mathf.Min(homeSelectedToolbar, names.Length - 1), names);

				switch (homeSelectedToolbar.Value)
				{
					case 0:
						OnHomeSelectedGeneral(model);
						break;
					case 1:
						OnHomeSelectedEntries(model);
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized index", MessageType.Error);
						break;
				}
			}
			GUILayout.EndVertical();
		}

		void OnHomeSelectedGeneral(LanguageDatabaseModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.LanguageId.Value = model.SetMetaKey(MetaKeyConstants.LanguageDatabase.LanguageId, EditorGUILayout.TextField("Language Id", model.LanguageId.Value));
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();
				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;
				model.LanguageTag.Value = EditorGUILayout.TextField(new GUIContent("Language Tag", "The standardized language tag, such as en-US."), model.LanguageTag.Value);
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The internal description for notes and production purposes."), model.Description.Value, leftOffset: false);

				model.Tags.Value = EditorGUILayoutExtensions.StringArray(
					"Tags",
					model.Tags.Value
				);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedModified);
		}

		void OnHomeSelectedEntries(LanguageDatabaseModel model)
		{
			var duplicates = model.Language.Duplicates;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Entry Count: " + model.Language.Edges.Length + " | Duplicates: " + duplicates.Length);

				EditorGUILayoutExtensions.PushColor(Color.red);
				{
					if (GUILayout.Button("Delete All Entries", GUILayout.ExpandWidth(false)))
					{
						if (EditorUtilityExtensions.DialogConfirm("Are you sure you want to delete all entries?"))
						{
							selectedModified = true;
							model.Language.Edges = new LanguageDatabaseEdge[0];
						}
					}
				}
				EditorGUILayoutExtensions.PopColor();
			}
			GUILayout.EndHorizontal();

			var hasDuplicates = duplicates.Any();
			if (hasDuplicates) EditorGUILayout.HelpBox("There are duplicate values.", MessageType.Info);

			homeEntriesScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeEntriesScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					var alternate = false;
					foreach (var entry in model.Language.Edges)
					{
						var isDuplicate = hasDuplicates && duplicates.Contains(entry.Key);

						if (alternate) EditorGUILayoutExtensions.PushColor(Color.gray);
						GUILayout.BeginVertical(EditorStyles.helpBox);
						if (alternate) EditorGUILayoutExtensions.PopColor();
						{
							if (isDuplicate) EditorGUILayoutExtensions.PushColor(Color.yellow);

							GUILayout.BeginHorizontal();
							{
								GUILayout.Label(entry.Key, EditorStyles.boldLabel);
								if (isDuplicate) GUILayout.Label("[ Duplicate ]", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
							}
							GUILayout.EndHorizontal();
							EditorGUILayoutExtensions.PushIndent();
							{
								var original = entry.Value;
								var result = EditorGUILayoutExtensions.TextDynamic(original);
								if (original != result)
								{
									var apply = true;
									if (string.IsNullOrEmpty(result)) apply = EditorUtilityExtensions.DialogConfirm("Entering a null or empty string will remove this entry.");
									if (apply) model.Language.Set(entry.Key, result);
								}
							}
							EditorGUILayoutExtensions.PopIndent();

							GUILayout.Space(4f);

							if (isDuplicate) EditorGUILayoutExtensions.PopColor();
						}
						GUILayout.EndVertical();

						alternate = !alternate;
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref selectedModified);
			}
			GUILayout.EndScrollView();
		}

		void CreateNew()
		{
			var info = SaveLoadService.Create<LanguageDatabaseModel>();
			info.LanguageId.Value = info.SetMetaKey(MetaKeyConstants.LanguageDatabase.LanguageId, Guid.NewGuid().ToString());
			info.Name.Value = string.Empty;
			info.Meta.Value = info.Name;
			SaveLoadService.Save(info, OnCreateNew);
		}

		void LoadSaveList()
		{
			saveListStatus = RequestStatus.Unknown;
			SaveLoadService.List<LanguageDatabaseModel>(OnLoadSaveList);
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
			SaveLoadService.Load<LanguageDatabaseModel>(model, OnLoadSelected);
		}

		void SaveSelected()
		{
			if (selected == null) return;
			SaveLoadService.Save(selected, OnSaveSelected, false);
		}

		void OnSaveSelected(SaveLoadRequest<LanguageDatabaseModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			selected = result.TypedModel;
			selectedModified = false;
			saveListStatus = RequestStatus.Cancel;
		}

		void OnCreateNew(SaveLoadRequest<LanguageDatabaseModel> result)
		{
			selectedStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			saveListStatus = RequestStatus.Cancel;
			homeSelectedPath.Value = result.TypedModel.Path;
			selected = result.TypedModel;
		}

		void OnLoadSaveList(SaveLoadArrayRequest<SaveModel> result)
		{
			saveListStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			saveList = result.Models;

			if (selected == null) return;
			if (saveList.FirstOrDefault(e => e.Path.Value == selected.Path.Value) != null) return;
			OnDeselect();
		}

		void OnDeselect()
		{
			homeState.Value = HomeStates.Browsing;
			selectedStatus = RequestStatus.Failure;
			selected = null;
			selectedModified = false;
		}

		void OnDrawSaveInfo(SaveModel info, ref bool isAlternate)
		{
			var isSelected = homeSelectedPath.Value == info.Path.Value;
			if (isSelected || isAlternate) EditorGUILayoutExtensions.PushBackgroundColor(isSelected ? Color.blue : Color.grey);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (!isSelected && isAlternate) EditorGUILayoutExtensions.PopBackgroundColor();
			{
				var infoPath = info.IsInternal ? info.InternalPath : info.Path;
				var infoId = info.GetMetaKey(MetaKeyConstants.LanguageDatabase.LanguageId);
				var infoName = infoId;
				if (string.IsNullOrEmpty(infoName)) infoName = "< No LanguageId >";
				else if (20 < infoName.Length) infoName = infoName.Substring(0, 20) + "...";

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical();
					{
						GUILayout.Label(new GUIContent(string.IsNullOrEmpty(info.Meta) ? "< No Meta >" : info.Meta, "Name is set by Meta field."), EditorStyles.boldLabel, GUILayout.Height(14f));
						if (GUILayout.Button(new GUIContent(infoName, "Copy LanguageId of " + infoPath)))
						{
							EditorGUIUtility.systemCopyBuffer = infoId;
							ShowNotification(new GUIContent("Copied LanguageId to Clipboard"));
						}
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{
						if (GUILayout.Button("Edit"))
						{
							LoadSelected(info);
						}
						if (GUILayout.Button("Select In Project"))
						{
							EditorUtility.FocusProjectWindow();
							Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(info.InternalPath);
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

		void OnLoadSelected(SaveLoadRequest<LanguageDatabaseModel> result)
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