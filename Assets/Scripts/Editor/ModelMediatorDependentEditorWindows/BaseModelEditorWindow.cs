using System;
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

		EditorPrefsBool modelAlwaysAllowSaving;
		EditorPrefsEnum<ModelSelectionStates> modelSelectionState;
		EditorPrefsEnum<ModelTabStates> modelTabState;
		EditorPrefsFloat modelSelectorScroll;
		EditorPrefsString modelSelectedPath;
		EditorPrefsInt modelSelectedToolbar;

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

		protected BaseModelEditorWindow(string keyPrefix) : base(keyPrefix)
		{
			modelAlwaysAllowSaving = new EditorPrefsBool(KeyPrefix + "ModelAlwaysAllowSaving");
			modelSelectionState = new EditorPrefsEnum<ModelSelectionStates>(KeyPrefix + "ModelSelectionState", ModelSelectionStates.Browsing);
			modelTabState = new EditorPrefsEnum<ModelTabStates>(KeyPrefix + "ModelTabState", ModelTabStates.Maximized);
			modelSelectorScroll = new EditorPrefsFloat(KeyPrefix + "ModelSelectorScroll");
			modelSelectedPath = new EditorPrefsString(KeyPrefix + "ModelSelectedPath");
			modelSelectedToolbar = new EditorPrefsInt(KeyPrefix + "ModelSelectedToolbar");

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
			if (string.IsNullOrEmpty(modelSelectedPath.Value)) return;

			switch (selectedStatus)
			{
				case RequestStatus.Cancel:
					OnLoadSelection(modelList.FirstOrDefault(m => m.Path == modelSelectedPath.Value));
					return;
			}
		}

		void OnNewModel(M model)
		{
			SaveLoadService.Save(model, OnNewModelSaveDone);
		}

		void OnNewModelSaveDone(SaveLoadRequest<M> result)
		{
			selectedStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			modelListStatus = RequestStatus.Cancel;
			modelSelectedPath.Value = result.TypedModel.Path;
			ModelSelection = result.TypedModel;
		}

		void OnLoadList()
		{
			modelListStatus = RequestStatus.Unknown;
			SaveLoadService.List<GalaxyInfoModel>(OnLoadListDone);
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
			GUIUtility.keyboardControl = 0;
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

			if (modelListStatus != RequestStatus.Success || selectedStatus != RequestStatus.Success) return;

			GUILayout.BeginHorizontal();
			{
				switch (modelTabState.Value)
				{
					case ModelTabStates.Minimized:
						DrawModelSelectorMinimized();
						break;
					case ModelTabStates.Maximized:
						DrawModelSelectorMaximized();
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized tab state: " + modelTabState.Value, MessageType.Error);
						break;
				}

				GUILayout.BeginVertical();
				{
					switch (modelSelectionState.Value)
					{
						case ModelSelectionStates.Browsing:
							DrawBrowsingEditor();
							break;
						case ModelSelectionStates.Selected:
							DrawSelectedEditor(ModelSelection);
							break;
						default:
							EditorGUILayout.HelpBox("Unrecognized selection state: " + modelSelectionState.Value, MessageType.Error);
							break;
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		void DrawModelSelectorMinimized()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(28f));
			{
				if (GUILayout.Button("<", GUILayout.Width(24f), GUILayout.Height(24f))) SetTabState(ModelTabStates.Maximized);
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();
		}

		void DrawModelSelectorMaximized()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f));
			{
				GUILayout.BeginHorizontal();
				{
					const float buttonHeight = 24f;
					if (GUILayout.Button(">", GUILayout.Height(buttonHeight), GUILayout.Width(buttonHeight))) SetTabState(ModelTabStates.Minimized);
					if (GUILayout.Button("New", GUILayout.Height(buttonHeight))) OnNewModel(CreateModel());
					if (GUILayout.Button("Refresh", GUILayout.Height(buttonHeight))) OnLoadList();
				}
				GUILayout.EndHorizontal();

				modelSelectorScroll.VerticalScroll = GUILayout.BeginScrollView(modelSelectorScroll.VerticalScroll, false, true);
				{
					var isAlternate = false;
					foreach (var model in modelList) OnDrawModel(model, ref isAlternate);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		void OnDrawModel(SaveModel model, ref bool isAlternate)
		{
			var isSelected = modelSelectedPath.Value == model.Path.Value;
			if (isSelected || isAlternate) EditorGUILayoutExtensions.PushBackgroundColor(isSelected ? Color.blue : Color.grey);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (!isSelected && isAlternate) EditorGUILayoutExtensions.PopBackgroundColor();
			{
				var modelPath = model.IsInternal ? model.InternalPath : model.Path;
				var modelId = GetModelId(model);
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
							BeforeLoadSelection();
							OnLoadSelection(model);
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
		}

		void OnModelDisable()
		{
			EditorApplication.modifierKeysChanged -= OnModelModifierKeysChanged;
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
		void OnModelSave()
		{
			if (ModelSelection == null) return;
			SaveLoadService.Save(ModelSelection, OnModelSaveDone, false);
		}

		void OnModelSaveDone(SaveLoadRequest<M> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			ModelSelection = result.TypedModel;
			ModelSelectionModified = false;
			modelListStatus = RequestStatus.Cancel;
		}
		// ------

		#endregion

		#region Unity Events
		void OnModelModifierKeysChanged()
		{
			Repaint();
		}
		#endregion

		#region Child Utilities
		protected void RegisterToolbar(string name, Action<M> callback)
		{
			toolbars.Add(new ToolbarEntry { Name = name, Callback = callback });
		}

		protected void SetTabState(ModelTabStates tabState)
		{
			modelTabState.Value = tabState;
		}
		#endregion

		#region Defaults
		protected virtual M CreateModel()
		{
			var model = SaveLoadService.Create<M>();
			AssignModelId(model, Guid.NewGuid().ToString());
			return model;
		}

		protected virtual void DrawBrowsingEditor()
		{
			EditorGUILayout.HelpBox("Select a model to begin.", MessageType.Info);
		}

		protected virtual void DrawSelectedEditor(M model)
		{
			Action<M> onDraw = DrawSelectedEditorNotImplemented;

			GUILayout.BeginHorizontal();
			{
				var metaName = string.IsNullOrEmpty(model.Meta) ? "< No Meta > " : model.Meta;
				GUILayout.Label("Editing: " + metaName, GUILayout.ExpandWidth(false));

				switch (toolbars.Count)
				{
					case 0: break;
					case 1:
						onDraw = toolbars.First().Callback;
						break;
					default:
						modelSelectedToolbar.Value = GUILayout.Toolbar(Mathf.Min(modelSelectedToolbar, toolbars.Count - 1), toolbars.Select(t => t.Name).ToArray());
						onDraw = toolbars[modelSelectedToolbar].Callback;
						break;
				}

				GUILayout.Label("Always Allow Saving", GUILayout.ExpandWidth(false));
				modelAlwaysAllowSaving.Value = EditorGUILayout.Toggle(modelAlwaysAllowSaving.Value, GUILayout.Width(14f));
				EditorGUILayoutExtensions.PushEnabled(modelAlwaysAllowSaving.Value || ModelSelectionModified);
				{
					if (GUILayout.Button("Save", GUILayout.Width(64f))) Save();
				}
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();

			onDraw(model);
		}
		#endregion

		#region Required
		protected abstract void AssignModelId(M model, string id);
		protected abstract string GetModelId(SaveModel model);
		#endregion

		#region Child Events
		protected Action BeforeLoadSelection = ActionExtensions.Empty;
		protected Action<M> AfterLoadSelection = ActionExtensions.GetEmpty<M>();
		protected Action Deselect = ActionExtensions.Empty;
		#endregion
	}
}