﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class EncounterEditorWindow
	{
		class EncounterEditorLogCache
		{
			public string[] BustIdsInitialized;
			public string[] BustIdsMissingInitialization;
			public bool BustIdsNormalizationMismatch;
		}

		EditorPrefsFloat logsListScroll;

		EncounterEditorLogCache logCache;

		void LogsConstruct()
		{
			var currPrefix = KeyPrefix + "Logs";

			logsListScroll = new EditorPrefsFloat(currPrefix + "ListScroll");

			RegisterToolbar("Logs", LogsToolbar);
		}

		void LogsToolbar(EncounterInfoModel model)
		{
			if (ModelSelectionModified || logCache == null)
			{
				logCache = new EncounterEditorLogCache
				{
					BustIdsInitialized = GetAllBustIdsInitialized(model),
					BustIdsMissingInitialization = GetAllBustIdsMissingInitialization(model),
					BustIdsNormalizationMismatch = GetAllBustIds(model).Length != GetAllBustIdsNormalized(model).Length
				};
			}

			DrawLogsToolbar(model);
		}

		void DrawLogsToolbar(EncounterInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Log Count: " + model.Logs.All.Value.Count() + " |", GUILayout.ExpandWidth(false));
					GUILayout.Label("Append New Log", GUILayout.ExpandWidth(false));
					AppendNewLog(EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Log Type -", EncounterLogTypes.Unknown), model);
					GUILayout.Label("Hold 'Control' to rearrange entries or 'Shift' to delete them.", GUILayout.ExpandWidth(false));
				}
				GUILayout.EndHorizontal();

				if (model.Logs.All.Value.None(l => l.Beginning.Value))
				{
					EditorGUILayout.HelpBox("No \"Beginning\" log has been specified!", MessageType.Error);
				}
				if (model.Logs.All.Value.None(l => l.Ending.Value))
				{
					EditorGUILayout.HelpBox("No \"Ending\" log has been specified!", MessageType.Error);
				}
				if (logCache.BustIdsNormalizationMismatch) EditorGUILayout.HelpBox("Duplicate Bust Ids are detected when normalizing all Ids.", MessageType.Error);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			logsListScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, logsListScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					var deleted = string.Empty;
					var beginning = string.Empty;

					EncounterLogModel indexToBeginning = null;
					EncounterLogModel indexToEnding = null;
					EncounterLogModel indexSwap0 = null;
					EncounterLogModel indexSwap1 = null;

					var isMoving = Event.current.shift;
					var isDeleting = Event.current.control;

					var sorted = model.Logs.All.Value.OrderBy(l => l.Index.Value).ToList();
					var sortedCount = sorted.Count;
					EncounterLogModel lastLog = null;
					for (var i = 0; i < sortedCount; i++)
					{
						var log = sorted[i];
						var nextLog = (i + 1 < sortedCount) ? sorted[i + 1] : null;
						int currMoveDelta;

						if (OnLogBegin(i, sortedCount, model, log, isMoving, isDeleting, out currMoveDelta, ref beginning)) deleted = log.LogId;

						switch (currMoveDelta)
						{
							case 0:
								break;
							case -2:
								indexToBeginning = log;
								break;
							case 2:
								indexToEnding = log;
								break;
							case -1:
							case 1:
								indexSwap0 = log;
								indexSwap1 = currMoveDelta == 1 ? nextLog : lastLog;
								break;
						}

						OnLog(model, log);
						OnLogEnd(model, log);

						lastLog = log;
					}

					if (!string.IsNullOrEmpty(deleted))
					{
						model.Logs.All.Value = model.Logs.All.Value.Where(l => l.LogId != deleted).ToArray();
					}
					else if (!string.IsNullOrEmpty(beginning))
					{
						foreach (var logs in model.Logs.All.Value)
						{

							logs.Beginning.Value = logs.LogId.Value == beginning;
						}
					}
					else if (indexToBeginning != null)
					{
						indexToBeginning.Index.Value = 0;
						var index = 1;
						foreach (var log in sorted.Where(l => l.LogId.Value != indexToBeginning.LogId.Value))
						{
							log.Index.Value = index;
							index++;
						}
					}
					else if (indexToEnding != null)
					{
						indexToEnding.Index.Value = sortedCount;
						var index = 0;
						foreach (var log in sorted.Where(l => l.LogId.Value != indexToEnding.LogId.Value))
						{
							log.Index.Value = index;
							index++;
						}
					}
					else if (indexSwap0 != null && indexSwap1 != null)
					{
						var swap0 = indexSwap0.Index.Value;
						var swap1 = indexSwap1.Index.Value;

						indexSwap0.Index.Value = swap1;
						indexSwap1.Index.Value = swap0;
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
			}
			GUILayout.EndScrollView();
		}

		string AppendNewLog(EncounterLogTypes logType, EncounterInfoModel infoModel)
		{
			if (logType == EncounterLogTypes.Unknown) return null;

			var isBeginning = infoModel.Logs.All.Value.Length == 0;
			var nextIndex = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Select(l => l.Index.Value).LastOrFallback(-1) + 1;
			switch (logType)
			{
				case EncounterLogTypes.Text:
					return NewEncounterLog<TextEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.KeyValue:
					return NewEncounterLog<KeyValueEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Switch:
					return NewEncounterLog<SwitchEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Button:
					return NewEncounterLog<ButtonEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Encyclopedia:
					return NewEncounterLog<EncyclopediaEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Event:
					return NewEncounterLog<EncounterEventEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Dialog:
					return NewEncounterLog<DialogEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Bust:
					return NewEncounterLog<BustEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Conversation:
					return NewEncounterLog<ConversationEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				default:
					Debug.LogError("Unrecognized EncounterLogType: " + logType);
					break;
			}
			return null;
		}

		T NewEncounterLog<T>(EncounterInfoModel model, int index, bool isBeginning) where T : EncounterLogModel, new()
		{
			var result = new T();
			result.Index.Value = index;
			result.LogId.Value = Guid.NewGuid().ToString();
			result.Beginning.Value = isBeginning;
			model.Logs.All.Value = model.Logs.All.Value.Append(result).ToArray();
			return result;
		}

		bool OnLogBegin(
			int count,
			int maxCount,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			bool isMoving,
			bool isDeleting,
			out int indexDelta,
			ref string beginning
		)
		{
			var deleted = false;
			indexDelta = 0;
			var isAlternate = count % 2 == 0;

			EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.cyan.NewH(0.5f), Color.cyan.NewH(0.6f), isAlternate);
			GUILayout.Space(2f);

			GUILayout.BeginVertical(EditorStyles.miniButton);

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushColor(Color.cyan.NewH(0.55f).NewS(0.4f));
				var header = "#" + (count + 1) + " | " + model.LogType + (model.HasName ? ".Name:" : ".LogId:");

				GUILayout.Label(new GUIContent(header, model.LogId.Value), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
				EditorGUILayout.SelectableLabel(model.HasName ? model.Name.Value : model.LogId.Value, EditorStyles.boldLabel);
				EditorGUILayoutExtensions.PopColor();
				if (isMoving)
				{
					GUILayout.Label("Click to Rearrange", GUILayout.ExpandWidth(false));
					EditorGUILayoutExtensions.PushEnabled(0 < count);
					if (GUILayout.Button("^", EditorStyles.miniButtonLeft, GUILayout.Width(30f)))
					{
						indexDelta = -1;
					}
					if (GUILayout.Button("^^", EditorStyles.miniButtonMid, GUILayout.Width(30f)))
					{
						indexDelta = -2;
					}
					EditorGUILayoutExtensions.PopEnabled();
					EditorGUILayoutExtensions.PushEnabled(count < maxCount - 1);
					if (GUILayout.Button("vv", EditorStyles.miniButtonMid, GUILayout.Width(30f)))
					{
						indexDelta = 2;
					}
					if (GUILayout.Button("v", EditorStyles.miniButtonRight, GUILayout.Width(30f)))
					{
						indexDelta = 1;
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				else if (isDeleting)
				{
					deleted = EditorGUILayoutExtensions.XButton(true);
				}
				else
				{
					if (GUILayout.Button(new GUIContent("Name", "Name this log so it can be referred to easily in log dropdowns."), EditorStyles.miniButtonLeft, GUILayout.Width(60f)))
					{
						FlexiblePopupDialog.Show(
							"Editing Log Name",
							new Vector2(400f, 22f),
							() => { model.Name.Value = EditorGUILayoutExtensions.TextDynamic(model.Name.Value); }
						);
					}
					if (GUILayout.Button(new GUIContent("Notes", "Add production notes for this log."), EditorStyles.miniButtonRight, GUILayout.Width(60f)))
					{
						FlexiblePopupDialog.Show(
							"Editing Log Notes",
							new Vector2(400f, 22f),
							() => { model.Notes.Value = EditorGUILayoutExtensions.TextDynamic(model.Notes.Value); }
						);
					}

					if (EditorGUILayout.ToggleLeft("Beginning", model.Beginning.Value, GUILayout.Width(70f)) && !model.Beginning.Value)
					{
						beginning = model.LogId;
					}
					model.Ending.Value = EditorGUILayout.ToggleLeft("Ending", model.Ending.Value, GUILayout.Width(60f));
				}
			}
			GUILayout.EndHorizontal();

			if (model.HasNotes) GUILayout.Label("Notes: " + model.Notes.Value);

			OnLogDuration(infoModel, model);

			return deleted;
		}

		void OnLog(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			if (model.CanFallback) OnFallbackLog(infoModel, model);

			switch (model.LogType)
			{
				case EncounterLogTypes.Text:
					OnTextLog(infoModel, model as TextEncounterLogModel);
					break;
				case EncounterLogTypes.KeyValue:
					OnKeyValueLog(infoModel, model as KeyValueEncounterLogModel);
					break;
				case EncounterLogTypes.Switch:
					OnSwitchLog(infoModel, model as SwitchEncounterLogModel);
					break;
				case EncounterLogTypes.Button:
					OnButtonLog(infoModel, model as ButtonEncounterLogModel);
					break;
				case EncounterLogTypes.Encyclopedia:
					OnEncyclopediaLog(infoModel, model as EncyclopediaEncounterLogModel);
					break;
				case EncounterLogTypes.Event:
					OnEncounterEventLog(infoModel, model as EncounterEventEncounterLogModel);
					break;
				case EncounterLogTypes.Dialog:
					OnDialogLog(infoModel, model as DialogEncounterLogModel);
					break;
				case EncounterLogTypes.Bust:
					OnBustLog(infoModel, model as BustEncounterLogModel);
					break;
				case EncounterLogTypes.Conversation:
					OnConversationLog(infoModel, model as ConversationEncounterLogModel);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized EncounterLogType: " + model.LogType, MessageType.Error);
					break;
			}
		}

		void OnLogDuration(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			if (!model.EditableDuration) return;
			model.Duration.Value = EditorGUILayout.FloatField("Duration", model.Duration);
		}

		#region Text Logs
		void OnTextLog(EncounterInfoModel infoModel, TextEncounterLogModel model)
		{
			EditorGUILayout.HelpBox("delete this", MessageType.Error);
			//model.Header.Value = EditorGUILayoutExtensions.TextDynamic("Header", model.Header.Value);
			//model.Message.Value = EditorGUILayoutExtensions.TextDynamic("Message", model.Message.Value);
			//OnLinearLog(infoModel, model);
		}
		#endregion

		#region KeyValue Logs
		void OnKeyValueLog(EncounterInfoModel infoModel, KeyValueEncounterLogModel model)
		{
			var targets = Enum.GetValues(typeof(KeyValueTargets)).Cast<KeyValueTargets>().ToList();
			var kvTypes = Enum.GetValues(typeof(KeyValueOperations)).Cast<KeyValueOperations>().ToList();

			var labels = new List<string>(new string[] { "- Select Operation -" });
			var onSelections = new Dictionary<int, Action>();
			var index = 1;

			foreach (var target in targets)
			{
				if (target == KeyValueTargets.Unknown) continue;
				labels.Add("--- " + target + " ---");
				index++;

				foreach (var kvType in kvTypes)
				{
					if (kvType == KeyValueOperations.Unknown) continue;
					labels.Add(target + "." + kvType);
					onSelections.Add(
						index,
						() => OnKeyValueLogSpawn(infoModel, model, target, kvType)
					);
					index++;
				}
			}

			var selection = 0;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Append New Key Value Operation: ", GUILayout.ExpandWidth(false));
				selection = EditorGUILayout.Popup(selection, labels.ToArray());
			}
			GUILayout.EndHorizontal();
			Action onSelection;
			if (onSelections.TryGetValue(selection, out onSelection)) onSelection();

			var deleted = string.Empty;
			var isAlternate = false;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					foreach (var operation in model.Operations.Value)
					{
						isAlternate = !isAlternate;

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.grey.NewV(0.5f), isAlternate);
						{
							if (OnKeyValueLogHeader(infoModel, model, operation)) deleted = operation.OperationId.Value;
							switch (operation.Operation)
							{
								case KeyValueOperations.SetString:
									OnKeyValueLogSetString(infoModel, model, operation as SetStringOperationModel);
									break;
								case KeyValueOperations.SetBoolean:
									OnKeyValueLogSetBoolean(infoModel, model, operation as SetBooleanOperationModel);
									break;
								default:
									Debug.LogError("Unrecognized KeyValueOperation: " + operation.Operation);
									break;
							}
						}
						EditorGUILayoutExtensions.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Operations.Value = model.Operations.Value.Where(kv => kv.OperationId != deleted).ToArray();
			}
		}

		bool OnKeyValueLogHeader(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueOperationModel operation
		)
		{
			var deleted = false;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(operation.Operation + ":", GUILayout.ExpandWidth(false));
				operation.Target.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Target -", operation.Target.Value);
				deleted = EditorGUILayoutExtensions.XButton();
			}
			GUILayout.EndHorizontal();
			return deleted;
		}

		void OnKeyValueLogSetString(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			SetStringOperationModel operation
		)
		{
			operation.Key.Value = EditorGUILayout.TextField("Key", operation.Key.Value);
			operation.Value.Value = EditorGUILayoutExtensions.TextDynamic("Value", operation.Value.Value);
		}

		void OnKeyValueLogSetBoolean(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			SetBooleanOperationModel operation
		)
		{
			operation.Key.Value = EditorGUILayout.TextField("Key", operation.Key.Value);
			operation.Value.Value = EditorGUILayoutExtensions.ToggleButton(new GUIContent("Value"), operation.Value.Value);
		}

		void OnKeyValueLogSpawn(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueTargets target,
			KeyValueOperations operation
		)
		{
			var guid = Guid.NewGuid().ToString();
			switch (operation)
			{
				case KeyValueOperations.SetString:
					var setString = new SetStringOperationModel();
					setString.OperationId.Value = guid;
					setString.Target.Value = target;
					model.Operations.Value = model.Operations.Value.Append(setString).ToArray();
					break;
				case KeyValueOperations.SetBoolean:
					var setBoolean = new SetBooleanOperationModel();
					setBoolean.OperationId.Value = guid;
					setBoolean.Target.Value = target;
					model.Operations.Value = model.Operations.Value.Append(setBoolean).ToArray();
					break;
				default:
					Debug.LogError("Unrecognized KeyValueOperation: " + operation);
					break;
			}
		}
		#endregion

		#region Switch Logs
		void OnSwitchLog(
			EncounterInfoModel infoModel,
			SwitchEncounterLogModel model
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				new GUIContent("Append New Switch"),
				null,
				infoModel,
				model,
				existingSelection => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, existingSelection)),
				newSelection => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, AppendNewLog(newSelection, infoModel))),
				EncounterLogBlankHandling.None,
				EncounterLogMissingHandling.None,
				"- Select Target Log -",
				"< Blank >"
			);

			OnEdgedLog<SwitchEncounterLogModel, SwitchEdgeModel>(infoModel, model, OnSwitchLogEdge);
		}

		void OnSwitchLogSpawn(
			SwitchEdgeModel edge,
			string targetLogId
		)
		{
			edge.Entry.NextLogId.Value = targetLogId;
		}

		void OnSwitchLogEdge(
			EncounterInfoModel infoModel,
			SwitchEncounterLogModel model,
			SwitchEdgeModel edge
		)
		{
			var entry = edge.Entry;

			EditorGUILayoutEncounter.LogPopup(
				new GUIContent("Target Log"),
				entry.NextLogId.Value,
				infoModel,
				model,
				existingSelection => entry.NextLogId.Value = existingSelection,
				newSelection => entry.NextLogId.Value = AppendNewLog(newSelection, infoModel),
				EncounterLogBlankHandling.FallsThrough,
				EncounterLogMissingHandling.Error,
				"- Select Target Log -"
			);

			EditorGUILayoutValueFilter.Field(
				new GUIContent("Filtering", "Passing this filter is required to continue to the target log."),
				entry.Filtering
			);
		}
		#endregion

		#region Button Logs
		void OnButtonLog(
			EncounterInfoModel infoModel,
			ButtonEncounterLogModel model
		)
		{
			var wasStyle = model.Style.Value;
			var newStyle = EditorGUILayoutExtensions.HelpfulEnumPopup(
				"Style",
				"- Select Button Style -",
				model.Style.Value
			);

			if (wasStyle != newStyle)
			{
				if (wasStyle != ConversationButtonStyles.Unknown)
				{
					// Value has changed, and not to Unknown, so we warn the user data may be lost.
					if (EditorUtility.DisplayDialog(
						"Overwrite Button Style",
						"Changing the button style will overwrite style specific data, and cannot be recovered.",
						"Continue",
						"Cancel"
					))
					{
						OnButtonLogApplyStyle(infoModel, model, newStyle);
					}
					throw new ExitGUIException(); // We do this because opening a dialog messes everything up for some reason...
				}
				OnButtonLogApplyStyle(infoModel, model, newStyle);
			}

			switch (model.Style.Value)
			{
				case ConversationButtonStyles.Conversation:
					OnButtonLogBustStyle(infoModel, model);
					break;
				case ConversationButtonStyles.Unknown:
					EditorGUILayout.HelpBox("A style must be specified for this button.", MessageType.Error);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized Style: " + model.Style.Value, MessageType.Error);
					break;
			}

			EditorGUILayoutEncounter.LogPopup(
				new GUIContent("Append New Button"),
				null,
				infoModel,
				model,
				existingSelection => OnEdgedLogSpawn(model, result => OnButtonLogSpawn(result, existingSelection)),
				newSelection => OnEdgedLogSpawn(model, result => OnButtonLogSpawn(result, AppendNewLog(newSelection, infoModel))),
				EncounterLogBlankHandling.None,
				EncounterLogMissingHandling.None,
				"- Select Target Log -",
				"< Blank >"
			);

			OnEdgedLog<ButtonEncounterLogModel, ButtonEdgeModel>(infoModel, model, OnButtonLogEdge);
		}

		void OnButtonLogBustStyle(
			EncounterInfoModel infoModel,
			ButtonEncounterLogModel model
		)
		{
			var style = model.ConversationStyle.Value;

			style.Theme = EditorGUILayoutExtensions.HelpfulEnumPopup(
				"Theme",
				"- Select Button Theme -",
				style.Theme
			);

			if (style.Theme == ConversationThemes.Unknown) EditorGUILayout.HelpBox("A theme must be specified.", MessageType.Error);

			model.ConversationStyle.Value = style;
		}

		void OnButtonLogApplyStyle(
			EncounterInfoModel infoModel,
			ButtonEncounterLogModel model,
			ConversationButtonStyles style
		)
		{
			var styleApplied = true;
			Action onApplyStyle = null;

			switch (style)
			{
				case ConversationButtonStyles.Conversation:
					onApplyStyle = () => model.ConversationStyle.Value = ButtonEncounterLogModel.ConversationStyleBlock.Default;
					break;
				default:
					Debug.LogError("Unrecognized Style: " + style);
					styleApplied = false;
					break;
			}

			if (styleApplied)
			{
				// Reset other styles here.
				model.ConversationStyle.Value = default(ButtonEncounterLogModel.ConversationStyleBlock);

				if (onApplyStyle == null) Debug.LogError("onApplyStyle cannot be null");
				else onApplyStyle();

				model.Style.Value = style;
			}
		}

		void OnButtonLogSpawn(
			ButtonEdgeModel edge,
			string targetLogId
		)
		{
			edge.Entry.NextLogId.Value = targetLogId;
		}

		void OnButtonLogEdge(
			EncounterInfoModel infoModel,
			ButtonEncounterLogModel model,
			ButtonEdgeModel edge
		)
		{
			var entry = edge.Entry;

			entry.Message.Value = EditorGUILayout.TextField("Message", entry.Message.Value);

			var hasBlankOrMissingNextId = false;
			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutEncounter.LogPopup(
					new GUIContent("Target Log"),
					entry.NextLogId.Value,
					infoModel,
					model,
					existingSelection => entry.NextLogId.Value = existingSelection,
					newSelection => entry.NextLogId.Value = AppendNewLog(newSelection, infoModel),
					EncounterLogBlankHandling.None,
					EncounterLogMissingHandling.None,
					out hasBlankOrMissingNextId,
					"- Select Target Log -"
				);

				entry.NotAutoUsed.Value = !EditorGUILayout.ToggleLeft(new GUIContent("Auto Used", "When this button is pressed, automatically set it to appear used the next time around."), !entry.NotAutoUsed.Value, GUILayout.Width(74f));
				entry.AutoDisableInteractions.Value = EditorGUILayout.ToggleLeft(new GUIContent("Auto Disable Interactions", "When this button is pressed, automatically disable future interactions the next time around."), entry.AutoDisableInteractions.Value, GUILayout.Width(152f));
				entry.AutoDisableEnabled.Value = EditorGUILayout.ToggleLeft(new GUIContent("Auto Disable", "When this button is pressed, automatically set this button to be disabled and invisible the next time around."), entry.AutoDisableEnabled.Value, GUILayout.Width(90f));
			}
			GUILayout.EndHorizontal();

			if (hasBlankOrMissingNextId)
			{
				if (string.IsNullOrEmpty(entry.NextLogId.Value)) EditorGUILayout.HelpBox("Specifying no log will fall through to the current log's \"Next Log\".", MessageType.Info);
				else EditorGUILayout.HelpBox("This button's \"Target Log\" references missing Log Id: "+entry.NextLogId.Value, MessageType.Error);
			}

			EditorGUIExtensions.PauseChangeCheck();
			{
				edge.ShowFiltering.Value = EditorGUILayout.Foldout(edge.ShowFiltering.Value, "Filtering");
			}
			EditorGUIExtensions.UnPauseChangeCheck();

			if (edge.ShowFiltering.Value)
			{
				EditorGUILayoutValueFilter.Field(new GUIContent("Used Filtering", "If this filter returns true, the button will appear used."), entry.UsedFiltering);
				EditorGUILayoutValueFilter.Field(new GUIContent("Interactable Filtering", "If this filter returns true, the button will be interactable."), entry.InteractableFiltering);
				EditorGUILayoutValueFilter.Field(new GUIContent("Enabled Filtering", "If this filter returns true, the button will be enabled and visible."), entry.EnabledFiltering);
			}
		}
		#endregion

		#region Encyclopedia Logs
		void OnEncyclopediaLog(
			EncounterInfoModel infoModel,
			EncyclopediaEncounterLogModel model
		)
		{
			if (GUILayout.Button("Append New Encyclopedia Section")) OnEdgedLogSpawn(model, OnEncyclopediaLogSpawn);

			OnEdgedLog<EncyclopediaEncounterLogModel, EncyclopediaEdgeModel>(infoModel, model, OnEncyclopediaLogEdge);
		}

		void OnEncyclopediaLogSpawn(
			EncyclopediaEdgeModel edge
		)
		{
			edge.Entry.OrderWeight.Value = -1;
		}

		void OnEncyclopediaLogEdge(
			EncounterInfoModel infoModel,
			EncyclopediaEncounterLogModel model,
			EncyclopediaEdgeModel edge
		)
		{
			var entry = edge.Entry;
			entry.Title.Value = EditorGUILayoutExtensions.TextDynamic("Title", entry.Title.Value);
			entry.Header.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Header", "The section header, leave blank to indicate this is the introduction."), entry.Header.Value);
			entry.Body.Value = EditorGUILayoutExtensions.TextDynamic("Body", entry.Body.Value);
			entry.Priority.Value = EditorGUILayout.IntField(new GUIContent("Priority", "Higher priority sections will replace lower priority sections with the same header."), entry.Priority.Value);
			entry.OrderWeight.Value = EditorGUILayout.IntField(new GUIContent("Order Weight", "The order of this section in the article, lower weights appear first."), entry.OrderWeight.Value);
		}
		#endregion

		#region Event Logs
		void OnEncounterEventLog(
			EncounterInfoModel infoModel,
			EncounterEventEncounterLogModel model
		)
		{
			if (model.AlwaysHalting.Value || model.Edges.Any(e => e.Entry.IsHalting.Value))
			{
				EditorGUILayout.HelpBox("This log will halt until all events are complete.", MessageType.Info);
			}
			else if (model.Ending.Value)
			{
				EditorGUILayout.HelpBox("This log is non-halting and also the end log, events may complete after the encounter is complete.", MessageType.Warning);
			}

			model.AlwaysHalting.Value = EditorGUILayout.Toggle(
				new GUIContent("Is Halting", "Does the handler wait for the event to complete before it continues?"),
				model.AlwaysHalting.Value
			);

			var appendSelection = EditorGUILayoutExtensions.HelpfulEnumPopup(
				GUIContent.none,
				"- Append New Event -",
				EncounterEvents.Types.Unknown
			);

			if (appendSelection != EncounterEvents.Types.Unknown) OnEdgedLogSpawn(model, edge => OnEncounterEventLogSpawn(edge, appendSelection));

			OnEdgedLog<EncounterEventEncounterLogModel, EncounterEventEdgeModel>(infoModel, model, OnEncounterEventLogEdge);
		}

		void OnEncounterEventLogSpawn(
			EncounterEventEdgeModel edge,
			EncounterEvents.Types type
		)
		{
			var entry = edge.Entry;
			entry.EncounterEvent.Value = type;
		}

		void OnEncounterEventLogEdge(
			EncounterInfoModel infoModel,
			EncounterEventEncounterLogModel model,
			EncounterEventEdgeModel edge
		)
		{
			var entry = edge.Entry;

			switch (entry.EncounterEvent.Value)
			{
				case EncounterEvents.Types.Unknown:
					EditorGUILayout.HelpBox("No event type has been specified.", MessageType.Error);
					break;
				case EncounterEvents.Types.Custom:
					EditorGUILayout.HelpBox("Editing custom events is not supported yet.", MessageType.Warning);
					break;
				case EncounterEvents.Types.Debug:
					OnEncounterEventLogEdgeDebugLog(entry);
					break;
				case EncounterEvents.Types.ToolbarSelection:
					OnEncounterEventLogEdgeToolbarSelection(entry);
					break;
			}

			EditorGUILayoutValueFilter.Field(
				new GUIContent("Filtering", "These conditions must be met or the event will not be called"),
				entry.Filtering
			);

			if (model.AlwaysHalting.Value) EditorGUILayoutExtensions.PushColor(Color.gray);
			entry.IsHalting.Value = EditorGUILayout.Toggle(
				new GUIContent("Is Halting", "Does the handler wait for the event to complete before it continues?"),
				entry.IsHalting.Value
			);
			if (model.AlwaysHalting.Value) EditorGUILayoutExtensions.PopColor();
		}

		void OnEncounterEventLogEdgeDebugLog(
			EncounterEventEntryModel entry
		)
		{
			entry.KeyValues.SetEnum(
				EncounterEvents.Debug.EnumKeys.Severity,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Severity"),
					"- Select A Severity -",
					entry.KeyValues.GetEnum<EncounterEvents.Debug.Severities>(EncounterEvents.Debug.EnumKeys.Severity)
				)
			);
			entry.KeyValues.SetString(
				EncounterEvents.Debug.StringKeys.Message,
				EditorGUILayout.TextField(entry.KeyValues.GetString(EncounterEvents.Debug.StringKeys.Message))
			);
		}

		void OnEncounterEventLogEdgeToolbarSelection(
			EncounterEventEntryModel entry
		)
		{
			entry.KeyValues.SetEnum(
				EncounterEvents.ToolbarSelection.EnumKeys.Selection,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Selection"),
					"- No Change -",
					entry.KeyValues.GetEnum<ToolbarSelections>(EncounterEvents.ToolbarSelection.EnumKeys.Selection)
				)
			);

			entry.KeyValues.SetEnum(
				EncounterEvents.ToolbarSelection.EnumKeys.LockState,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Locking"),
					"- No Change -",
					entry.KeyValues.GetEnum<EncounterEvents.ToolbarSelection.LockStates>(EncounterEvents.ToolbarSelection.EnumKeys.LockState)
				)
			);
		}
		#endregion

		#region Dialog Logs
		void OnDialogLog(
			EncounterInfoModel infoModel,
			DialogEncounterLogModel model
		)
		{
			if (GUILayout.Button("Append New Dialog")) OnEdgedLogSpawn(model, OnDialogLogSpawn);

			OnEdgedLog<DialogEncounterLogModel, DialogEdgeModel>(infoModel, model, OnDialogLogEdge);
		}

		void OnDialogLogSpawn(
			DialogEdgeModel edge
		)
		{
			var entry = edge.Entry;

			entry.DialogType.Value = DialogTypes.Confirm;
			entry.DialogStyle.Value = DialogStyles.Neutral;
			// Nothing to do...
		}

		void OnDialogLogEdge(
			EncounterInfoModel infoModel,
			DialogEncounterLogModel model,
			DialogEdgeModel edge
		)
		{
			var entry = edge.Entry;

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Configuration");
				entry.DialogType.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
					//new GUIContent("Type"),
					GUIContent.none,
					"- Select A Type -",
					entry.DialogType.Value
				);

				entry.DialogStyle.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
					//new GUIContent("Style"),
					GUIContent.none,
					"- Select A Style -",
					entry.DialogStyle.Value
				);
			}
			GUILayout.EndHorizontal();

			entry.Title.Value = EditorGUILayout.TextField("Title", entry.Title.Value);
			entry.Message.Value = EditorGUILayoutExtensions.TextDynamic("Message", entry.Message.Value);

			switch (entry.DialogType.Value)
			{
				case DialogTypes.Confirm:
					OnDialogLogEdgeOption(
						infoModel,
						model,
						edge,
						RequestStatus.Success
					);
					break;
				case DialogTypes.ConfirmDeny:
					OnDialogLogEdgeOption(
						infoModel,
						model,
						edge,
						RequestStatus.Success
					);
					OnDialogLogEdgeOption(
						infoModel,
						model,
						edge,
						RequestStatus.Failure
					);
					break;
				case DialogTypes.ConfirmDenyCancel:
					OnDialogLogEdgeOption(
						infoModel,
						model,
						edge,
						RequestStatus.Success
					);
					OnDialogLogEdgeOption(
						infoModel,
						model,
						edge,
						RequestStatus.Failure
					);
					OnDialogLogEdgeOption(
						infoModel,
						model,
						edge,
						RequestStatus.Cancel
					);
					break;
				case DialogTypes.Unknown:
					EditorGUILayout.HelpBox("A valid DialogType must be specified.", MessageType.Error);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized DialogType " + entry.DialogType.Value, MessageType.Error);
					break;
			}

			EditorGUILayoutValueFilter.Field(
				new GUIContent("Filtering", "The first dialog to meet these conditions will be shown"),
				entry.Filtering
			);
		}

		void OnDialogLogEdgeOption(
			EncounterInfoModel infoModel,
			DialogEncounterLogModel model,
			DialogEdgeModel edge,
			RequestStatus option
		)
		{
			var entry = edge.Entry;

			GUILayout.BeginVertical(EditorStyles.helpBox);
			{
				var entryName = "Unknown";
				switch (option)
				{
					case RequestStatus.Success: entryName = "Confirm"; break;
					case RequestStatus.Failure: entryName = "Deny"; break;
					case RequestStatus.Cancel: entryName = "Cancel"; break;
				}

				GUILayout.Label(entryName + " Button", EditorStyles.boldLabel);
				ListenerProperty<string> text = null;
				ListenerProperty<string> nextId = null;

				switch (option)
				{
					case RequestStatus.Success:
						text = entry.SuccessText;
						nextId = entry.SuccessLogId;
						break;
					case RequestStatus.Failure:
						text = entry.FailureText;
						nextId = entry.FailureLogId;
						break;
					case RequestStatus.Cancel:
						text = entry.CancelText;
						nextId = entry.CancelLogId;
						break;
				}

				if (text == null || nextId == null)
				{
					EditorGUILayout.HelpBox("Unrecognized RequestStatus: " + option, MessageType.Error);
				}
				else
				{
					GUILayout.BeginHorizontal();
					{
						text.Value = EditorGUILayout.TextField("Text", text.Value);
						var noText = string.IsNullOrEmpty(text.Value);
						if (noText) EditorGUILayoutExtensions.PushColor(Color.gray);
						GUILayout.Label(new GUIContent(noText ? "Not Overriding" : "Overriding", "Leave blank to use the default values for dialog button text."), GUILayout.ExpandWidth(false));
						if (noText) EditorGUILayoutExtensions.PopColor();
					}
					GUILayout.EndHorizontal();
					EditorGUILayoutEncounter.LogPopup(
						new GUIContent("Target Log"),
						nextId.Value,
						infoModel,
						model,
						existingSelection => nextId.Value = existingSelection,
						newSelection => nextId.Value = AppendNewLog(newSelection, infoModel),
						EncounterLogBlankHandling.FallsThrough,
						EncounterLogMissingHandling.Error,
						"- Select Target Log -"
					);
				}
			}
			GUILayout.EndVertical();
		}
		#endregion

		#region Bust Logs
		void OnBustLog(
			EncounterInfoModel infoModel,
			BustEncounterLogModel model
		)
		{
			if (1 < model.Edges.Count(e => e.Entry.BustEvent.Value == BustEntryModel.Events.Focus))
			{
				EditorGUILayout.HelpBox("Multiple Focus events will cause halting issues.", MessageType.Error);
			}
			if (model.Edges.Any(e => e.Entry.BustEvent.Value == BustEntryModel.Events.Focus && !e.Entry.FocusInfo.Value.Instant))
			{
				EditorGUILayout.HelpBox("This log will halt until all busts are focused.", MessageType.Info);
			}

			var appendOptions = new List<string>
			{
				"- Select Bust Event -",
				BustEntryModel.Events.Initialize.ToString(),
				"--- Focus ---"
			};
			var appendOptionsRaw = new List<string>(appendOptions);

			var initializeIndex = 1;
			var focusBeginIndex = appendOptions.Count();

			foreach (var bustId in logCache.BustIdsInitialized.OrderBy(i => i))
			{
				appendOptions.Add(bustId + ".Focus");
				appendOptionsRaw.Add(bustId);
			}
			foreach (var bustId in logCache.BustIdsMissingInitialization.OrderBy(i => i))
			{
				appendOptions.Add(bustId + ".Focus < Not Initialized >");
				appendOptionsRaw.Add(bustId);
			}

			var appendResult = EditorGUILayout.Popup(0, appendOptions.ToArray());

			if (appendResult == initializeIndex) OnBustLogEdgeNameNewInitialize(infoModel, model);
			else if (focusBeginIndex <= appendResult) OnEdgedLogSpawn(model, edge => OnBustLogSpawnFocus(appendOptionsRaw[appendResult], edge));

			OnEdgedLog<BustEncounterLogModel, BustEdgeModel>(infoModel, model, OnBustLogEdge);
		}

		void OnBustLogEdgeNameNewInitialize(
			EncounterInfoModel infoModel,
			BustEncounterLogModel model
		)
		{
			TextDialogPopup.Show(
				"New Bust Initialization",
				value =>
				{
					if (string.IsNullOrEmpty(value))
					{
						Debug.LogError("Null or empty BustIds are not allowed.");
						return;
					}
					if (logCache.BustIdsInitialized.Contains(value)) Debug.LogWarning("Creating duplicate Bust initialization for Bust Id: " + value);
					else if (GetAllBustIdsNormalized(infoModel).Contains(value.ToLower()))
					{
						Debug.LogError("BustIds that are duplicates after normalization of casing are not allowed.");
						return;
					}
					OnEdgedLogSpawn(model, edge => OnBustLogSpawnInitialize(value, edge));
				},
				doneText: "Create",
				description: "Enter a unique name for this Bust. Reminder: Busts should idealy be initialized only once per encounter."
			);
		}

		void OnBustLogSpawnInitialize(
			string bustId,
			BustEdgeModel edge
		)
		{
			var entry = edge.Entry;
			entry.BustId.Value = bustId;
			entry.BustEvent.Value = BustEntryModel.Events.Initialize;
			entry.InitializeInfo.Value = BustEntryModel.InitializeBlock.Default;
		}

		void OnBustLogSpawnFocus(
			string bustId,
			BustEdgeModel edge
		)
		{
			edge.Entry.BustId.Value = bustId;
			edge.Entry.BustEvent.Value = BustEntryModel.Events.Focus;
			edge.Entry.FocusInfo.Value = BustEntryModel.FocusBlock.Default;
		}

		void OnBustLogEdge(
			EncounterInfoModel infoModel,
			BustEncounterLogModel model,
			BustEdgeModel edge
		)
		{
			var entry = edge.Entry;

			if (entry.BustEvent.Value == BustEntryModel.Events.Initialize)
			{
				if (1 < model.Edges.Select(e => e.Entry).Where(e => e.BustId.Value == entry.BustId.Value && e.BustEvent.Value == BustEntryModel.Events.Initialize).Count())
				{
					EditorGUILayout.HelpBox("This Bust Id is initialized multiple times within the same log.", MessageType.Error);
				}
			}

			var targetOptions = new List<string> { logCache.BustIdsInitialized.Contains(entry.BustId.Value) ? entry.BustId.Value : entry.BustId.Value + " < Not Initialized >" };
			var targetOptionsRaw = new List<string> { entry.BustId.Value };

			foreach (var bustId in logCache.BustIdsInitialized.OrderBy(i => i))
			{
				if (bustId == entry.BustId.Value) continue;
				targetOptions.Add(bustId);
				targetOptionsRaw.Add(bustId);
			}
			foreach (var bustId in logCache.BustIdsMissingInitialization.OrderBy(i => i))
			{
				if (bustId == entry.BustId.Value) continue;
				targetOptions.Add(bustId + " < Not Initialized >");
				targetOptionsRaw.Add(bustId);
			}

			GUILayout.BeginHorizontal();
			{
				var targetResult = EditorGUILayout.Popup(new GUIContent("Bust Id"), 0, targetOptions.ToArray());
				if (targetResult != 0) entry.BustId.Value = targetOptionsRaw[targetResult];
				if (GUILayout.Button("Rename", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) OnBustLogEdgeRenameBustId(infoModel, entry.BustId.Value);
			}
			GUILayout.EndHorizontal();
			if (string.IsNullOrEmpty(entry.BustId.Value)) EditorGUILayout.HelpBox("A Bust Id must be specified.", MessageType.Error);

			switch (entry.BustEvent.Value)
			{
				case BustEntryModel.Events.Initialize: OnBustLogEdgeInitialize(entry); break;
				case BustEntryModel.Events.Focus: OnBustLogEdgeFocus(entry); break;
				default: EditorGUILayout.HelpBox("Unrecognized BustEvent: " + entry.BustEvent.Value, MessageType.Error); break;
			}
		}

		void OnBustLogEdgeRenameBustId(
			EncounterInfoModel infoModel,
			string bustId
		)
		{
			var changeCount = GetBustIdCount(infoModel, bustId);
			TextDialogPopup.Show(
				"Rename Bust Id: " + bustId,
				value =>
				{
					if (bustId == value)
					{
						Debug.LogWarning("Bust Id is already " + bustId);
						return;
					}
					if (string.IsNullOrEmpty(value))
					{
						Debug.LogError("Null or empty BustIds are not allowed.");
						return;
					}
					var collisionCount = GetBustIdCount(infoModel, value);
					if (0 < collisionCount)
					{
						var collisionConfirm = EditorUtility.DisplayDialog(
							"Bust Id Collision Detected",
							"A Bust Id \"" + value + "\" already exists, with " + collisionCount + " entry(s). Are you sure you went to continue renaming this Bust Id?",
							"Yes",
							"No"
						);
						if (collisionConfirm) OnBustLogEdgeRenameBustIdComplete(infoModel, bustId, value, changeCount);
						return;
					}
					if (bustId.ToLower() != value.ToLower() && GetAllBustIdsNormalized(infoModel).Contains(value.ToLower()))
					{
						Debug.LogError("BustIds that are duplicates after normalization of casing are not allowed.");
						return;
					}
					OnBustLogEdgeRenameBustIdComplete(infoModel, bustId, value, changeCount);
				},
				doneText: "Rename",
				description: "Renaming this Bust Id will affect " + changeCount + " Bust events."
			);
		}

		void OnBustLogEdgeRenameBustIdComplete(
			EncounterInfoModel infoModel,
			string oldBustId,
			string newBustId,
			int count
		)
		{
			foreach (var bust in infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(b => b.Entries.Value).Select(e => e.Entry).Where(e => e.BustId.Value == oldBustId))
			{
				bust.BustId.Value = newBustId;
			}
			Debug.Log("Renamed " + count + " Bust event(s) from \"" + oldBustId + "\" to \"" + newBustId + "\"");
		}

		void OnBustLogEdgeInitialize(
			BustEntryModel entry
		)
		{
			var block = entry.InitializeInfo.Value;

			GUILayout.Label("Title", EditorStyles.boldLabel);
			EditorGUILayoutExtensions.PushIndent();
			{
				block.TitleSource = EditorGUILayout.TextField("Source", block.TitleSource);
				block.TitleClassification = EditorGUILayout.TextField("Classification", block.TitleClassification);
			}
			EditorGUILayoutExtensions.PopIndent();

			GUILayout.Label("Transmission", EditorStyles.boldLabel);
			EditorGUILayoutExtensions.PushIndent();
			{
				block.TransmitionType = EditorGUILayout.TextField("Type", block.TransmitionType);
				block.TransmitionStrength = EditorGUILayout.TextField("Strength", block.TransmitionStrength);
				block.TransmitionStrengthIcon = EditorGUILayoutExtensions.HelpfulEnumPopup("Strength Bar", "- Select Strength -", block.TransmitionStrengthIcon);
			}
			EditorGUILayoutExtensions.PopIndent();

			GUILayout.Label("Placard", EditorStyles.boldLabel);
			EditorGUILayoutExtensions.PushIndent();
			{
				block.PlacardName = EditorGUILayout.TextField("Name", block.PlacardName);
				block.PlacardDescription = EditorGUILayout.TextField("Description", block.PlacardDescription);
			}
			EditorGUILayoutExtensions.PopIndent();

			GUILayout.Label("Avatar", EditorStyles.boldLabel);
			EditorGUILayoutExtensions.PushIndent();
			{
				block.AvatarType = EditorGUILayoutExtensions.HelpfulEnumPopup("Type", "- Select Avatar Type -", block.AvatarType);
				switch (block.AvatarType)
				{
					case BustEntryModel.AvatarTypes.Unknown: EditorGUILayout.HelpBox("An Avatar Type must be specified.", MessageType.Error); break;
					case BustEntryModel.AvatarTypes.Static: block = OnBustLogEdgeInitializeAvatarStatic(block); break;
					default: EditorGUILayout.HelpBox("Unrecognized AvatarType: " + block.AvatarType, MessageType.Error); break;
				}
			}
			EditorGUILayoutExtensions.PopIndent();

			entry.InitializeInfo.Value = block;
		}

		BustEntryModel.InitializeBlock OnBustLogEdgeInitializeAvatarStatic(BustEntryModel.InitializeBlock block)
		{
			block.AvatarStaticIndex = Mathf.Max(0, EditorGUILayout.IntField("Index", block.AvatarStaticIndex));
			return block;
		}

		void OnBustLogEdgeFocus(
			BustEntryModel entry
		)
		{
			var block = entry.FocusInfo.Value;

			block.Instant = EditorGUILayout.Toggle(new GUIContent("Instant", "Determines if the bust is shown instantly. If so, this encounter log will not halt."), block.Instant);

			entry.FocusInfo.Value = block;
		}
		#endregion

		#region Conversation Logs
		void OnConversationLog(
			EncounterInfoModel infoModel,
			ConversationEncounterLogModel model
		)
		{
			var appendSelection = EditorGUILayoutExtensions.HelpfulEnumPopup(
				GUIContent.none,
				"- Append a Conversation Entry -",
				ConversationTypes.Unknown,
				EnumExtensions.GetValues(ConversationTypes.AttachmentIncoming) // Attachments aren't supported yet...
			);

			switch (appendSelection)
			{
				case ConversationTypes.Unknown: break;
				case ConversationTypes.MessageIncoming:
				case ConversationTypes.MessageOutgoing:
					OnEdgedLogSpawn(model, edge => OnConversationLogSpawnMessage(appendSelection, edge));
					break;
				case ConversationTypes.Prompt:
					OnEdgedLogSpawn(model, OnConversationLogSpawnPrompt);
					break;
				default:
					Debug.LogError("Unrecognized ConversationType: " + appendSelection);
					break;

			}

			OnEdgedLog<ConversationEncounterLogModel, ConversationEdgeModel>(infoModel, model, OnConversationLogEdge);
		}

		void OnConversationLogSpawnMessage(
			ConversationTypes type,
			ConversationEdgeModel edge
		)
		{
			edge.Entry.ConversationType.Value = type;
			edge.Entry.MessageInfo.Value = ConversationEntryModel.MessageBlock.Default;
		}

		void OnConversationLogSpawnPrompt(ConversationEdgeModel edge)
		{
			edge.Entry.ConversationType.Value = ConversationTypes.Prompt;
			edge.Entry.PromptInfo.Value = ConversationEntryModel.PromptBlock.Default;
		}

		void OnConversationLogEdge(
			EncounterInfoModel infoModel,
			ConversationEncounterLogModel model,
			ConversationEdgeModel edge
		)
		{
			var entry = edge.Entry;

			switch (entry.ConversationType.Value)
			{
				case ConversationTypes.MessageIncoming:
				case ConversationTypes.MessageOutgoing:
					OnConversationLogEdgeMessage(entry);
					break;
				case ConversationTypes.Prompt:
					OnConversationLogEdgePrompt(entry);
					break;
				default: EditorGUILayout.HelpBox("Unrecognized ConversationEvent: " + entry.ConversationType.Value, MessageType.Error); break;
			}

			entry.Message.Value = EditorGUILayoutExtensions.TextDynamic(
				"Message",
				entry.Message.Value
			);
		}

		void OnConversationLogEdgeMessage(ConversationEntryModel entry)
		{
			var block = entry.MessageInfo.Value;

			var isIncoming = entry.ConversationType.Value == ConversationTypes.MessageIncoming;

			if (EditorGUILayoutExtensions.ToggleButtonArray("Alignment", isIncoming, "Incoming", "Outgoing") != isIncoming)
			{
				isIncoming = !isIncoming;
				entry.ConversationType.Value = isIncoming ? ConversationTypes.MessageIncoming : ConversationTypes.MessageOutgoing;
			}

			entry.MessageInfo.Value = block;
		}

		void OnConversationLogEdgePrompt(ConversationEntryModel entry)
		{
			var block = entry.PromptInfo.Value;

			GUILayout.BeginHorizontal();
			{
				block.Style = EditorGUILayoutExtensions.HelpfulEnumPopup("Button Configuration", "- Select Style -", block.Style);
				block.Theme = EditorGUILayoutExtensions.HelpfulEnumPopup(GUIContent.none, "- Select Theme -", block.Theme);
			}
			GUILayout.EndHorizontal();

			entry.PromptInfo.Value = block;
		}
		#endregion

		#region Edged Logs
		void OnEdgedLog<L, E>(
			EncounterInfoModel infoModel,
			L model,
			Action<EncounterInfoModel, L, E> edgeEditor
		)
			where L : IEdgedEncounterLogModel<E>
			where E : class, IEdgeModel
		{
			var deleted = string.Empty;
			var isAlternate = false;

			E indexSwap0 = null;
			E indexSwap1 = null;

			var isMoving = Event.current.shift;
			var isDeleting = Event.current.control;

			var sorted = model.Edges.OrderBy(l => l.EdgeIndex).ToList();
			var sortedCount = sorted.Count;
			E last = null;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					var hasDuplicateIds = false;
					var existingEdgeIds = new List<string>();
					foreach (var curr in sorted)
					{
						if (existingEdgeIds.Contains(curr.EdgeId))
						{
							hasDuplicateIds = true;
							break;
						}
						existingEdgeIds.Add(curr.EdgeId);
					}

					if (hasDuplicateIds)
					{
						EditorGUILayout.HelpBox("There are edges with duplicate Ids, unexpected behaviour may occur.", MessageType.Error);
					}

					for (var i = 0; i < sortedCount; i++)
					{
						var current = sorted[i];
						var next = (i + 1 < sortedCount) ? sorted[i + 1] : null;
						int currMoveDelta;

						isAlternate = !isAlternate;

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.grey.NewV(0.5f), isAlternate);
						{

							if (OnEdgedLogEdgeHeader(current.EdgeName, i, sortedCount, isMoving, isDeleting, out currMoveDelta)) deleted = current.EdgeId;

							if (currMoveDelta != 0)
							{
								indexSwap0 = current;
								indexSwap1 = currMoveDelta == 1 ? next : last;
							}

							edgeEditor(infoModel, model, current);

							last = current;
						}
						EditorGUILayoutExtensions.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Edges = model.Edges.Where(e => e.EdgeId != deleted).ToArray();
			}

			if (indexSwap0 != null && indexSwap1 != null)
			{
				var swap0 = indexSwap0.EdgeIndex;
				var swap1 = indexSwap1.EdgeIndex;

				indexSwap0.EdgeIndex = swap1;
				indexSwap1.EdgeIndex = swap0;
			}
		}

		void OnEdgedLogSpawn<E>(
			IEdgedEncounterLogModel<E> model,
			Action<E> initialize = null
		)
			where E : class, IEdgeModel, new()
		{
			var index = 0;
			if (model.Edges.Any())
			{
				index = model.Edges.OrderBy(e => e.EdgeIndex).Last().EdgeIndex + 1;
			}
			var result = new E();
			result.EdgeId = Guid.NewGuid().ToString();
			result.EdgeIndex = index;
			if (initialize != null) initialize(result);
			model.Edges = model.Edges.Append(result).ToArray();
		}

		bool OnEdgedLogEdgeHeader(
			string label,
			int count,
			int maxCount,
			bool isMoving,
			bool isDeleting,
			out int indexDelta
		)
		{
			var deleted = false;
			indexDelta = 0;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("#" + (count + 1) + " | " + label);
				if (isMoving)
				{
					GUILayout.Label("Click to Rearrange", GUILayout.ExpandWidth(false));
					EditorGUILayoutExtensions.PushEnabled(0 < count);
					if (GUILayout.Button("^", EditorStyles.miniButtonLeft, GUILayout.Width(30f)))
					{
						indexDelta = -1;
					}
					EditorGUILayoutExtensions.PopEnabled();
					EditorGUILayoutExtensions.PushEnabled(count < maxCount - 1);
					if (GUILayout.Button("v", EditorStyles.miniButtonRight, GUILayout.Width(30f)))
					{
						indexDelta = 1;
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				else if (isDeleting)
				{
					deleted = EditorGUILayoutExtensions.XButton(true);
				}
			}
			GUILayout.EndHorizontal();

			return deleted;
		}
		#endregion

		void OnFallbackLog(
			EncounterInfoModel infoModel,
			EncounterLogModel model
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				new GUIContent(model.RequiresFallbackLog ? "Next Log" : "Fallback Log", "The encounter will fallthrough to tthis log if not overridden."),
				model.FallbackLogId.Value,
				infoModel,
				model,
				existingSelection => model.FallbackLogId.Value = existingSelection,
				newSelection => model.FallbackLogId.Value = AppendNewLog(newSelection, infoModel),
				EncounterLogBlankHandling.SpecifiedByModel,
				EncounterLogMissingHandling.Error,
				model.RequiresFallbackLog ? "- Select Next Log -" : "- Select Fallback Log -"
			);
		}

		void OnLogEnd(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			GUILayout.EndVertical();
			GUILayout.Space(2f);
			EditorGUILayoutExtensions.EndVertical();

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				EditorGUILayoutExtensions.PushColor(Color.cyan.NewH(0.55f).NewS(0.7f));
				{
					GUILayout.Button(GUIContent.none, EditorStyles.radioButton);
					GUILayout.Button(GUIContent.none, EditorStyles.radioButton);
					GUILayout.Button(GUIContent.none, EditorStyles.radioButton);
				}
				EditorGUILayoutExtensions.PopColor();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}

		#region Utility
		string[] GetAllBustIds(EncounterInfoModel infoModel)
		{
			return infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(l => l.Edges).Select(e => e.Entry.BustId.Value).Distinct().ToArray();
		}

		string[] GetAllBustIdsNormalized(EncounterInfoModel infoModel)
		{
			//return infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(l => l.Edges).Where(e => e.Entry.BustId.Value != null).Select(e => e.Entry.BustId.Value.ToLower()).Distinct().ToArray();
			return infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(l => l.Edges).Select(e => e.Entry.BustId.Value.ToLower()).Distinct().ToArray();
		}

		string[] GetAllBustIdsInitialized(EncounterInfoModel infoModel)
		{
			return infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(l => l.Edges).Where(e => e.Entry.BustEvent.Value == BustEntryModel.Events.Initialize).Select(e => e.Entry.BustId.Value).Distinct().ToArray();
		}

		string[] GetAllBustIdsMissingInitialization(EncounterInfoModel infoModel)
		{
			var allIds = GetAllBustIds(infoModel);
			var allInitializedIds = GetAllBustIdsInitialized(infoModel);
			return allIds.Where(i => !allInitializedIds.Contains(i)).ToArray();
		}

		int GetBustIdCount(EncounterInfoModel infoModel, string bustId)
		{
			return infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(l => l.Edges).Select(e => e.Entry.BustId.Value).Count(i => i == bustId);
		}
		#endregion

		#region Exposed Utility

		#endregion
	}
}