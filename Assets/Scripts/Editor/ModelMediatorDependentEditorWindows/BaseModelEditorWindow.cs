﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public abstract class BaseModelEditorWindow<T, M> : ModelMediatorDependentEditorWindow
		where T : BaseModelEditorWindow<T, M>
		where M : SaveModel, new()
	{
		struct ToolbarEntry
		{
			public string Name;
			public Action<M> Callback;
		}

		enum ModelSelectionStates
		{
			Unknown = 0,
			Browsing = 10,
			Selected = 20
		}

		protected enum ModelTabStates
		{
			Unknown = 0,
			Minimized = 10,
			Maximized = 20
		}

		string readableModelName;

		EditorPrefsBool modelAlwaysAllowSaving;
		EditorPrefsEnum<ModelSelectionStates> modelSelectionState;
		EditorPrefsEnum<ModelTabStates> modelTabState;
		EditorPrefsFloat modelSelectorScroll;
		EditorPrefsString modelSelectedPath;
		EditorPrefsInt modelSelectedToolbar;
		EditorPrefsBool modelShowIgnored;

		bool lastIsPlayingOrWillChangePlaymode;
		int frameDelayRemaining;
		List<ToolbarEntry> toolbars = new List<ToolbarEntry>();

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

		protected M ModelSelection;
		protected bool ModelSelectionModified;

		protected static void OnInitialize(string windowName)
		{
			GetWindow(typeof(T), false, windowName).Show();
		}

		protected BaseModelEditorWindow(string keyPrefix, string readableModelName) : base(keyPrefix)
		{
			this.readableModelName = readableModelName;

			modelAlwaysAllowSaving = new EditorPrefsBool(KeyPrefix + "ModelAlwaysAllowSaving");
			modelSelectionState = new EditorPrefsEnum<ModelSelectionStates>(KeyPrefix + "ModelSelectionState", ModelSelectionStates.Browsing);
			modelTabState = new EditorPrefsEnum<ModelTabStates>(KeyPrefix + "ModelTabState", ModelTabStates.Maximized);
			modelSelectorScroll = new EditorPrefsFloat(KeyPrefix + "ModelSelectorScroll");
			modelSelectedPath = new EditorPrefsString(KeyPrefix + "ModelSelectedPath");
			modelSelectedToolbar = new EditorPrefsInt(KeyPrefix + "ModelSelectedToolbar");
			modelShowIgnored = new EditorPrefsBool(KeyPrefix + "ModelShowIgnored", true);

			Enable += OnModelEnable;
			Disable += OnModelDisable;
			Gui += OnModelGui;
			Save += OnModelSave;
			EditorUpdate += OnModelEditorUpdate;
		}

		#region Creating, Saving, & Loading
		void OnModelCheckStatus()
		{
			switch (modelListStatus)
			{
				case RequestStatus.Cancel:
					OnLoadList();
					return;
				case RequestStatus.Unknown:
					return;
			}

			if (string.IsNullOrEmpty(modelSelectedPath.Value))
			{
				selectedStatus = RequestStatus.Failure;
				modelSelectionState.Value = ModelSelectionStates.Browsing;
				return;
			}

			switch (selectedStatus)
			{
				case RequestStatus.Cancel:
					OnLoadSelection(modelList.FirstOrDefault(m => m.Path == modelSelectedPath.Value));
					return;
			}
		}

		void OnNewModel()
		{
			AskForSaveIfModifiedBeforeContinuing(
				() =>
				{
					TextDialogPopup.Show(
						"New " + readableModelName + " Model",
						value =>
						{
							BeforeLoadSelection();
							SaveLoadService.Save(CreateModel(value), OnNewModelSaveDone);
						},
						doneText: "Create",
						description: "Enter a name for this new " + readableModelName + " model. This will also be used for the meta key."
					);
				}
			);
		}

		void OnNewModelSaveDone(SaveLoadRequest<M> result)
		{
			//selectedStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			modelListStatus = RequestStatus.Cancel;
			//modelSelectedPath.Value = result.TypedModel.Path;
			//ModelSelection = result.TypedModel;
			OnLoadSelection(result.Model);
		}

		void OnLoadList()
		{
			modelListStatus = RequestStatus.Unknown;
			SaveLoadService.List<M>(OnLoadListDone);
		}

		void OnLoadListDone(SaveLoadArrayRequest<SaveModel> result)
		{
			modelListStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			modelList = result.Models;

			if (ModelSelection == null) return;
			if (modelList.FirstOrDefault(e => e.Path.Value == ModelSelection.Path.Value) != null) return;

			// Model doesn't exist so we clear selections
			modelSelectionState.Value = ModelSelectionStates.Browsing;
			selectedStatus = RequestStatus.Failure;
			ModelSelection = null;
			ModelSelectionModified = false;
			Deselect();
		}

		void OnLoadSelection(SaveModel model)
		{
			if (model == null)
			{
				modelSelectedPath.Value = null;
				selectedStatus = RequestStatus.Failure;
				return;
			}
			selectedStatus = RequestStatus.Unknown;
			modelSelectedPath.Value = model.Path;
			SaveLoadService.Load<M>(model, OnLoadSelectionDone);
		}

		void OnLoadSelectionDone(SaveLoadRequest<M> result)
		{
			EditorGUIExtensions.ResetControls();
			selectedStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			var model = result.TypedModel;
			ModelSelectionModified = false;
			modelSelectionState.Value = ModelSelectionStates.Selected;
			modelSelectedPath.Value = model.Path;
			ModelSelection = model;
			AfterLoadSelection(ModelSelection);
		}
		#endregion

		#region Base Drawing
		void DrawAll()
		{
			OnModelEditorUpdate();
			if (lastIsPlayingOrWillChangePlaymode)
			{
				EditorGUILayout.HelpBox("Cannot edit in playmode.", MessageType.Info);
				return;
			}
			if (0 < frameDelayRemaining) return; // Adding a helpbox messes with this...

			OnModelCheckStatus();

			if (modelListStatus != RequestStatus.Success) return;
			if (selectedStatus != RequestStatus.Success && modelSelectionState.Value == ModelSelectionStates.Selected) return;

			if (modelTabState.Value == ModelTabStates.Maximized)
			{
				GUILayout.BeginHorizontal();
				{
					DrawSelector();

					GUILayout.BeginVertical();
					{
						DrawTabs();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				DrawTabs();
			}
		}

		void DrawSelector()
		{
			switch (modelTabState.Value)
			{
				case ModelTabStates.Minimized: break;
				case ModelTabStates.Maximized:
					DrawModelSelectorMaximized();
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized tab state: " + modelTabState.Value, MessageType.Error);
					break;
			}
		}

		void DrawTabs()
		{
			switch (modelSelectionState.Value)
			{
				case ModelSelectionStates.Browsing:
				case ModelSelectionStates.Selected:
					DrawSelectedEditor(ModelSelection);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized selection state: " + modelSelectionState.Value, MessageType.Error);
					break;
			}
		}

		void DrawModelSelectorMaximized()
		{
			EditorGUILayoutExtensions.BeginVertical(SubLightEditorConfig.Instance.SharedModelEditorModelsBackground, SubLightEditorConfig.Instance.SharedModelEditorModelsBackgroundColor, options: GUILayout.Width(300f));
			{
				GUILayout.BeginHorizontal(EditorStyles.toolbar);
				{
					if (GUILayout.Button(GetTabStateLabel(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) ToggleTabState();
					GUILayout.Label(readableModelName + " Entries");
					const float modelSelectorWidth = 72f;
					if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(modelSelectorWidth))) OnNewModel();
					if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(modelSelectorWidth))) OnLoadList();
				}
				GUILayout.EndHorizontal();

				var ignoredCount = 0;
				modelSelectorScroll.VerticalScroll = GUILayout.BeginScrollView(modelSelectorScroll.VerticalScroll);
				{
					var isAlternate = false;
					foreach (var model in modelList.OrderBy(m => m.Meta.Value))
					{
						if (!modelShowIgnored.Value && model.Ignore.Value) ignoredCount++;
						else OnDrawModel(model, ref isAlternate);
					}
				}
				GUILayout.EndScrollView();

				GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
				{
					modelShowIgnored.Value = EditorGUILayout.ToggleLeft(new GUIContent("Show Ignored"), modelShowIgnored.Value, GUILayout.Width(100f));

					GUILayout.FlexibleSpace();

					if (0 < ignoredCount)
					{
						EditorGUILayoutExtensions.PushColor(Color.gray);
						{
							GUILayout.Label(ignoredCount + " entries hidden", GUILayout.ExpandWidth(false));
						}
						EditorGUILayoutExtensions.PopColor();
					}
				}
				GUILayout.EndHorizontal();
			}
			EditorGUILayoutExtensions.EndVertical();
		}

		void OnDrawModel(SaveModel model, ref bool isAlternate)
		{
			var isSelected = modelSelectedPath.Value == model.Path.Value;

			EditorGUILayoutExtensions.BeginVertical(isSelected ? SubLightEditorConfig.Instance.SharedModelEditorModelsEntrySelectedBackground : SubLightEditorConfig.Instance.SharedModelEditorModelsEntryBackground, SubLightEditorConfig.Instance.SharedModelEditorModelsEntryBackgroundColor);
			{
				if (isSelected) EditorGUILayoutExtensions.PushColorCombined(Color.blue.NewS(0.15f), Color.blue.NewS(0.35f));
				{
					var modelPath = model.IsInternal ? model.InternalPath : model.Path;
					var modelId = GetModelId(model);
					var modelName = modelId;
					if (string.IsNullOrEmpty(modelName)) modelName = "< No Id >";
					else if (8 < modelName.Length) modelName = modelName.Substring(0, 8) + "...";

					GUILayout.BeginHorizontal();
					{
						var isIgnored = model.Ignore.Value;
						if (isIgnored) EditorGUILayoutExtensions.PushColor(new Color(0.65f, 0.65f, 0.65f));
						{
							var metaName = string.IsNullOrEmpty(model.Meta) ? "< No Meta >" : model.Meta;
							if (32 < metaName.Length) metaName = metaName.Substring(0, 29) + "...";
							GUILayout.Label(new GUIContent(metaName, "Name is set by Meta field."), EditorStyles.boldLabel, GUILayout.Height(14f));
							GUILayout.FlexibleSpace();
							GUILayout.Label(modelName, EditorStyles.boldLabel);
						}
						if (isIgnored) EditorGUILayoutExtensions.PopColor();
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						if (GUILayout.Button(new GUIContent("Copy Id", "Copy Id of " + modelPath + "."), EditorStyles.miniButtonLeft))
						{
							EditorGUIUtility.systemCopyBuffer = modelId;
							ShowNotification(new GUIContent("Copied Id to Clipboard"));
						}
						if (GUILayout.Button(new GUIContent("Reveal", "Selects the model in the project."), EditorStyles.miniButtonMid))
						{
							EditorUtility.FocusProjectWindow();
							Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(model.InternalPath);
						}
						if (GUILayout.Button("Edit", EditorStyles.miniButtonRight))
						{
							AskForSaveIfModifiedBeforeContinuing(
								() => 
								{
									BeforeLoadSelection();
									OnLoadSelection(model);
								}
							);
						}
					}
					GUILayout.EndHorizontal();
				}
				if (isSelected) EditorGUILayoutExtensions.PopColorCombined();
			}
			EditorGUILayoutExtensions.EndVertical();
			isAlternate = !isAlternate;
		}

		protected void DrawSelectedEditorNotImplemented(M model)
		{
			EditorGUILayout.HelpBox("Register a toolbar or override DrawSelectedEditor to modify the selected model.", MessageType.Warning);
		}
		#endregion

		#region Parent Events
		void OnModelEnable()
		{
			modelListStatus = RequestStatus.Cancel;
			selectedStatus = RequestStatus.Cancel;

			EditorApplication.modifierKeysChanged += OnModelModifierKeysChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		void OnModelDisable()
		{
			EditorApplication.modifierKeysChanged -= OnModelModifierKeysChanged;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		void OnModelGui()
		{
			Exception innerException = null;
			try
			{
				try
				{
					DrawAll();
				}
				catch (ExitGUIException exitGuiException)
				{
					if (!DevPrefs.IgnoreGuiExitExceptions)
					{
						innerException = exitGuiException;
						throw exitGuiException;
					}
				}
				catch (Exception originalException)
				{
					innerException = originalException;
					GUIUtility.ExitGUI();
				}
			}
			catch (ExitGUIException)
			{
				if (innerException.Message.StartsWith("Getting control ")) return;
				EditorGUILayout.HelpBox("Exception occured: \n" + innerException.Message, MessageType.Error);
				Debug.LogException(innerException);
				//if (GUILayout.Button("Print Exception")) Debug.LogException(innerException);
			}
		}

		void OnModelEditorUpdate()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlayingOrWillChangePlaymode != lastIsPlayingOrWillChangePlaymode)
			{
				modelListStatus = RequestStatus.Cancel;
				modelList = new SaveModel[0];
				selectedStatus = RequestStatus.Cancel;
				ModelSelection = null;
				frameDelayRemaining = 8;
			}
			lastIsPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;

			if (!lastIsPlayingOrWillChangePlaymode && 0 < frameDelayRemaining)
			{
				frameDelayRemaining--;
				Repaint();
			}
		}

		// --- SAVING ---
		void OnModelSave(Action<RequestStatus> done)
		{
			if (ModelSelection == null)
			{
				if (done != null) done(RequestStatus.Failure);
				return;
			}
			SaveLoadService.Save(
				ModelSelection,
				result => OnModelSaveDone(result, done),
				false
			);
		}

		void OnModelSaveDone(SaveLoadRequest<M> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				if (done != null) done(result.Status);
				return;
			}
			AssetDatabase.Refresh();
			ModelSelection = result.TypedModel;
			ModelSelectionModified = false;
			modelListStatus = RequestStatus.Cancel;
			if (done != null) done(result.Status);
		}
		// ------

		#endregion

		#region Unity Events
		void OnModelModifierKeysChanged()
		{
			Repaint();
		}

		void OnPlayModeStateChanged(PlayModeStateChange playModeState)
		{
			if (!ModelSelectionModified) return;

			switch (playModeState)
			{
				case PlayModeStateChange.ExitingEditMode:
					if (!EditorApplication.isPlayingOrWillChangePlaymode) break;

					AskForSaveIfModifiedBeforeContinuing(
						() => EditorApplication.isPlaying = true,
						() => EditorApplication.isPlaying = false,
						() => EditorApplication.isPlaying = false
					);

					break;
			}
		}
		#endregion

		#region Child Utilities
		protected void RegisterToolbar(string name, Action<M> callback)
		{
			toolbars.Add(new ToolbarEntry { Name = name, Callback = callback });
		}

		protected GUIContent GetTabStateLabel()
		{
			switch (modelTabState.Value)
			{
				case ModelTabStates.Minimized:
					return new GUIContent(SubLightEditorConfig.Instance.SharedModelEditorOpenModelsImage, "Show " + readableModelName + " entries.");
				case ModelTabStates.Maximized:
					return new GUIContent(SubLightEditorConfig.Instance.SharedModelEditorCloseModelsImage, "Hide " + readableModelName + " entries.");
				default:
					return new GUIContent("?", "Unrecognized TabState: " + modelTabState.Value);
			}
		}

		protected void ToggleTabState()
		{
			switch (modelTabState.Value)
			{
				case ModelTabStates.Minimized: SetTabState(ModelTabStates.Maximized); break;
				case ModelTabStates.Maximized: SetTabState(ModelTabStates.Minimized); break;
				default:
					Debug.LogError("Unrecognized TabState: " + modelTabState.Value);
					break;
			}
		}

		protected void SetTabState(ModelTabStates tabState)
		{
			modelTabState.Value = tabState;
		}
		#endregion

		#region Defaults
		protected virtual M CreateModel(string name)
		{
			var model = SaveLoadService.Create<M>();
			model.Meta.Value = name;
			AssignModelId(model, Guid.NewGuid().ToString());
			AssignModelName(model, name);
			return model;
		}

		protected virtual void DrawSelectedEditor(M model)
		{
			Action<M> onDraw = DrawSelectedEditorNotImplemented;

			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				if (modelTabState.Value == ModelTabStates.Minimized && GUILayout.Button(GetTabStateLabel(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) ToggleTabState();

				if (model == null)
				{
					EditorGUILayout.HelpBox("Select a model to begin editing.", MessageType.Info);
					onDraw = null;
				}
				else
				{
					var metaName = string.IsNullOrEmpty(model.Meta) ? "< No Meta > " : model.Meta;

					GUILayout.Label(metaName, GUILayout.ExpandWidth(false));

					switch (toolbars.Count)
					{
						case 0: break;
						case 1: onDraw = toolbars.First().Callback; break;
						default: onDraw = OnDrawToolbar(); break;
					}

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(64f))) ShowSettingsDialog();

					EditorGUILayoutExtensions.PushEnabled(modelAlwaysAllowSaving.Value || ModelSelectionModified || (!Event.current.shift && Event.current.control));
					{
						var saveContent = ModelSelectionModified ? new GUIContent("*Save*", "There are unsaved changes.") : new GUIContent("Save", "There are no unsaved changes.");
						if (GUILayout.Button(saveContent, EditorStyles.toolbarButton, GUILayout.Width(64f))) Save(null);
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
			}
			GUILayout.EndHorizontal();

			if (onDraw != null) onDraw(model);
		}
		#endregion

		#region Settings Events
		protected void ShowSettingsDialog()
		{
			FlexiblePopupDialog.Show(
				readableModelName + " Editor Settings",
				GetSettingsDialogSize,
				OnSettingsGui
			);
		}

		protected virtual Vector2 GetSettingsDialogSize { get { return new Vector2(500f, 200f); } }

		protected virtual void OnSettingsGui()
		{
			modelAlwaysAllowSaving.Value = EditorGUILayout.Toggle(new GUIContent("Always Allow Saving", "When enabled the 'Save' button always be clickable."), modelAlwaysAllowSaving.Value);
			SettingsGui();
		}
		#endregion

		#region Required
		protected abstract void AssignModelId(M model, string id);
		protected abstract void AssignModelName(M model, string name);
		protected abstract string GetModelId(SaveModel model);
		#endregion

		#region Child Events
		protected Action BeforeLoadSelection = ActionExtensions.Empty;
		protected Action<M> AfterLoadSelection = ActionExtensions.GetEmpty<M>();
		protected Action Deselect = ActionExtensions.Empty;
		protected Action SettingsGui = ActionExtensions.Empty;
		#endregion

		#region Utility
		protected virtual Action<M> OnDrawToolbar()
		{
			var modelSelectedToolbarPrevious = modelSelectedToolbar.Value;
			var toolbarIndex = 0;
			foreach (var option in toolbars.Select(t => t.Name).ToArray())
			{
				if (GUILayout.Toggle(toolbarIndex == modelSelectedToolbar.Value, new GUIContent(option), EditorStyles.toolbarButton, GUILayout.MinWidth(72f)))
				{
					modelSelectedToolbar.Value = toolbarIndex;
				}
				toolbarIndex++;
			}
			if (modelSelectedToolbar.Value != modelSelectedToolbarPrevious) EditorGUIExtensions.ResetControls();
			return toolbars[Mathf.Clamp(modelSelectedToolbar, 0, toolbars.Count - 1)].Callback;
		}

		void AskForSaveIfModifiedBeforeContinuing(
			Action done,
			Action savePrepare = null,
			Action cancel = null
		)
		{
			if (done == null) throw new ArgumentNullException("done");

			if (!ModelSelectionModified)
			{
				done();
				return;
			}

			var result = EditorUtility.DisplayDialogComplex(
				"Unsaved Changes to " + readableModelName,
				"There are unsaved changes to \"" + ModelSelection.Meta.Value + "\", would you like to save them before continuing?",
				"Save",
				"Cancel",
				"Don't Save"
			);

			switch (result)
			{
				case 0: // Save
					if (savePrepare != null) savePrepare();
					Save(
						saveResult =>
						{
							if (saveResult != RequestStatus.Success)
							{
								Debug.LogError("Issue saving " + readableModelName + ", unable to continue. Calling cancel if provided.");
								if (cancel != null) cancel();
								return;
							}
							done();
						}
					);
					break;
				case 1: // Cancel
					if (cancel != null) cancel();
					break;
				case 2: // Don't Save
					done();
					break;
			}
		}
  		#endregion
	}
}