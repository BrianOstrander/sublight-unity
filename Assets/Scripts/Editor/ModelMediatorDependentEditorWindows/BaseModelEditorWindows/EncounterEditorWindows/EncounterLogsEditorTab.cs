﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight
{
	public class EncounterLogsEditorTab : ModelEditorTab<EncounterEditorWindow, EncounterInfoModel>
	{
		static class LogStrings
		{
			public const string SelectOrCreateLog = "- Select Or Create Log -";
			public const string DefaultEndLogName = "< End Encounter >";
			public const string EdgeEntryAppendPrefix = "Append";
			public const string EdgeEntryInsertPrefix = "Insert";
		}

		static class LogFloats
		{
			public const float KeyValueOperationWidth = 80f;
			public const float AppendEntryWidthMaximum = 300f;
			public const float HighlightEntryDuration = 1f;
		}

		class EncounterEditorLogCache
		{
			public string EncounterId;
			public string[] BustIdsInitialized;
			public string[] BustIdsMissingInitialization;
			public bool BustIdsNormalizationMismatch;
		}

		class LogEdgeVisualOverride
		{
			public enum Controls
			{
				Unknown = 0,
				Static = 10,
				Decay = 20
			}

			public enum Effects
			{
				Unknown = 0,
				Highlight = 10
			}

			public string EdgeId;

			public Controls Control;
			public Effects Effect;

			public int FrameCount;

			public float Duration;
			public float DurationRemaining;

			public float InverseNormal { get { return DurationRemaining / Duration; } }
			public float Normal { get { return 1f - InverseNormal; } }
			public float QuartNormal { get { return Mathf.Pow(Normal, 4f); } }

			public LogEdgeVisualOverride(
				string edgeId,
				Controls control,
				Effects effect,
				float duration
			)
			{
				EdgeId = edgeId;
				Control = control;
				Effect = effect;
				Duration = duration;
				DurationRemaining = duration;
			}
		}

		enum LogsAppendSources
		{
			Unknown = 0,
			Automatic = 5,
			Toolbar = 10,
			LogFallback = 20,
			EdgeSpawn = 30,
			EdgeAssignment = 40
		}

		EditorPrefsFloat logsListScroll;
		EditorPrefsFloat logsStackScroll;
		EditorPrefsBool logsEdgeIndentsEnabled;
		EditorPrefsBool logsShowNameSource;
		EditorPrefsBool logsShowHaltingInfo;
		EditorPrefsBool logsShowHaltingWarnings;
		EditorPrefsBool logsJumpFromToolbarAppend;

		#region Log Stack Serialization
		EditorPrefsInt logsFocusedLogIdsIndex;
		EditorPrefsString logsFocusedLogIds;

		bool LogsIsFocusedOnStack { get { return logsFocusedLogIdsIndex.Value != -1; } }

		List<string> logsFocusedLogIdsStack;
		IEnumerable<string> LogsFocusedLogIdsStack
		{
			get
			{
				return logsFocusedLogIdsStack ?? (logsFocusedLogIdsStack = new List<string>((logsFocusedLogIds.Value ?? string.Empty).Split('|').Where(v => !string.IsNullOrEmpty(v))));
			}
			set
			{
				var serialized = string.Empty;
				if (value != null)
				{
					foreach (var logId in value) serialized += logId + "|";
				}
				logsFocusedLogIds.Value = serialized;
				logsFocusedLogIdsStack = value == null ? null : value.ToList();
			}
		}
		#endregion

		EncounterEditorLogCache logCache;
		Dictionary<string, Rect> logRectCache = new Dictionary<string, Rect>();

		#region Log Edge Visual Overrides
		List<LogEdgeVisualOverride> logEdgeVisualOverrides = new List<LogEdgeVisualOverride>();

		void LogAddEdgeVisualOverrideHighlight(string edgeId)
		{
			var existing = logEdgeVisualOverrides.FirstOrDefault(e => e.EdgeId == edgeId);
			if (existing == null)
			{
				logEdgeVisualOverrides.Add(
					new LogEdgeVisualOverride(
						edgeId,
						LogEdgeVisualOverride.Controls.Decay,
						LogEdgeVisualOverride.Effects.Highlight,
						LogFloats.HighlightEntryDuration
					)
				);
				return;
			}

			switch (existing.Control)
			{
				case LogEdgeVisualOverride.Controls.Static: break;
				case LogEdgeVisualOverride.Controls.Decay:
					existing.Duration = LogFloats.HighlightEntryDuration;
					existing.DurationRemaining = LogFloats.HighlightEntryDuration;
					break;
				default:
					Debug.LogError("Unrecognized EdgeVisualOverrideControl: " + existing.Control);
					break;
			}
		}
		#endregion

		public EncounterLogsEditorTab(EncounterEditorWindow window) : base(window, "Logs")
		{
			logsListScroll = new EditorPrefsFloat(TabKeyPrefix + "ListScroll");
			logsStackScroll = new EditorPrefsFloat(TabKeyPrefix + "StackScroll");
			logsEdgeIndentsEnabled = new EditorPrefsBool(TabKeyPrefix + "EdgeIndentsEnabled", true);
			logsShowNameSource = new EditorPrefsBool(TabKeyPrefix + "ShowNameSource");
			logsShowHaltingInfo = new EditorPrefsBool(TabKeyPrefix + "ShowHaltingInfo", true);
			logsShowHaltingWarnings = new EditorPrefsBool(TabKeyPrefix + "ShowHaltingWarnings", true);
			logsJumpFromToolbarAppend = new EditorPrefsBool(TabKeyPrefix + "JumpFromToolbarAppend", true);

			logsFocusedLogIdsIndex = new EditorPrefsInt(TabKeyPrefix + "FocusedLogsIdsIndex");
			logsFocusedLogIds = new EditorPrefsString(TabKeyPrefix + "FocusedLogIds");
		}

		#region Events
		public override void SettingsGui()
		{
			GUILayout.Label("Logs", EditorStyles.boldLabel);
			logsShowNameSource.Value = EditorGUILayout.Toggle(new GUIContent("Show Source of Log Names", "Prefixes the source of the name for a log, if it's an Id or form the actual Name field."), logsShowNameSource.Value);
			logsJumpFromToolbarAppend.Value = EditorGUILayout.Toggle(new GUIContent("Jump To Log When Created From Toolbar", "When enabled, the editor will focus logs when created from the top toolbar area."), logsJumpFromToolbarAppend.Value);
			logsEdgeIndentsEnabled.Value = EditorGUILayout.Toggle(new GUIContent("Edge Indenting", "Certain logs, like conversations, support indenting some edges for clarity."), logsEdgeIndentsEnabled.Value);
			GUILayout.Label("Tips and Warnings", EditorStyles.boldLabel);
			GUILayout.Label("Unless there are performance problems with the editor, these should be kept enabled.");
			logsShowHaltingInfo.Value = EditorGUILayout.Toggle(new GUIContent("Halting Info", "Show a tooltip if certain logs cause an encounter to halt gracefully."), logsShowHaltingInfo.Value);
			logsShowHaltingWarnings.Value = EditorGUILayout.Toggle(new GUIContent("Halting Warnings", "Show a warning if certain logs cause an encounter to halt in possibly dangerous ways."), logsShowHaltingWarnings.Value);
		}

		public override void Gui(EncounterInfoModel model)
		{
			if (Window.ModelSelectionModified || logCache == null || model.Id.Value != logCache.EncounterId)
			{
				logCache = new EncounterEditorLogCache
				{
					EncounterId = model.Id.Value,
					BustIdsInitialized = GetAllBustIdsInitialized(model),
					BustIdsMissingInitialization = GetAllBustIdsMissingInitialization(model),
					BustIdsNormalizationMismatch = GetAllBustIds(model).Length != GetAllBustIdsNormalized(model).Length
				};
			}

			DrawLogsToolbar(model);
		}

		public override void BeforeLoadSelection()
		{
			LogsFocusedLogIdsPop();
		}

		public override void AfterLoadSelection(EncounterInfoModel model)
		{
			if (string.IsNullOrEmpty(model.DefaultEndLogId.Value))
			{
				Debug.LogWarning("Default log id was never instantiated, doing that now");
				LogsCreateDefaultEndLog(model);
				return;
			}

			var defaultEndLog = model.Logs.GetLogFirstOrDefault(model.DefaultEndLogId.Value);

			if (defaultEndLog == null)
			{
				Debug.LogError("A default end log id \"" + model.DefaultEndLogId.Value + "\" is specified, but could not be found.");
				return;
			}

			// Eventually you can do some changes to the default end log here, if needed...
		}

		void LogsCreateDefaultEndLog(EncounterInfoModel model)
		{
			model.DefaultEndLogId.Value = AppendNewLog(EncounterLogTypes.Event, model, LogsAppendSources.Automatic);
			var endLogInstance = model.Logs.GetLogFirstOrDefault(model.DefaultEndLogId.Value);
			endLogInstance.Ending.Value = true;
			endLogInstance.Name.Value = LogStrings.DefaultEndLogName;
		}

		public override void EditorUpdate(float delta)
		{
			if (logEdgeVisualOverrides.None()) return;

			var newOverrides = new List<LogEdgeVisualOverride>();
			var wasUpdated = false;

			foreach (var current in logEdgeVisualOverrides)
			{
				current.FrameCount++;

				if (current.Control == LogEdgeVisualOverride.Controls.Static || current.FrameCount == 1)
				{
					newOverrides.Add(current);
					continue;
				}

				if (current.Control != LogEdgeVisualOverride.Controls.Decay)
				{
					Debug.LogError("Unrecognized EdgeVisualControl: " + current.Control);
					continue;
				}

				wasUpdated = true;

				current.DurationRemaining = Mathf.Max(0f, current.DurationRemaining - delta);
				if (Mathf.Approximately(0f, current.DurationRemaining)) continue;

				newOverrides.Add(current);
			}

			logEdgeVisualOverrides = newOverrides;

			if (wasUpdated) Window.Repaint();
		}
		#endregion

		void DrawLogsToolbar(EncounterInfoModel model)
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				EditorGUILayoutExtensions.PushEnabled(LogsIsFocusedOnStack);
				{
					if (GUILayout.Button(new GUIContent(SubLightEditorConfig.Instance.EncounterEditorLogToolbarLastImage), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
					{
						if (0 < logsFocusedLogIdsIndex.Value) LogsFocusedLogIdsOffsetIndex(-1);
						else LogsFocusedLogIdsPop();
					}

					EditorGUILayoutExtensions.PushEnabled(logsFocusedLogIdsIndex.Value < LogsFocusedLogIdsStack.Count() - 1);
					{
						if (GUILayout.Button(new GUIContent(SubLightEditorConfig.Instance.EncounterEditorLogToolbarNextImage), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) LogsFocusedLogIdsOffsetIndex(1);
					}
					EditorGUILayoutExtensions.PopEnabled();

					if (!LogsIsFocusedOnStack) GUILayout.Label("Hold 'control' to rearrange entries or 'alt' to delete them.", GUILayout.ExpandWidth(false));

					logsStackScroll.HorizontalScroll = GUILayout.BeginScrollView(logsStackScroll.HorizontalScroll, GUIStyle.none, GUIStyle.none);
					{
						GUILayout.BeginHorizontal();
						var currLogIdIndex = 0;
						foreach (var logId in LogsFocusedLogIdsStack)
						{
							var logIsFocused = logsFocusedLogIdsIndex.Value == currLogIdIndex;
							var log = model.Logs.GetLogFirstOrDefault(logId);
							var logName = EditorGUILayoutEncounter.GetReadableLogName(
								log == null ? EncounterLogTypes.Unknown : log.LogType,
								logId,
								log == null ? null : log.Name.Value,
								log == null
							);

							if (logIsFocused != GUILayout.Toggle(logIsFocused, new GUIContent(logName, logIsFocused ? "Log is currently focused." : "Jump to this log."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) && !logIsFocused)
							{
								LogsSetFocusedLogIdsIndex(currLogIdIndex);
							}

							currLogIdIndex++;
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndScrollView();
				}
				EditorGUILayoutExtensions.PopEnabled();

				EditorGUILayoutExtensions.PushEnabled(!LogsIsFocusedOnStack);
				{
					if (GUILayout.Button(new GUIContent("-", "Collapse all logs"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
					{
						foreach (var log in model.Logs.All.Value) log.Collapsed.Value = true;
						EditorGUIExtensions.ResetControls();
						LogsBustHeightCache();
					}
					if (GUILayout.Button(new GUIContent("+", "Collapse all logs"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
					{
						foreach (var log in model.Logs.All.Value) log.Collapsed.Value = false;
						EditorGUIExtensions.ResetControls();
						LogsBustHeightCache();
					}
				}
				EditorGUILayoutExtensions.PopEnabled();

				EditorGUILayoutEncounter.SelectLogPopup(
					LogsFocusedLogIdsPeek(),
					new GUIContent("Filter"),
					model,
					LogsFocusedLogIdsPush,
					() => LogsFocusedLogIdsPop(),
					EditorStyles.toolbarDropDown
				);

				var appendResult = EditorGUILayoutExtensions.HelpfulEnumPopupValue(
					"Create",
					EncounterLogTypes.Unknown,
					EnumExtensions.GetValues(EncounterLogTypes.Unknown).OrderBy(t => t.ToString()).Prepend(EncounterLogTypes.Unknown).ToArray(),
					EditorStyles.toolbarPopup,
					GUILayout.Width(48f)
				);

				if (appendResult != EncounterLogTypes.Unknown) AppendNewLog(appendResult, model, LogsAppendSources.Toolbar);
			}
			GUILayout.EndHorizontal();

			logsListScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, logsListScroll)).y;
			{
				EditorGUILayoutExtensions.PushBackgroundColor(SubLightEditorConfig.Instance.EncounterEditorLogBackgroundColor);
				GUILayout.BeginVertical(SubLightEditorConfig.Instance.EncounterEditorLogBackground, GUILayout.ExpandHeight(true));
				EditorGUILayoutExtensions.PopBackgroundColor();
				{
					EditorGUIExtensions.BeginChangeCheck();
					{
						var deleted = string.Empty;
						var beginning = string.Empty;

						EncounterLogModel indexToBeginning = null;
						EncounterLogModel indexToEnding = null;
						EncounterLogModel indexSwap0 = null;
						EncounterLogModel indexSwap1 = null;

						var isMoving = !Event.current.shift && Event.current.control;
						var isDeleting = !Event.current.shift && Event.current.alt;

						var sorted = model.Logs.All.Value.OrderBy(l => l.Index.Value).ToList();
						var sortedCount = sorted.Count;
						EncounterLogModel lastLog = null;

						var focusedLogId = string.Empty;

						if (-1 < logsFocusedLogIdsIndex.Value) focusedLogId = LogsFocusedLogIdsStack.ElementAtOrDefault(logsFocusedLogIdsIndex.Value);

						var checkForFocus = !string.IsNullOrEmpty(focusedLogId);
						var totalShown = 0;

						GUILayout.Space(4f);

						for (var i = 0; i < sortedCount; i++)
						{
							var log = sorted[i];

							Rect logRect;
							if (logRectCache.TryGetValue(log.LogId.Value, out logRect))
							{
								if (logRect.yMax < logsListScroll.Value || logsListScroll.Value < (logRect.yMin - Window.position.height))
								{
									GUILayout.Space(logRect.height);
									continue;
								}
							}

							totalShown++;

							if (checkForFocus && focusedLogId != log.LogId.Value) continue;

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

							if (Event.current.type == EventType.Repaint)
							{
								logRectCache[log.LogId.Value] = GUILayoutUtility.GetLastRect();
							}
						}

						//Debug.Log(totalShown);

						if (!string.IsNullOrEmpty(deleted))
						{
							model.Logs.All.Value = model.Logs.All.Value.Where(l => l.LogId != deleted).ToArray();
							LogsFocusedLogIdsPop();
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
					EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndScrollView();

			if (model.Logs.All.Value.None(l => l.Beginning.Value)) EditorGUILayout.HelpBox("No \"Beginning\" log has been specified!", MessageType.Error);
			if (model.Logs.All.Value.None(l => l.Ending.Value)) EditorGUILayout.HelpBox("No \"Ending\" log has been specified!", MessageType.Error);
			if (logCache.BustIdsNormalizationMismatch) EditorGUILayout.HelpBox("Duplicate Bust Ids are detected when normalizing all Ids.", MessageType.Error);
		}

		string AppendNewLog(EncounterLogTypes logType, EncounterInfoModel infoModel, LogsAppendSources source)
		{
			if (logType == EncounterLogTypes.Unknown) return null;

			Window.ModelSelectionModified = true;

			var isBeginning = infoModel.Logs.All.Value.Length == 0;
			var nextIndex = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Select(l => l.Index.Value).LastOrFallback(-1) + 1;
			string result = null;
			switch (logType)
			{
				case EncounterLogTypes.KeyValue:
					result = NewEncounterLog<KeyValueEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				case EncounterLogTypes.Switch:
					result = OnNewEncoutnerLog(NewEncounterLog<SwitchEncounterLogModel>(infoModel, nextIndex, isBeginning)).LogId.Value;
					break;
				case EncounterLogTypes.Button:
					result = NewEncounterLog<ButtonEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				case EncounterLogTypes.Encyclopedia:
					result = NewEncounterLog<EncyclopediaEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				case EncounterLogTypes.Event:
					result = NewEncounterLog<EncounterEventEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				case EncounterLogTypes.Dialog:
					result = NewEncounterLog<DialogEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				case EncounterLogTypes.Bust:
					result = NewEncounterLog<BustEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				case EncounterLogTypes.Conversation:
					result = NewEncounterLog<ConversationEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
					break;
				default:
					Debug.LogError("Unrecognized EncounterLogType: " + logType);
					break;
			}

			if (!string.IsNullOrEmpty(result))
			{
				switch (source)
				{
					case LogsAppendSources.Toolbar:
						if (logsJumpFromToolbarAppend.Value) LogsFocusedLogIdsPush(result);
						break;
				}
			}

			return result;
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

		SwitchEncounterLogModel OnNewEncoutnerLog(SwitchEncounterLogModel model)
		{
			model.SelectionMethod.Value = SwitchEncounterLogModel.SelectionMethods.FirstFilter;
			return model;
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
			var isCollapsed = !LogsIsFocusedOnStack && model.Collapsed.Value;
			var deleted = false;
			indexDelta = 0;
			var isAlternate = count % 2 == 0;

			EditorGUILayoutExtensions.BeginVertical(
				isCollapsed ? SubLightEditorConfig.Instance.EncounterEditorLogEntryCollapsedBackground : SubLightEditorConfig.Instance.EncounterEditorLogEntryBackground,
				SubLightEditorConfig.Instance.EncounterEditorLogEntryBackgroundColor,
				SubLightEditorConfig.Instance.EncounterEditorLogEntryBackgroundColor,
				isAlternate
			);

			GUILayout.BeginHorizontal();
			{
				var nameSourcePrefix = logsShowNameSource.Value ? (model.HasName ? ".Name:" : ".LogId:") : ":";
				var header = "#" + (count + 1) + " | " + model.LogType + nameSourcePrefix + " " + (model.HasName ? ("<b>" + model.Name.Value + "</b>") : Window.Shorten(model.LogId.Value, 8));
				GUILayout.Label(new GUIContent(header, model.LogId.Value), SubLightEditorConfig.Instance.EncounterEditorLogEntryIndex);
				if (isMoving)
				{
					GUILayout.Label(LogsIsFocusedOnStack ? "Cannot Rearrange While Focused" : "Click to Rearrange", GUILayout.ExpandWidth(false));

					EditorGUILayoutExtensions.PushEnabled(!LogsIsFocusedOnStack);
					{
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
					EditorGUILayoutExtensions.PopEnabled();
				}
				else if (isDeleting)
				{
					EditorGUILayoutExtensions.PushEnabled(infoModel.DefaultEndLogId.Value != model.LogId.Value);
					{
						deleted = EditorGUILayoutExtensions.XButton(true);
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				else
				{
					const float TitleOptionWidth = 42f;

					EditorGUILayoutExtensions.PushEnabled(infoModel.DefaultEndLogId.Value != model.LogId.Value);
					{
						if (GUILayout.Button(new GUIContent("Name", "Name this log so it can be referred to easily in log dropdowns."), EditorStyles.miniButtonLeft, GUILayout.Width(TitleOptionWidth)))
						{
							FlexiblePopupDialog.Show(
								"Editing Log Name",
								new Vector2(400f, 22f),
								close => { model.Name.Value = EditorGUILayout.TextField(model.Name.Value); }
							);
						}
					}
					EditorGUILayoutExtensions.PopEnabled();

					if (!isCollapsed)
					{
						if (GUILayout.Button(new GUIContent("Notes", "Add production notes for this log."), EditorStyles.miniButtonMid, GUILayout.Width(TitleOptionWidth)))
						{
							FlexiblePopupDialog.Show(
								"Editing Log Notes",
								new Vector2(400f, 22f * 3f),
								close => { model.Notes.Value = EditorGUILayoutExtensions.TextAreaWrapped(model.Notes.Value, GUILayout.ExpandHeight(true)); }
							);
						}
					}

					EditorGUIExtensions.PauseChangeCheck();
					{
						if (GUILayout.Button(new GUIContent("Copy Id", "Copies this entry's Log Id to the clipboard."), EditorStyles.miniButtonMid, GUILayout.Width(48f)))
						{
							EditorGUIUtility.systemCopyBuffer = model.LogId.Value;
						}

						EditorGUILayoutExtensions.PushEnabled(!LogsIsFocusedOnStack);
						{
							if (GUILayout.Button(new GUIContent("Filter", "Show only this log."), EditorStyles.miniButtonRight, GUILayout.Width(TitleOptionWidth)))
							{
								LogsFocusedLogIdsPush(model.LogId.Value);
							}
						}
						EditorGUILayoutExtensions.PopEnabled();
					}
					EditorGUIExtensions.UnPauseChangeCheck();

					if (!isCollapsed)
					{
						EditorGUILayoutExtensions.PushEnabled(infoModel.DefaultEndLogId.Value != model.LogId.Value);
						{
							if (EditorGUILayout.ToggleLeft("Beginning", model.Beginning.Value, GUILayout.Width(70f)) && !model.Beginning.Value)
							{
								beginning = model.LogId;
							}
							model.Ending.Value = EditorGUILayout.ToggleLeft("Ending", model.Ending.Value, GUILayout.Width(55f));
						}
						EditorGUILayoutExtensions.PopEnabled();
					}

					EditorGUIExtensions.PauseChangeCheck();
					{
						if (model.Collapsed.Value == EditorGUILayout.Toggle(!model.Collapsed.Value, EditorStyles.foldout, GUILayout.Width(14f)))
						{
							model.Collapsed.Value = !model.Collapsed.Value;
							EditorGUIExtensions.ResetControls();
							LogsBustHeightCache();
						}
					}
					EditorGUIExtensions.UnPauseChangeCheck();
				}
			}
			GUILayout.EndHorizontal();

			if (isDeleting && !isMoving) GUILayout.Space(9f);
			else GUILayout.Space(8f);

			if (isCollapsed) return deleted;

			if (model.HasNotes) GUILayout.Label("Notes: " + model.Notes.Value);

			OnLogDuration(infoModel, model);

			return deleted;
		}

		void OnLog(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			if (!LogsIsFocusedOnStack && model.Collapsed.Value) return;

			if (model.CanFallback) OnFallbackLog(infoModel, model);

			switch (model.LogType)
			{
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

		#region KeyValue Logs
		void OnKeyValueLog(EncounterInfoModel infoModel, KeyValueEncounterLogModel model)
		{
			GUILayout.BeginHorizontal();
			{
				var selection = EditorGUILayoutExtensions.HelpfulEnumPopup(
					GUIContent.none,
					"- Append New Key Value Operation -",
					KeyValueTypes.Unknown
				);

				switch (selection)
				{
					case KeyValueTypes.Unknown: break;
					default:
						OnEdgedLogSpawn(model, result => OnKeyValueLogSpawn(result, selection));
						break;
				}

				EditorGUIExtensions.PauseChangeCheck();
				{
					if (
						GUILayout.Button(
							new GUIContent("Defines", "Select a predefined key that is updated are listened to by the event system."),
							EditorStyles.miniButton,
							GUILayout.ExpandWidth(false)
						)
					)
					{
						KeyDefinitionEditorWindow.Show(
							null,
							result => OnEdgedLogSpawn(
								model,
								spawnResult => OnKeyValueLogSpawn(spawnResult, result.ValueType, result.Target, result.Key)
							),
							requiresWrite: true
						);
					}
				}
				EditorGUIExtensions.UnPauseChangeCheck();
			}
			GUILayout.EndHorizontal();


			OnEdgedLog<KeyValueEncounterLogModel, KeyValueEdgeModel>(infoModel, model, OnKeyValueLogEdge);
		}

		void OnKeyValueLogSpawn(
			KeyValueEdgeModel edge,
			KeyValueTypes keyValueType,
			KeyValueTargets target = KeyValueTargets.Unknown,
			string key = null
		)
		{
			Window.ModelSelectionModified = true;
			edge.Entry.KeyValueType.Value = keyValueType;
			IKeyValueAddress output = null;
			Action<IKeyValueAddress> setOutput;

			switch (keyValueType)
			{
				case KeyValueTypes.Boolean:
					output = (edge.Entry.BooleanValue.Value = KeyValueEntryModel.BooleanBlock.Default).Output;
					setOutput = result => edge.Entry.BooleanValue.Value.Output = (KeyValueAddress<bool>)result;
					break;
				case KeyValueTypes.Integer:
				case KeyValueTypes.Enumeration:
					output = (edge.Entry.IntegerValue.Value = KeyValueEntryModel.IntegerBlock.Default).Output;
					setOutput = result => edge.Entry.IntegerValue.Value.Output = (KeyValueAddress<int>)result;
					break;
				case KeyValueTypes.String:
					output = (edge.Entry.StringValue.Value = KeyValueEntryModel.StringBlock.Default).Output;
					setOutput = result => edge.Entry.StringValue.Value.Output = (KeyValueAddress<string>)result;
					break;
				case KeyValueTypes.Float:
					output = (edge.Entry.FloatValue.Value = KeyValueEntryModel.FloatBlock.Default).Output;
					setOutput = result => edge.Entry.FloatValue.Value.Output = (KeyValueAddress<float>)result;
					break;
				default:
					Debug.LogError("Unrecognized KeyValueType: " + keyValueType);
					return;
			}

			if (target != KeyValueTargets.Unknown && !string.IsNullOrEmpty(key))
			{
				output.Source = KeyValueSources.KeyValue;
				output.ForeignTarget = target;
				output.ForeignKey = key;
				setOutput(output);
			}
		}

		void OnKeyValueLogEdge(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueEdgeModel edge
		)
		{
			var entry = edge.Entry;

			switch (entry.KeyValueType.Value)
			{
				case KeyValueTypes.Boolean:
					OnKeyValueLogEdgeEntry(entry.BooleanValue);
					break;
				case KeyValueTypes.Integer:
					OnKeyValueLogEdgeEntry(entry.IntegerValue);
					break;
				case KeyValueTypes.String:
					OnKeyValueLogEdgeEntry(entry.StringValue);
					break;
				case KeyValueTypes.Float:
					OnKeyValueLogEdgeEntry(entry.FloatValue);
					break;
				case KeyValueTypes.Enumeration:
					OnKeyValueLogEdgeEntryEnumeration(entry.IntegerValue);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized KeyValueType: " + entry.KeyValueType.Value, MessageType.Error);
					break;
			}

			EditorGUILayoutValueFilter.Field(
				new GUIContent("Filtering", "Passing this filter is required to continue to run this key value logic."),
				entry.Filtering
			);
		}

		void OnKeyValueLogEdgeEntryBeginFirstLine<T>(
			KeyValueEntryModel.BaseBlock<T> block
		)
			where T : IConvertible
		{
			GUILayout.BeginHorizontal();

			EditorGUILayoutKeyDefinition.Value(
				() => block.Output,
				result => block.Output = result,
				KeyValueSources.KeyValue,
				requiresWrite: true
			);
		}

		void OnKeyValueLogEdgeEntryEndFirstLine()
		{
			GUILayout.EndHorizontal();
		}

		void OnKeyValueLogEdgeEntry(
			KeyValueEntryModel.BooleanBlock block
		)
		{
			OnKeyValueLogEdgeEntryBeginFirstLine(block);

			var previousOperation = block.Operation;
			block.Operation = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				GUIContent.none,
				"- Operation -",
				block.Operation,
				Color.red,
				guiOptions: GUILayout.Width(LogFloats.KeyValueOperationWidth)
			);

			if (previousOperation != block.Operation)
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.BooleanBlock.Operations.Set:
						block.Input1 = KeyValueAddress<bool>.Default;
						break;
				}
			}

			OnKeyValueLogEdgeEntryEndFirstLine();

			GUILayout.BeginHorizontal();
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.BooleanBlock.Operations.Set:
						GUILayout.Label("FROM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels);
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);

						GUILayout.FlexibleSpace();
						break;
					case KeyValueEntryModel.BooleanBlock.Operations.And:
					case KeyValueEntryModel.BooleanBlock.Operations.Or:
					case KeyValueEntryModel.BooleanBlock.Operations.Xor:
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);
						GUILayout.Label(block.OperationReadable, SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(40f));
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input1,
							result => block.Input1 = result
						);
						break;
					case KeyValueEntryModel.BooleanBlock.Operations.Random:
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized Operation: " + block.Operation, MessageType.Error);
						break;
				}
			}
			GUILayout.EndHorizontal();
		}

		void OnKeyValueLogEdgeEntry(
			KeyValueEntryModel.IntegerBlock block
		)
		{
			OnKeyValueLogEdgeEntryBeginFirstLine(block);

			var previousOperation = block.Operation;
			block.Operation = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				GUIContent.none,
				"- Operation -",
				block.Operation,
				Color.red,
				guiOptions: GUILayout.Width(LogFloats.KeyValueOperationWidth)
			);

			if (previousOperation != block.Operation)
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.IntegerBlock.Operations.Set:
					case KeyValueEntryModel.IntegerBlock.Operations.Clamp:
					case KeyValueEntryModel.IntegerBlock.Operations.Random:
						block.Input1 = KeyValueAddress<int>.Default;
						break;
				}
			}

			OnKeyValueLogEdgeEntryEndFirstLine();

			GUILayout.BeginHorizontal();
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.IntegerBlock.Operations.Set:
					case KeyValueEntryModel.IntegerBlock.Operations.Clamp:
						GUILayout.Label("FROM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels);
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);
						GUILayout.FlexibleSpace();
						break;
					case KeyValueEntryModel.IntegerBlock.Operations.Add:
					case KeyValueEntryModel.IntegerBlock.Operations.Subtract:
					case KeyValueEntryModel.IntegerBlock.Operations.Multiply:
					case KeyValueEntryModel.IntegerBlock.Operations.Divide:
					case KeyValueEntryModel.IntegerBlock.Operations.Modulo:
					case KeyValueEntryModel.IntegerBlock.Operations.Random:
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);
						GUILayout.Label(block.OperationReadable, SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(40f));
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input1,
							result => block.Input1 = result
						);
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized Operation: " + block.Operation, MessageType.Error);
						break;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(block.MinimumClampingEnabled);
				{
					GUILayout.Label("MINIMUM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(72f));
					EditorGUILayoutKeyDefinition.Value(
						() => block.MinimumClamping,
						result => block.MinimumClamping = result
					);
				}
				EditorGUILayoutExtensions.PopEnabled();
				if (block.MinimumClampingEnabled != EditorGUILayout.Toggle(block.MinimumClampingEnabled, GUILayout.Width(14f)))
				{
					block.MinimumClampingEnabled = !block.MinimumClampingEnabled;
					if (!block.MinimumClampingEnabled) block.MinimumClamping = KeyValueAddress<int>.Default;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(block.MaximumClampingEnabled);
				{
					GUILayout.Label("MAXIMUM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(72f));
					EditorGUILayoutKeyDefinition.Value(
						() => block.MaximumClamping,
						result => block.MaximumClamping = result
					);
				}
				EditorGUILayoutExtensions.PopEnabled();
				if (block.MaximumClampingEnabled != EditorGUILayout.Toggle(block.MaximumClampingEnabled, GUILayout.Width(14f)))
				{
					block.MaximumClampingEnabled = !block.MaximumClampingEnabled;
					if (!block.MinimumClampingEnabled) block.MaximumClamping = KeyValueAddress<int>.Default;
				}
			}
			GUILayout.EndHorizontal();
		}

		void OnKeyValueLogEdgeEntry(
			KeyValueEntryModel.StringBlock block
		)
		{
			OnKeyValueLogEdgeEntryBeginFirstLine(block);

			var previousOperation = block.Operation;
			block.Operation = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				GUIContent.none,
				"- Operation -",
				block.Operation,
				Color.red,
				guiOptions: GUILayout.Width(LogFloats.KeyValueOperationWidth)
			);

			OnKeyValueLogEdgeEntryEndFirstLine();

			GUILayout.BeginHorizontal();
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.StringBlock.Operations.Set:
						GUILayout.Label("FROM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels);
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);

						GUILayout.FlexibleSpace();
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized Operation: " + block.Operation, MessageType.Error);
						break;
				}
			}
			GUILayout.EndHorizontal();
		}

		void OnKeyValueLogEdgeEntry(
			KeyValueEntryModel.FloatBlock block
		)
		{
			OnKeyValueLogEdgeEntryBeginFirstLine(block);

			var previousOperation = block.Operation;
			block.Operation = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				GUIContent.none,
				"- Operation -",
				block.Operation,
				Color.red,
				guiOptions: GUILayout.Width(LogFloats.KeyValueOperationWidth)
			);

			if (previousOperation != block.Operation)
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.FloatBlock.Operations.Set:
					case KeyValueEntryModel.FloatBlock.Operations.Clamp:
					case KeyValueEntryModel.FloatBlock.Operations.Round:
					case KeyValueEntryModel.FloatBlock.Operations.Floor:
					case KeyValueEntryModel.FloatBlock.Operations.Ceiling:
					case KeyValueEntryModel.FloatBlock.Operations.Random:
						block.Input1 = KeyValueAddress<float>.Default;
						break;
					case KeyValueEntryModel.FloatBlock.Operations.RandomNormal:
						block.Input1 = KeyValueAddress<float>.Default;
						block.Input0 = KeyValueAddress<float>.Default;
						break;
				}
			}

			OnKeyValueLogEdgeEntryEndFirstLine();

			GUILayout.BeginHorizontal();
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.FloatBlock.Operations.Set:
					case KeyValueEntryModel.FloatBlock.Operations.Clamp:
					case KeyValueEntryModel.FloatBlock.Operations.Round:
					case KeyValueEntryModel.FloatBlock.Operations.Floor:
					case KeyValueEntryModel.FloatBlock.Operations.Ceiling:
						GUILayout.Label("FROM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels);
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);
						GUILayout.FlexibleSpace();
						break;
					case KeyValueEntryModel.FloatBlock.Operations.Add:
					case KeyValueEntryModel.FloatBlock.Operations.Subtract:
					case KeyValueEntryModel.FloatBlock.Operations.Multiply:
					case KeyValueEntryModel.FloatBlock.Operations.Divide:
					case KeyValueEntryModel.FloatBlock.Operations.Modulo:
					case KeyValueEntryModel.FloatBlock.Operations.Random:
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result
						);
						GUILayout.Label(block.OperationReadable, SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(40f));
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input1,
							result => block.Input1 = result
						);
						break;
					case KeyValueEntryModel.FloatBlock.Operations.RandomNormal:
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized Operation: " + block.Operation, MessageType.Error);
						break;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(block.MinimumClampingEnabled);
				{
					GUILayout.Label("MINIMUM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(72f));
					EditorGUILayoutKeyDefinition.Value(
						() => block.MinimumClamping,
						result => block.MinimumClamping = result
					);
				}
				EditorGUILayoutExtensions.PopEnabled();
				if (block.MinimumClampingEnabled != EditorGUILayout.Toggle(block.MinimumClampingEnabled, GUILayout.Width(14f)))
				{
					block.MinimumClampingEnabled = !block.MinimumClampingEnabled;
					if (!block.MinimumClampingEnabled) block.MinimumClamping = KeyValueAddress<float>.Default;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(block.MaximumClampingEnabled);
				{
					GUILayout.Label("MAXIMUM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels, GUILayout.Width(72f));
					EditorGUILayoutKeyDefinition.Value(
						() => block.MaximumClamping,
						result => block.MaximumClamping = result
					);
				}
				EditorGUILayoutExtensions.PopEnabled();
				if (block.MaximumClampingEnabled != EditorGUILayout.Toggle(block.MaximumClampingEnabled, GUILayout.Width(14f)))
				{
					block.MaximumClampingEnabled = !block.MaximumClampingEnabled;
					if (!block.MinimumClampingEnabled) block.MaximumClamping = KeyValueAddress<float>.Default;
				}
			}
			GUILayout.EndHorizontal();
		}

		void OnKeyValueLogEdgeEntryEnumeration(
			KeyValueEntryModel.IntegerBlock block
		)
		{
			OnKeyValueLogEdgeEntryBeginFirstLine(block);

			var previousOperation = block.Operation;
			block.Operation = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				GUIContent.none,
				"- Operation -",
				block.Operation,
				Color.red,
				new KeyValueEntryModel.IntegerBlock.Operations[] { KeyValueEntryModel.IntegerBlock.Operations.Unknown, KeyValueEntryModel.IntegerBlock.Operations.Set },
				GUILayout.Width(LogFloats.KeyValueOperationWidth)
			);

			OnKeyValueLogEdgeEntryEndFirstLine();

			GUILayout.BeginHorizontal();
			{
				switch (block.Operation)
				{
					case KeyValueEntryModel.IntegerBlock.Operations.Set:
						GUILayout.Label("FROM", SubLightEditorConfig.Instance.EncounterEditorLogKeyValueOperationLabels);
						EditorGUILayoutKeyDefinition.Value(
							() => block.Input0,
							result => block.Input0 = result,
							keyValueOverride: KeyValueTypes.Enumeration,
							keyValueOverrideRelated: block.Output
						);

						GUILayout.FlexibleSpace();
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized Operation: " + block.Operation, MessageType.Error);
						break;
				}
			}
			GUILayout.EndHorizontal();
		}
		#endregion

		#region Switch Logs
		void OnSwitchLog(
			EncounterInfoModel infoModel,
			SwitchEncounterLogModel model
		)
		{
			model.SelectionMethod.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				new GUIContent("Selection Method"),
				"- Select Selection Method -",
				model.SelectionMethod.Value,
				Color.red
			);

			EditorGUILayoutEncounter.AppendSelectOrBlankLogPopup(
				new GUIContent("Append New Switch"),
				new GUIContent("- Select Target Log -"),
				null,
				infoModel,
				model,
				existingSelection => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, existingSelection)),
				newSelection => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, AppendNewLog(newSelection, infoModel, LogsAppendSources.EdgeSpawn))),
				EncounterLogBlankHandling.None,
				EncounterLogMissingHandling.None,
				EncounterLogBlankOptionHandling.NotSelectable,
				new GUIContent("< Fallback >"),
				() => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, null)),
				null
			);

			OnEdgedLog<SwitchEncounterLogModel, SwitchEdgeModel>(infoModel, model, OnSwitchLogEdge);
		}

		void OnSwitchLogSpawn(
			SwitchEdgeModel edge,
			string targetLogId
		)
		{
			edge.Entry.RandomWeight.Value = 1f;
			edge.Entry.NextLogId.Value = targetLogId;
		}

		void OnSwitchLogEdge(
			EncounterInfoModel infoModel,
			SwitchEncounterLogModel model,
			SwitchEdgeModel edge
		)
		{
			var entry = edge.Entry;

			switch (model.SelectionMethod.Value)
			{
				case SwitchEncounterLogModel.SelectionMethods.FirstFilter:
				case SwitchEncounterLogModel.SelectionMethods.Random:
					break;
				case SwitchEncounterLogModel.SelectionMethods.RandomWeighted:
					entry.RandomWeight.Value = EditorGUILayout.FloatField("Random Weight", entry.RandomWeight.Value);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized SelectionMethod: " + model.SelectionMethod.Value, MessageType.Error);
					break;
			}

			entry.NextLogId.Changed = newLogId => Window.ModelSelectionModified = true;

			EditorGUILayoutEncounter.AppendSelectOrBlankLogPopup(
				new GUIContent("Target Log"),
				new GUIContent("- Select Target Log -"),
				entry.NextLogId.Value,
				infoModel,
				model,
				existingSelection => entry.NextLogId.Value = existingSelection,
				newSelection => entry.NextLogId.Value = AppendNewLog(newSelection, infoModel, LogsAppendSources.EdgeAssignment),
				EncounterLogBlankHandling.FallsThrough,
				EncounterLogMissingHandling.Error,
				EncounterLogBlankOptionHandling.Selectable,
				new GUIContent("< Fallback >"),
				() => entry.NextLogId.Value = null,
				LogsFocusedLogIdsPush
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
			EditorGUILayoutEncounter.AppendSelectOrBlankLogPopup(
				new GUIContent("Append New Button"),
				new GUIContent("- Select Target Log -"),
				null,
				infoModel,
				model,
				existingSelection => OnEdgedLogSpawn(model, result => OnButtonLogSpawn(result, existingSelection)),
				newSelection => OnEdgedLogSpawn(model, result => OnButtonLogSpawn(result, AppendNewLog(newSelection, infoModel, LogsAppendSources.EdgeSpawn))),
				EncounterLogBlankHandling.None,
				EncounterLogMissingHandling.None,
				EncounterLogBlankOptionHandling.NotSelectable,
				new GUIContent("< Fallback >"),
				() => OnEdgedLogSpawn(model, result => OnButtonLogSpawn(result, null)),
				null
			);

			OnEdgedLog<ButtonEncounterLogModel, ButtonEdgeModel>(infoModel, model, OnButtonLogEdge);
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

			entry.NextLogId.Changed = newLogId => Window.ModelSelectionModified = true;

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutEncounter.AppendSelectOrBlankLogPopup(
					new GUIContent("Target Log"),
					new GUIContent("- Select Target Log -"),
					entry.NextLogId.Value,
					infoModel,
					model,
					existingSelection => entry.NextLogId.Value = existingSelection,
					newSelection => entry.NextLogId.Value = AppendNewLog(newSelection, infoModel, LogsAppendSources.EdgeAssignment),
					EncounterLogBlankHandling.FallsThrough,
					EncounterLogMissingHandling.Error,
					EncounterLogBlankOptionHandling.Selectable,
					new GUIContent("< Fallback >"),
					() => entry.NextLogId.Value = null,
					LogsFocusedLogIdsPush
				);

				entry.NotAutoUsed.Value = !EditorGUILayout.ToggleLeft(new GUIContent("Auto Used", "When this button is pressed, automatically set it to appear used the next time around."), !entry.NotAutoUsed.Value, GUILayout.Width(74f));
				entry.AutoDisableInteractions.Value = EditorGUILayout.ToggleLeft(new GUIContent("Auto Disable Interactions", "When this button is pressed, automatically disable future interactions the next time around."), entry.AutoDisableInteractions.Value, GUILayout.Width(152f));
				entry.AutoDisableEnabled.Value = EditorGUILayout.ToggleLeft(new GUIContent("Auto Disable", "When this button is pressed, automatically set this button to be disabled and invisible the next time around."), entry.AutoDisableEnabled.Value, GUILayout.Width(90f));
			}
			GUILayout.EndHorizontal();

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
			entry.Title.Value = EditorGUILayout.TextField("Title", entry.Title.Value);
			entry.Header.Value = EditorGUILayout.TextField(new GUIContent("Header", "The section header, leave blank to indicate this is the introduction."), entry.Header.Value);
			entry.Body.Value = EditorGUILayoutExtensions.TextAreaWrapped("Body", entry.Body.Value);
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
			model.AlwaysHalting.Value = EditorGUILayout.Toggle(
				new GUIContent("Is Halting", "Does the handler wait for the event to complete before it continues?"),
				model.AlwaysHalting.Value
			);

			if (model.AlwaysHalting.Value || model.Edges.Any(e => e.Entry.IsHalting.Value))
			{
				if (logsShowHaltingInfo.Value) EditorGUILayout.HelpBox("This log will halt until all events are complete.", MessageType.Info);
			}
			else if (model.Ending.Value)
			{
				if (logsShowHaltingWarnings.Value) EditorGUILayout.HelpBox("This log is non-halting and also the end log, events may complete after the encounter is complete.", MessageType.Warning);
			}

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

			switch (type)
			{
				case EncounterEvents.Types.ToolbarSelection:
					entry.IsHalting.Value = true;
					break;
			}
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
					OnEncounterEventLogEdgeCustom(entry);
					break;
				case EncounterEvents.Types.Debug:
					OnEncounterEventLogEdgeDebugLog(entry);
					break;
				case EncounterEvents.Types.ToolbarSelection:
					OnEncounterEventLogEdgeToolbarSelection(entry);
					break;
				case EncounterEvents.Types.DumpKeyValues:
					OnEncounterEventLogEdgeDumpKeyValues(entry);
					break;
				case EncounterEvents.Types.GameComplete:
					OnEncounterEventLogEdgeGameComplete(entry);
					break;
				case EncounterEvents.Types.TriggerQueue:
					OnEncounterEventLogEdgePopTriggers(entry);
					break;
				case EncounterEvents.Types.Delay:
					OnEncounterEventLogEdgeDelay(entry);
					break;
				case EncounterEvents.Types.RefreshSystem:
					OnEncounterEventLogEdgeRefreshSystem(entry);
					break;
				case EncounterEvents.Types.AudioSnapshot:
					OnEncounterEventLogEdgeAudioSnapshot(entry);
					break;
				case EncounterEvents.Types.Waypoint:
					OnEncounterEventLogEdgeWaypoint(entry);
					break;
				case EncounterEvents.Types.ModuleTrait:
					OnEncounterEventLogEdgeModuleTrait(entry);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized EventType: " + entry.EncounterEvent.Value, MessageType.Error);
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

		void OnEncounterEventLogEdgeCustom(
			EncounterEventEntryModel entry
		)
		{
			var customEventName = entry.KeyValues.GetString(EncounterEvents.Custom.StringKeys.CustomEventName);
			var wasNullOrEmpty = string.IsNullOrEmpty(customEventName);

			if (wasNullOrEmpty) EditorGUILayoutExtensions.PushBackgroundColor(Color.red);
			{
				entry.KeyValues.SetString(
					EncounterEvents.Custom.StringKeys.CustomEventName,
					EditorGUILayout.TextField(new GUIContent("Custom Event Name"), customEventName)
				);
			}
			if (wasNullOrEmpty) EditorGUILayoutExtensions.PopBackgroundColor();
		}

		void OnEncounterEventLogEdgeDebugLog(
			EncounterEventEntryModel entry
		)
		{
			entry.KeyValues.SetEnumeration(
				EncounterEvents.Debug.EnumKeys.Severity,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Severity"),
					"- Select A Severity -",
					entry.KeyValues.GetEnumeration<EncounterEvents.Debug.Severities>(EncounterEvents.Debug.EnumKeys.Severity)
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
			entry.KeyValues.SetEnumeration(
				EncounterEvents.ToolbarSelection.EnumKeys.Selection,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Selection"),
					"- No Change -",
					entry.KeyValues.GetEnumeration<ToolbarSelections>(EncounterEvents.ToolbarSelection.EnumKeys.Selection)
				)
			);

			entry.KeyValues.SetEnumeration(
				EncounterEvents.ToolbarSelection.EnumKeys.LockState,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Locking"),
					"- No Change -",
					entry.KeyValues.GetEnumeration<EncounterEvents.ToolbarSelection.LockStates>(EncounterEvents.ToolbarSelection.EnumKeys.LockState)
				)
			);
		}

		void OnEncounterEventLogEdgeDumpKeyValues(
			EncounterEventEntryModel entry
		)
		{
			entry.KeyValues.SetEnumeration(
				EncounterEvents.DumpKeyValues.EnumKeys.Target,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Target"),
					"All",
					entry.KeyValues.GetEnumeration<KeyValueTargets>(EncounterEvents.DumpKeyValues.EnumKeys.Target)
				)
			);
		}

		void OnEncounterEventLogEdgeGameComplete(
			EncounterEventEntryModel entry
		)
		{
			entry.KeyValues.SetEnumeration(
				EncounterEvents.GameComplete.EnumKeys.Condition,
				EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					new GUIContent("Condition"),
					"- Select a Condition -",
					entry.KeyValues.GetEnumeration<EncounterEvents.GameComplete.Conditions>(EncounterEvents.GameComplete.EnumKeys.Condition),
					Color.red
				)
			);

			entry.KeyValues.SetEnumeration(
				EncounterEvents.GameComplete.EnumKeys.IconOverride,
				EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					new GUIContent("Icon Override"),
					"- Select an Icon -",
					entry.KeyValues.GetEnumeration<VerticalOptionsIcons>(EncounterEvents.GameComplete.EnumKeys.IconOverride),
					Color.yellow
				)
			);

			entry.KeyValues.SetString(
				EncounterEvents.GameComplete.StringKeys.Title,
				EditorGUILayout.TextField("Title", entry.KeyValues.GetString(EncounterEvents.GameComplete.StringKeys.Title))
			);

			entry.KeyValues.SetString(
				EncounterEvents.GameComplete.StringKeys.Header,
				EditorGUILayout.TextField("Header", entry.KeyValues.GetString(EncounterEvents.GameComplete.StringKeys.Header))
			);

			entry.KeyValues.SetString(
				EncounterEvents.GameComplete.StringKeys.Body,
				EditorGUILayout.TextField("Body", entry.KeyValues.GetString(EncounterEvents.GameComplete.StringKeys.Body))
			);
		}

		void OnEncounterEventLogEdgePopTriggers(
			EncounterEventEntryModel entry
		)
		{
			GUILayout.Label("Pop the following triggers if on the stack");
			EditorGUILayoutExtensions.PushIndent();
			{
				foreach (var trigger in EnumExtensions.GetValues(EncounterTriggers.Unknown))
				{
					entry.KeyValues.SetBoolean(
						EncounterEvents.TriggerQueue.BooleanKeys.PopTrigger(trigger),
						EditorGUILayout.Toggle(
							ObjectNames.NicifyVariableName(trigger.ToString()),
							entry.KeyValues.GetBoolean(EncounterEvents.TriggerQueue.BooleanKeys.PopTrigger(trigger))
						)
					);
				}
			}
			EditorGUILayoutExtensions.PopIndent();
			GUILayout.Label("Push the following triggers onto the stack in descending order");
			EditorGUILayoutExtensions.PushIndent();
			{
				foreach (var trigger in EnumExtensions.GetValues(EncounterTriggers.Unknown))
				{
					var pushValue = entry.KeyValues.GetInteger(EncounterEvents.TriggerQueue.IntegerKeys.PushTrigger(trigger));

					if (pushValue == EncounterEvents.TriggerQueue.PushDisabled) EditorGUILayoutExtensions.PushColor(Color.gray);
					{
						entry.KeyValues.SetInteger(
							EncounterEvents.TriggerQueue.IntegerKeys.PushTrigger(trigger),
							Mathf.Max(
								EncounterEvents.TriggerQueue.PushDisabled,
								EditorGUILayout.IntField(
									ObjectNames.NicifyVariableName(trigger.ToString()),
									pushValue
								)
							)
						);
					}
					if (pushValue == EncounterEvents.TriggerQueue.PushDisabled) EditorGUILayoutExtensions.PopColor();
				}
			}
			EditorGUILayoutExtensions.PopIndent();
		}

		void OnEncounterEventLogEdgeDelay(
			EncounterEventEntryModel entry
		)
		{
			var trigger = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				new GUIContent("Trigger"),
				"- Select a Trigger -",
				entry.KeyValues.GetEnumeration<EncounterEvents.Delay.Triggers>(EncounterEvents.Delay.EnumKeys.Trigger),
				Color.red
			);

			entry.KeyValues.SetEnumeration(
				EncounterEvents.Delay.EnumKeys.Trigger,
				trigger
			);

			switch (trigger)
			{
				case EncounterEvents.Delay.Triggers.Unknown: break;
				case EncounterEvents.Delay.Triggers.Time:
					entry.KeyValues.SetFloat(
						EncounterEvents.Delay.FloatKeys.TimeDuration,
						EditorGUILayout.FloatField(
							new GUIContent("Duration"),
							entry.KeyValues.GetFloat(EncounterEvents.Delay.FloatKeys.TimeDuration)
						)
					);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized Trigger: " + trigger, MessageType.Error);
					break;
			}
		}

		void OnEncounterEventLogEdgeRefreshSystem(
			EncounterEventEntryModel entry
		)
		{
			OnEncounterEventLogEdgeNoModifications();
		}

		void OnEncounterEventLogEdgeAudioSnapshot(
			EncounterEventEntryModel entry
		)
		{
			entry.KeyValues.SetString(
				EncounterEvents.AudioSnapshot.StringKeys.SnapshotName,
				EditorGUILayout.TextField(
					new GUIContent("Snapshot Name"),
					entry.KeyValues.GetString(EncounterEvents.AudioSnapshot.StringKeys.SnapshotName)
				)
			);

			entry.KeyValues.SetFloat(
				EncounterEvents.AudioSnapshot.FloatKeys.TransitionDuration,
				EditorGUILayout.FloatField(
					new GUIContent("Transition Duration"),
					entry.KeyValues.GetFloat(EncounterEvents.AudioSnapshot.FloatKeys.TransitionDuration)
				)
			);
		}

		void OnEncounterEventLogEdgeWaypoint(
			EncounterEventEntryModel entry
		)
		{

			var waypointId = entry.KeyValues.GetString(EncounterEvents.Waypoint.StringKeys.WaypointId);
			var waypointIdInvalid = string.IsNullOrEmpty(waypointId);

			EditorGUILayoutExtensions.PushColorValidation(Color.red, waypointIdInvalid);
			{
				entry.KeyValues.SetString(
					EncounterEvents.Waypoint.StringKeys.WaypointId,
					EditorGUILayout.TextField(
						new GUIContent("Waypoint Id"),
						waypointId
					)
				);
			}
			EditorGUILayoutExtensions.PopColorValidation(waypointIdInvalid);

			entry.KeyValues.SetEnumeration(
				EncounterEvents.Waypoint.EnumKeys.Visibility,
				EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Visibility"),
					"- No Change -",
					entry.KeyValues.GetEnumeration<WaypointModel.VisibilityStates>(EncounterEvents.Waypoint.EnumKeys.Visibility)
				)
			);
		}
		
		void OnEncounterEventLogEdgeModuleTrait(
			EncounterEventEntryModel entry
		)
		{
			EncounterEvents.ModuleTrait.Operations operation;
			
			entry.KeyValues.SetEnumeration(
				EncounterEvents.ModuleTrait.EnumKeys.Operation,
				operation = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					new GUIContent("Operation"),
					"- Operation -",
					entry.KeyValues.GetEnumeration<EncounterEvents.ModuleTrait.Operations>(EncounterEvents.ModuleTrait.EnumKeys.Operation),
					Color.red
				)
			);

			GUIContent operationIdLabel = null;
			
			switch (operation)
			{
				case EncounterEvents.ModuleTrait.Operations.Unknown: break;
				case EncounterEvents.ModuleTrait.Operations.AppendByTraitId:
				case EncounterEvents.ModuleTrait.Operations.RemoveByTraitId:
					operationIdLabel = new GUIContent("Trait Id");
					break;
				case EncounterEvents.ModuleTrait.Operations.RemoveByFamilyId:
					operationIdLabel = new GUIContent("Family Id");
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized Operation: " + operation, MessageType.Error);
					break;
			}

			if (operationIdLabel != null)
			{
				var operationIdIsInvalid = string.IsNullOrEmpty(entry.KeyValues.GetString(EncounterEvents.ModuleTrait.StringKeys.OperationId));
				// TODO: Actually validate if operation id exists...
				EditorGUILayoutExtensions.PushColorValidation(Color.red, operationIdIsInvalid);
				{
					entry.KeyValues.SetString(
						EncounterEvents.ModuleTrait.StringKeys.OperationId,
						EditorGUILayout.TextField(
							operationIdLabel,
							entry.KeyValues.GetString(EncounterEvents.ModuleTrait.StringKeys.OperationId)
						)
					);
				}
				EditorGUILayoutExtensions.PopColorValidation(operationIdIsInvalid);
			}
			
			GUILayout.Label("Target Module Types (Select none to apply to all Module Types)");

			EditorGUILayoutExtensions.PushIndent();
			{
				foreach (var moduleType in EnumExtensions.GetValues(ModuleTypes.Unknown))
				{
					var moduleTypeKey = EncounterEvents.ModuleTrait.BooleanKeys.ModuleTypeIsValid(moduleType);

					entry.KeyValues.SetBoolean(
						moduleTypeKey,
						EditorGUILayout.Toggle(
							ObjectNames.NicifyVariableName(moduleType.ToString()),
							entry.KeyValues.GetBoolean(moduleTypeKey)
						)
					);
				}
			}
			EditorGUILayoutExtensions.PopIndent();
		}

		void OnEncounterEventLogEdgeNoModifications()
		{
			EditorGUILayout.HelpBox("No values to modify for this event.", MessageType.Info);
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
			entry.Message.Value = EditorGUILayoutExtensions.TextAreaWrapped("Message", entry.Message.Value);

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

					EditorGUILayoutEncounter.AppendSelectOrBlankLogPopup(
						new GUIContent("Target Log"),
						new GUIContent("- Select Target Log -"),
						nextId.Value,
						infoModel,
						model,
						existingSelection => nextId.Value = existingSelection,
						newSelection => nextId.Value = AppendNewLog(newSelection, infoModel, LogsAppendSources.EdgeAssignment),
						EncounterLogBlankHandling.FallsThrough,
						EncounterLogMissingHandling.Error,
						EncounterLogBlankOptionHandling.Selectable,
						new GUIContent("< Fallback >"),
						() => nextId.Value = null,
						LogsFocusedLogIdsPush
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
				if (logsShowHaltingInfo.Value) EditorGUILayout.HelpBox("This log will halt until all busts are focused.", MessageType.Info);
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
				block.TransmitionStrengthIcon = EditorGUILayoutExtensions.HelpfulEnumPopup(new GUIContent("Strength Bar"), "- Select Strength -", block.TransmitionStrengthIcon);
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
				block.AvatarType = EditorGUILayoutExtensions.HelpfulEnumPopup(new GUIContent("Type"), "- Select Avatar Type -", block.AvatarType);
				switch (block.AvatarType)
				{
					case BustEntryModel.AvatarTypes.Unknown: EditorGUILayout.HelpBox("An Avatar Type must be specified.", MessageType.Error); break;
					case BustEntryModel.AvatarTypes.Static: block = OnBustLogEdgeInitializeAvatarStatic(block); break;
					default: EditorGUILayout.HelpBox("Unrecognized AvatarType: " + block.AvatarType, MessageType.Error); break;
				}
			}
			EditorGUILayoutExtensions.PopIndent();

			GUILayout.Label("Button Theme", EditorStyles.boldLabel);
			EditorGUILayoutExtensions.PushIndent();
			{
				block.Theme = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					GUIContent.none,
					"- Select Theme -",
					block.Theme,
					Color.red
				);
			}
			EditorGUILayoutExtensions.PopIndent();

			entry.InitializeInfo.Value = block;
		}

		BustEntryModel.InitializeBlock OnBustLogEdgeInitializeAvatarStatic(BustEntryModel.InitializeBlock block)
		{
			block.AvatarStaticIndex = Mathf.Max(0, EditorGUILayout.IntField("Index", block.AvatarStaticIndex));
			block.AvatarStaticTerminalTextVisible = EditorGUILayout.Toggle("Terminal Text Visible", block.AvatarStaticTerminalTextVisible);
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
			OnEdgedLog<ConversationEncounterLogModel, ConversationEdgeModel>(
				infoModel,
				model,
				OnConversationLogEdge,
				OnConversationLogSpawnOptions,
				OnConversationLogEdgeHeaderLeft
			);
		}

		void OnConversationLogSpawnOptions(
			EncounterInfoModel infoModel,
			ConversationEncounterLogModel model,
			string prefix,
			int index = int.MaxValue
		)
		{
			var appendSelection = EditorGUILayoutExtensions.HelpfulEnumPopup(
				GUIContent.none,
				"- " + (string.IsNullOrEmpty(prefix) ? LogStrings.EdgeEntryAppendPrefix : prefix) + " a Conversation Entry -",
				ConversationTypes.Unknown,
				EnumExtensions.GetValues(ConversationTypes.AttachmentIncoming), // Attachments aren't supported yet...
				GUILayout.MaxWidth(LogFloats.AppendEntryWidthMaximum)
			);

			switch (appendSelection)
			{
				case ConversationTypes.Unknown: break;
				case ConversationTypes.MessageIncoming:
				case ConversationTypes.MessageOutgoing:
					OnEdgedLogSpawn(model, edge => OnConversationLogSpawnMessage(appendSelection, edge), index);
					break;
				case ConversationTypes.Prompt:
					OnEdgedLogSpawn(model, OnConversationLogSpawnPrompt, index);
					break;
				default:
					Debug.LogError("Unrecognized ConversationType: " + appendSelection);
					break;
			}
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

		void OnConversationLogEdgeHeaderLeft(
			EncounterInfoModel infoModel,
			ConversationEncounterLogModel model,
			ConversationEdgeModel edge
		)
		{
			switch (edge.Entry.ConversationType.Value)
			{
				case ConversationTypes.MessageIncoming:
				case ConversationTypes.MessageOutgoing:
					var isIncoming = edge.Entry.ConversationType.Value == ConversationTypes.MessageIncoming;

					if (EditorGUILayoutExtensions.ToggleButtonArray(isIncoming, "Incoming", "Outgoing") != isIncoming)
					{
						isIncoming = !isIncoming;
						edge.Entry.ConversationType.Value = isIncoming ? ConversationTypes.MessageIncoming : ConversationTypes.MessageOutgoing;
					}
					break;
				case ConversationTypes.Prompt:
					var promptBlock = edge.Entry.PromptInfo.Value;

					promptBlock.Behaviour = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
						GUIContent.none,
						"- Select Behaviour -",
						promptBlock.Behaviour,
						Color.red,
						guiOptions: new GUILayoutOption[] { GUILayout.Width(90f) }
					);

					edge.Entry.PromptInfo.Value = promptBlock;
					break;
			}
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
		}

		void OnConversationLogEdgeMessage(ConversationEntryModel entry)
		{
			var block = entry.MessageInfo.Value;

			entry.Message.Value = EditorGUILayoutExtensions.TextAreaWrapped(
				entry.Message.Value
			);
			
			entry.MessageInfo.Value = block;
		}

		void OnConversationLogEdgePrompt(ConversationEntryModel entry)
		{
			var block = entry.PromptInfo.Value;

			switch (block.Behaviour)
			{
				case ConversationButtonPromptBehaviours.ButtonOnly:
				case ConversationButtonPromptBehaviours.PrintMessage:
				case ConversationButtonPromptBehaviours.PrintOverride:
					entry.Message.Value = EditorGUILayoutExtensions.TextAreaWrapped(
						entry.Message.Value
					);
					break;
				case ConversationButtonPromptBehaviours.Continue:
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized PromptBehaviour: " + block.Behaviour, MessageType.Error);
					break;
			}

			switch (block.Behaviour)
			{
				case ConversationButtonPromptBehaviours.ButtonOnly:
				case ConversationButtonPromptBehaviours.PrintMessage:
				case ConversationButtonPromptBehaviours.Continue:
					break;
				case ConversationButtonPromptBehaviours.PrintOverride:
					block.MessageOverride = EditorGUILayoutExtensions.TextAreaWrapped(
						block.MessageOverride
					);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized PromptBehaviour: " + block.Behaviour, MessageType.Error);
					break;
			}

			entry.PromptInfo.Value = block;
		}
		#endregion

		#region Edged Logs
		void OnEdgedLog<L, E>(
			EncounterInfoModel infoModel,
			L model,
			Action<EncounterInfoModel, L, E> edgeEditor,
			Action<EncounterInfoModel, L, string, int> edgeSpawnOptions = null,
			Action<EncounterInfoModel, L, E> edgeHeaderLeft = null
		)
			where L : IEdgedEncounterLogModel<E>
			where E : class, IEdgeModel
		{
			if (edgeSpawnOptions != null) edgeSpawnOptions(infoModel, model, LogStrings.EdgeEntryAppendPrefix, int.MaxValue);

			var deleted = string.Empty;
			var isAlternate = false;

			E indexSwap0 = null;
			E indexSwap1 = null;

			var isMoving = !Event.current.shift && Event.current.control;
			var isDeleting = !Event.current.shift && Event.current.alt;

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

					var normalBackgroundColor = Color.white;
					var alternateBackgroundColor = Color.grey.NewV(0.5f);

					var isGlobalIndentingSupported = logsEdgeIndentsEnabled.Value;

					for (var i = 0; i < sortedCount; i++)
					{
						var current = sorted[i];
						var next = (i + 1 < sortedCount) ? sorted[i + 1] : null;
						int currMoveDelta;

						isAlternate = !isAlternate;

						var currentColor = isAlternate ? alternateBackgroundColor : normalBackgroundColor;
						var currentEffect = logEdgeVisualOverrides.FirstOrDefault(e => e.EdgeId == current.EdgeId);

						if (currentEffect != null)
						{
							switch (currentEffect.Effect)
							{
								case LogEdgeVisualOverride.Effects.Highlight:
									currentColor = Color.Lerp(Color.yellow, currentColor, currentEffect.QuartNormal);
									break;
								default:
									Debug.LogError("Unrecognized EdgeVisualOverrideEffect: " + currentEffect.Effect);
									break;
							}
						}

						var isIndented = isGlobalIndentingSupported && !Mathf.Approximately(0f, current.EdgeIndent);

						if (isIndented)
						{
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(current.EdgeIndent);
						}

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, currentColor);
						{
							if (
								OnEdgedLogEdgeHeader(
									infoModel,
									model,
									current,
									i,
									sortedCount,
									isMoving,
									isDeleting,
									out currMoveDelta,
									edgeSpawnOptions,
									edgeHeaderLeft
								)
							)
							{
								deleted = current.EdgeId;
							}

							if (currMoveDelta != 0)
							{
								indexSwap0 = current;
								indexSwap1 = currMoveDelta == 1 ? next : last;
							}

							if (!current.EdgeIgnore) edgeEditor(infoModel, model, current);

							last = current;
						}
						EditorGUILayoutExtensions.EndVertical();

						if (isIndented)
						{
							EditorGUILayout.EndHorizontal();
						}
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

			if (edgeSpawnOptions != null && model.Edges.Any()) edgeSpawnOptions(infoModel, model, LogStrings.EdgeEntryAppendPrefix, int.MaxValue);
		}

		void OnEdgedLogSpawn<E>(
			IEdgedEncounterLogModel<E> model,
			Action<E> initialize = null,
			int index = int.MaxValue
		)
			where E : class, IEdgeModel, new()
		{
			Window.ModelSelectionModified = true;

			if (model.Edges.Any())
			{
				var ordered = model.Edges.OrderBy(e => e.EdgeIndex);

				if (index == int.MinValue) index = ordered.First().EdgeIndex;
				else if (index == int.MaxValue) index = ordered.Last().EdgeIndex + 1;

				var currentIndex = ordered.First().EdgeIndex;
				var replacementIndex = 0;
				var wasReplaced = false;
				foreach (var edge in ordered)
				{
					if (edge.EdgeIndex == index)
					{
						replacementIndex++;
						wasReplaced = true;
					}
					
					edge.EdgeIndex = replacementIndex;
					
					replacementIndex++;
				}

				if (!wasReplaced) index = replacementIndex;
			}
			else index = 0;

			var result = new E();
			result.EdgeId = Guid.NewGuid().ToString();
			result.EdgeIndex = index;
			if (initialize != null) initialize(result);
			model.Edges = model.Edges.Append(result).ToArray();

			EditorGUIExtensions.ResetControls();

			LogAddEdgeVisualOverrideHighlight(result.EdgeId);
		}

		bool OnEdgedLogEdgeHeader<L, E>(
			EncounterInfoModel infoModel,
			L model,
			E edge,
			int count,
			int maxCount,
			bool isMoving,
			bool isDeleting,
			out int indexDelta,
			Action<EncounterInfoModel, L, string, int> edgeSpawnOptions = null,
			Action<EncounterInfoModel, L, E> edgeHeaderLeft = null
		)
			where L : IEdgedEncounterLogModel<E>
			where E : class, IEdgeModel
		{
			var deleted = false;
			indexDelta = 0;

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(!edge.EdgeIgnore);
				{
					GUILayout.Label("#" + (count + 1) + " | " + edge.EdgeName, GUILayout.ExpandWidth(false));
				}
				EditorGUILayoutExtensions.PopEnabled();
				if (isMoving)
				{
					GUILayout.FlexibleSpace();
					if (edgeSpawnOptions == null) GUILayout.Label("Entry Insertion Not Supported   |");
					else edgeSpawnOptions(infoModel, model, LogStrings.EdgeEntryInsertPrefix, edge.EdgeIndex);

					GUILayout.Label("Click to Rearrange", GUILayout.ExpandWidth(false));
					EditorGUILayoutExtensions.PushEnabled(0 < count);
					{
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
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				else if (isDeleting)
				{
					GUILayout.FlexibleSpace();
					deleted = EditorGUILayoutExtensions.XButton(true);
				}
				else
				{
					if (edgeHeaderLeft != null) edgeHeaderLeft(infoModel, model, edge);
					GUILayout.FlexibleSpace();
					GUILayout.Label(new GUIContent("Ignore", "Ignoring this entry will cause encounters to skip it."), GUILayout.ExpandWidth(false));
					edge.EdgeIgnore = GUILayout.Toggle(edge.EdgeIgnore, GUIContent.none, GUILayout.ExpandWidth(false));
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
			model.FallbackLogId.Changed = newLogId => Window.ModelSelectionModified = true;

			EditorGUILayoutEncounter.AppendSelectOrBlankLogPopup(
				new GUIContent(model.RequiresFallbackLog ? "Next Log" : "Fallback Log", "The encounter will fallthrough to this log if not overridden."),
				new GUIContent(model.RequiresFallbackLog ? "- Select Next Log -" : "- Select Fallback Log -"),
				model.FallbackLogId.Value,
				infoModel,
				model,
				existingSelection => model.FallbackLogId.Value = existingSelection,
				newSelection => model.FallbackLogId.Value = AppendNewLog(newSelection, infoModel, LogsAppendSources.LogFallback),
				EncounterLogBlankHandling.SpecifiedByModel,
				EncounterLogMissingHandling.Error,
				EncounterLogBlankOptionHandling.Selectable,
				new GUIContent("< Blank >"),
				() => model.FallbackLogId.Value = null,
				LogsFocusedLogIdsPush
			);
		}

		void OnLogEnd(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			EditorGUILayoutExtensions.EndVertical();
		}

		#region Utility
		string[] GetAllBustIds(EncounterInfoModel infoModel)
		{
			return infoModel.Logs.GetLogs<BustEncounterLogModel>().SelectMany(l => l.Edges).Select(e => e.Entry.BustId.Value).Distinct().ToArray();
		}

		string[] GetAllBustIdsNormalized(EncounterInfoModel infoModel)
		{
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

		void LogsBustHeightCache()
		{
			logRectCache.Clear();
		}
		#endregion

		#region Log Stack Utility
		/// <summary>
		/// Peeks at the stack at the current index.
		/// </summary>
		/// <returns>LogId.</returns>
		string LogsFocusedLogIdsPeek() { return LogsFocusedLogIdsStack.ElementAtOrDefault(logsFocusedLogIdsIndex.Value); }

		/// <summary>
		/// Peeks at the stack at the specified index.
		/// </summary>
		/// <returns>LogId.</returns>
		/// <param name="index">Index.</param>
		string LogsFocusedLogIdsPeekAbsolute(int index) { return LogsFocusedLogIdsStack.ElementAtOrDefault(index); }

		/// <summary>
		/// Peeks at the stack at the current index + the specified <paramref name="indexOffset"/>.
		/// </summary>
		/// <returns>LogId.</returns>
		/// <param name="indexOffset">Index offset.</param>
		string LogsFocusedLogIdsPeekRelative(int indexOffset) { return LogsFocusedLogIdsStack.ElementAtOrDefault(logsFocusedLogIdsIndex.Value + indexOffset); }

		int LogsSetFocusedLogIdsIndex(int index)
		{
			logsFocusedLogIdsIndex.Value = index;
			EditorGUIExtensions.ResetControls();
			LogsBustHeightCache();
			return index;
		}

		void LogsFocusedLogIdsPush(string logId)
		{
			logsStackScroll.Value = float.MaxValue;
			LogsFocusedLogIdsPopAndPush(logsFocusedLogIdsIndex.Value, logId);
		}

		void LogsFocusedLogIdsPopAndPush(int index, string logId)
		{
			if (string.IsNullOrEmpty(logId)) throw new ArgumentException("logId cannot be null or empty");
			LogsFocusedLogIdsPop(Mathf.Clamp(index, -1, LogsFocusedLogIdsStack.Count() - 1));
			LogsFocusedLogIdsStack = LogsFocusedLogIdsStack.Append(logId);
			LogsSetFocusedLogIdsIndex(LogsFocusedLogIdsStack.Count() - 1);
		}

		void LogsFocusedLogIdsPop(int index = -1)
		{
			var newStack = new List<string>();
			for (var i = 0; i <= index; i++) newStack.Add(LogsFocusedLogIdsStack.ElementAt(i));
			LogsFocusedLogIdsStack = newStack;
			LogsSetFocusedLogIdsIndex(index);
		}

		void LogsFocusedLogIdsOffsetIndex(int delta)
		{
			LogsSetFocusedLogIdsIndex(Mathf.Clamp(logsFocusedLogIdsIndex.Value + delta, 0, LogsFocusedLogIdsStack.Count() - 1));
		}
		#endregion

		#region Exposed Utility

		#endregion
	}
}