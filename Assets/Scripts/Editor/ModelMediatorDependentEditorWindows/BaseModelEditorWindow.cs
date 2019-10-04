using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Text.RegularExpressions;
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
		class BatchProgress
		{
			public enum States
			{
				Unknown = 0,
				NeverRun = 10,
				InProgress = 20,
				Complete = 30
			}

			public static BatchProgress Default
			{
				get
				{
					return new BatchProgress
					{
						State = States.NeverRun,
						ModelsRemaining = new List<SaveModel>(),
						ModelsCompleted = new List<SaveModel>(),
						ErrorCount = 0,
						Log = String.Empty,
						Write = false
					};
				}
			}

			public States State;
			public List<SaveModel> ModelsRemaining;
			public List<SaveModel> ModelsCompleted;
			public int ErrorCount;
			public string Log;
			public bool Write;

			public BatchModelOperationCache<M>.Entry Operation;
			public SaveModel ModelProcessing;

			public string GetLogPrefix()
			{
				return "[ " + ModelsCompleted.Count + " ] ";
			}
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

		protected const float ToolbarWidth = 64f;

		string readableModelName;

		EditorPrefsBool modelAlwaysAllowSaving;
		EditorPrefsEnum<ModelSelectionStates> modelSelectionState;
		EditorPrefsEnum<ModelTabStates> modelTabState;
		EditorPrefsFloat modelSelectorScroll;
		EditorPrefsString modelSelectedPath;
		EditorPrefsInt modelSelectedToolbar;
		EditorPrefsBool modelShowIgnored;
		EditorPrefsFloat modelBatchScroll;

		BatchModelOperationCache<M> batchCache;

		bool lastIsPlayingOrWillChangePlaymode;
		int frameDelayRemaining;
		List<ModelEditorTab<T, M>> toolbars = new List<ModelEditorTab<T, M>>();

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
		public bool ModelSelectionModified; // TODO: Eeek should this be public? Tabs need it though...

		BatchProgress batchProgress = BatchProgress.Default;

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
			modelBatchScroll = new EditorPrefsFloat(KeyPrefix + "ModelBatchScroll");

			batchCache = new BatchModelOperationCache<M>();

			Enable += OnModelEnable;
			Disable += OnModelDisable;
			Gui += OnModelGui;
			Save += OnModelSave;
			EditorUpdate += OnModelEditorUpdate;
		}

		bool BaseIsEnabled
		{
			get
			{
				switch (batchProgress.State)
				{
					case BatchProgress.States.InProgress:
						return false;
				}
				return true;
			}
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
					EditorModelMediator.Instance.List<M>(
						results =>
						{
							switch (results.Status)
							{
								case RequestStatus.Success:
									var modelName = string.Empty;
									var modelId = string.Empty;
									var modelIdPreview = string.Empty;
									
									FlexiblePopupDialog.Show(
										"New " + readableModelName + " Model",
										new Vector2(400f, 100f),
										close => OnNewModelGui(
											close,
											ref modelName,
											ref modelId,
											ref modelIdPreview,
											results.Models.Select(m => m.Id.Value).ToArray()
										)
									);
									break;
								default:
									Debug.LogError(
										"Listing models of type "+typeof(M)+" returned status "+results.Status+" and error: "+results.Error
									);
									break;
							}
							
						}
					);
//					TextDialogPopup.Show(
//						"New " + readableModelName + " Model",
//						value =>
//						{
//							BeforeLoadSelection();
//							SaveLoadService.Save(CreateModel(value), OnNewModelSaveDone);
//						},
//						doneText: "Create",
//						description: "Enter a name for this new " + readableModelName + " model. This will also be used for the meta key."
//					);
				}
			);
		}

		void OnNewModelGui(
			Action close,
			ref string modelName,
			ref string modelId,
			ref string modelIdPreview,
			string[] existingModelIds
		)
		{
			GUILayout.Label(
				"Enter a name for this new " + readableModelName + " model.",
				TextDialogPopup.Styles.DescriptionLabel
			);

			var oldModelName = modelName;
			modelName = GUILayout.TextField(
				modelName,
				TextDialogPopup.Styles.TextField,
				GUILayout.ExpandWidth(true)
			);

			if (oldModelName != modelName)
			{
				modelId = modelName.Replace(' ', '_').ToLower();
				if (!modelId.Replace("_", "").All(Char.IsLetterOrDigit))
				{
					modelIdPreview = "Invalid Id! Must contain only alphanumeric characters.";
					modelId = null;
				}
				else if (existingModelIds.Contains(modelId))
				{
					modelIdPreview = "Invalid Id! An existing model with that id exists.";
					modelId = null;
				}
				else modelIdPreview = modelId;
			}
			
			GUILayout.Label(
				modelIdPreview,
				TextDialogPopup.Styles.DescriptionLabel
			);

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Cancel", TextDialogPopup.Styles.Button)) close();
				EditorGUILayoutExtensions.PushEnabled(!string.IsNullOrEmpty(modelId));
				{
					if (GUILayout.Button("Create", TextDialogPopup.Styles.Button))
					{
						close();
						BeforeLoadSelection();
						SaveLoadService.Save(CreateModel(modelId, modelName), OnNewModelSaveDone);
					}
				}
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();
		}

		void OnNewModelSaveDone(SaveLoadRequest<M> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			modelListStatus = RequestStatus.Cancel;
			OnLoadSelection(result.Model);
		}

		void OnShowBatches()
		{
			AskForSaveIfModifiedBeforeContinuing(
				TriggerDeselect
			);
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
			TriggerDeselect();
		}

		void OnLoadSelection(SaveModel model)
		{
			if (model == null)
			{
				TriggerDeselect();
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
			EditorGUILayoutExtensions.PushEnabled(BaseIsEnabled);
			{
				OnModelEditorUpdate();

				if (!CanEditDuringPlaymode())
				{
					if (lastIsPlayingOrWillChangePlaymode)
					{
						EditorGUILayout.HelpBox("Cannot edit in playmode.", MessageType.Info);
						return;
					}
					if (0 < frameDelayRemaining) return; // Adding a helpbox messes with this...
				}

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
			EditorGUILayoutExtensions.PopEnabled();
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
					if (GUILayout.Button("B", EditorStyles.toolbarButton, GUILayout.Width(16f))) OnShowBatches();
				}
				GUILayout.EndHorizontal();

				var ignoredCount = 0;
				modelSelectorScroll.VerticalScroll = GUILayout.BeginScrollView(modelSelectorScroll.VerticalScroll);
				{
					var isAlternate = false;
					foreach (var model in modelList.OrderBy(m => m.Id.Value))
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

					var modelName = Shorten(modelId, 8, "< No Id >");

					GUILayout.BeginHorizontal();
					{
						var isIgnored = model.Ignore.Value;
						var labelStyle = new GUIStyle(EditorStyles.boldLabel);
						labelStyle.normal.textColor = isIgnored ? SubLightEditorConfig.Instance.SharedModelEditorModelsEntryLabelIgnoredColor : labelStyle.normal.textColor;
						labelStyle.fixedHeight = 18;

						var metaName = string.IsNullOrEmpty(model.Id.Value) ? "< No Id >" : model.Id.Value;
						if (32 < metaName.Length) metaName = metaName.Substring(0, 29) + "...";
						GUILayout.Label(new GUIContent(metaName, "Id for this model."), labelStyle);
					}
					GUILayout.EndHorizontal();

					GUILayout.Space(-4f);

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

		void OnModelEditorUpdate(float delta = 0f)
		{
			if ((EditorApplication.isPlayingOrWillChangePlaymode && !CanEditDuringPlaymode()) || EditorApplication.isPlayingOrWillChangePlaymode != lastIsPlayingOrWillChangePlaymode)
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
		protected void RegisterToolbar(ModelEditorTab<T, M> tab)
		{
			toolbars.Add(tab);
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
		protected virtual M CreateModel(string id, string name)
		{
			var model = SaveLoadService.Create<M>(id);
			AssignModelId(model, id);
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
					GUILayout.Label("Select a model to begin editing or run a batch operation");
					onDraw = nullModel => DrawBatchEditor();
				}
				else
				{
					var metaName = string.IsNullOrEmpty(model.Id.Value) ? "< No Id > " : model.Id.Value;

					GUILayout.Label(metaName, GUILayout.ExpandWidth(false));

					switch (toolbars.Count)
					{
						case 0: break;
						case 1: onDraw = toolbars.First().Gui; break;
						default: onDraw = OnDrawToolbar(); break;
					}

					GUILayout.FlexibleSpace();

					OnDrawPreSettings(model);

					if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(ToolbarWidth))) ShowSettingsDialog();

					EditorGUILayoutExtensions.PushEnabled(modelAlwaysAllowSaving.Value || ModelSelectionModified || (!Event.current.shift && Event.current.control));
					{
						var saveContent = ModelSelectionModified ? new GUIContent("*Save*", "There are unsaved changes.") : new GUIContent("Save", "There are no unsaved changes.");
						if (GUILayout.Button(saveContent, EditorStyles.toolbarButton, GUILayout.Width(ToolbarWidth))) Save(null);
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
			}
			GUILayout.EndHorizontal();

			if (onDraw != null) onDraw(model);
		}

		protected virtual bool CanEditDuringPlaymode()
		{
			return false;
		}
		#endregion

		#region Batch Operations
		void DrawBatchEditor()
		{
			modelBatchScroll.VerticalScroll = GUILayout.BeginScrollView(modelBatchScroll.VerticalScroll);
			{
				switch (batchProgress.State)
				{
					case BatchProgress.States.NeverRun:
						DrawBatchEditorBrowsing();
						break;
					case BatchProgress.States.InProgress:
					case BatchProgress.States.Complete:
						DrawBatchEditorShared();
						break;
					default:
						EditorGUILayout.HelpBox("Unknown batch progress: " + batchProgress.State, MessageType.Error);
						break;
				}
			}
			GUILayout.EndScrollView();
		}

		void DrawBatchEditorBrowsing()
		{
			if (batchCache.Entries.None())
			{
				EditorGUILayout.HelpBox("No batch operations were found, use the BatchModelOperation attribute to define a new one.", MessageType.Info);
				return;
			}

			foreach (var batch in batchCache.Entries)
			{
				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					GUILayout.BeginHorizontal();
					{
						GUILayout.BeginVertical();
						{
							GUILayout.Label(batch.Name, EditorStyles.boldLabel);
							GUILayout.Label(batch.Description);
						}
						GUILayout.EndVertical();

						EditorGUILayoutExtensions.PushEnabled(modelListStatus == RequestStatus.Success);
						{
							const float RunBatchHeight = 48f;
							const float RunBatchWidth = 64f;
							EditorGUILayoutExtensions.PushColorValidation(Color.red);
							{
								if (GUILayout.Button("Run", EditorStyles.miniButtonLeft, GUILayout.Height(RunBatchHeight), GUILayout.Width(RunBatchWidth))) RunBatchOperation(batch, true);
							}
							EditorGUILayoutExtensions.PopColorValidation();
							if (GUILayout.Button("Test", EditorStyles.miniButtonRight, GUILayout.Height(RunBatchHeight), GUILayout.Width(RunBatchWidth))) RunBatchOperation(batch, false);
						}
						EditorGUILayoutExtensions.PopEnabled();
					}
					GUILayout.EndHorizontal();

					GUILayout.Space(4f);
				}
				GUILayout.EndVertical();
			}
		}

		void DrawBatchEditorShared()
		{
			var statusCount = "[ " + batchProgress.ModelsCompleted.Count + " / " + (batchProgress.ModelsCompleted.Count + batchProgress.ModelsRemaining.Count) + " ]";
			var statusType = batchProgress.Write ? "Writing" : "Testing";
			var status = statusType + " operation \"" + batchProgress.Operation.Name + "\" is "+batchProgress.State+" - "+statusCount;

			var enabled = batchProgress.ModelsRemaining.Count == 0 && batchProgress.ModelProcessing == null;
			if (enabled && batchProgress.State != BatchProgress.States.Complete)
			{
				batchProgress.State = BatchProgress.States.Complete;
				batchProgress.Log += "\n[ DONE ]\n\n"+ statusType + " completed with " + batchProgress.ErrorCount + " error(s)";
				modelBatchScroll.Value = float.MaxValue;
				OnLoadList();
			}

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(status);
				EditorGUILayoutExtensions.PushEnabled(enabled);
				{
					if (GUILayout.Button("Acknowledge", GUILayout.Width(120f)))
					{
						batchProgress = BatchProgress.Default;
					}
				}
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.TextArea(batchProgress.Log);

			if (!enabled && batchProgress.ModelProcessing == null)
			{
				batchProgress.ModelProcessing = batchProgress.ModelsRemaining.First();
				batchProgress.ModelsRemaining.RemoveAt(0);

				SaveLoadService.Load<M>(
					batchProgress.ModelProcessing,
					OnBatchOperationLoaded
				);
			}
		}

		void OnBatchOperationLoaded(
			SaveLoadRequest<M> result
		)
		{
			if (result.Status != RequestStatus.Success)
			{
				batchProgress.ErrorCount++;
				var error = batchProgress.GetLogPrefix() + "Unable to load model for batch processing, ignoring...\n";
				Debug.LogError(error);
				batchProgress.Log += error;
				OnBatchOperationNext();
				return;
			}

			batchProgress.Operation.Run(
				result.TypedModel,
				OnBatchOperationRun,
				batchProgress.Write
			);
		}

		void OnBatchOperationRun(
			M typedModel,
			RequestResult result
		)
		{
			if (result.IsNotSuccess)
			{
				var error = batchProgress.GetLogPrefix() + "Returned error from batch operation: " + result.Message+"\n";
				Debug.LogError(error);
				batchProgress.Log += error;
				batchProgress.ErrorCount++;
				OnBatchOperationNext();
				return;
			}

			if (batchProgress.Write)
			{
				SaveLoadService.Save(
					typedModel,
					saveResult => OnBatchOperationSaved(saveResult, result),
					false
				);
			}
			else
			{
				OnBatchOperationSaved(
					null,
					result
				);
			}
		}

		void OnBatchOperationSaved(
			SaveLoadRequest<M>? result,
			RequestResult batchResult
		)
		{
			if (result.HasValue) // If null, it means we're not writing so don't check the result.
			{
				if (result.Value.Status != RequestStatus.Success)
				{
					batchProgress.ErrorCount++;
					var error = batchProgress.GetLogPrefix() + "Unable to save model for batch processing, ignoring...\n";
					Debug.LogError(error);
					batchProgress.Log += error;
					OnBatchOperationNext();
					return;
				}
			}

			var resultMessage = batchProgress.GetLogPrefix() + (string.IsNullOrEmpty(batchResult.Message) ? "Processed successfully..." : batchResult.Message) + "\n";
			batchProgress.Log += resultMessage;
			OnBatchOperationNext();
		}

		void OnBatchOperationNext()
		{
			if (batchProgress.ModelProcessing != null)
			{
				batchProgress.ModelsCompleted.Add(batchProgress.ModelProcessing);
			}

			batchProgress.ModelProcessing = null;

			modelBatchScroll.Value = float.MaxValue;

			Repaint();
		}


		void RunBatchOperation(
			BatchModelOperationCache<M>.Entry operation,
			bool write
		)
		{
			batchProgress = new BatchProgress();
			batchProgress.State = BatchProgress.States.InProgress;
			batchProgress.ModelsRemaining = modelList.ToList();
			batchProgress.ModelsCompleted = new List<SaveModel>();
			batchProgress.Operation = operation;
			batchProgress.Write = write;

			batchProgress.Log = "Beginning batch operation \""+operation.Name+"\" on "+batchProgress.ModelsRemaining.Count+" models.\n";

			OnBatchOperationNext();
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

		protected virtual void OnSettingsGui(Action close)
		{
			modelAlwaysAllowSaving.Value = EditorGUILayout.Toggle(new GUIContent("Always Allow Saving", "When enabled the 'Save' button always be clickable."), modelAlwaysAllowSaving.Value);
			SettingsGui();
		}

		#endregion

		#region Required
		// TODO: MAKE THIS NOT VIRTUAL
		protected virtual void AssignModelId(M model, string id)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentException("Cannot have null or empty id", nameof(id));
			model.Id.Value = id;
		}
		// TODO: MAKE THIS NOT VIRTUAL
		protected virtual string GetModelId(SaveModel model)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			return model.Id.Value;
		}

		protected abstract void AssignModelName(M model, string name);
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
			return toolbars[Mathf.Clamp(modelSelectedToolbar, 0, toolbars.Count - 1)].Gui;
		}

		protected virtual void OnDrawPreSettings(M model)
		{

		}

		protected void TriggerDeselect()
		{
			modelSelectedPath.Value = null;
			modelSelectionState.Value = ModelSelectionStates.Browsing;
			selectedStatus = RequestStatus.Failure;
			ModelSelection = null;
			ModelSelectionModified = false;
			Deselect();
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
				"There are unsaved changes to \"" + ModelSelection.Id.Value + "\", would you like to save them before continuing?",
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

		// TODO: Move this to some util?
		public string Shorten(
			string value,
			int maximumLength,
			string missingValue = "< Missing >"
		)
		{
			if (string.IsNullOrEmpty(value)) return missingValue;
			if (maximumLength < value.Length) return value.Substring(0, maximumLength) + "...";
			return value;
		}
  		#endregion
	}
}