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
	public partial class EncounterEditorWindow
	{
		enum HomeStates
		{
			Unknown = 0,
			Browsing = 10,
			Selected = 20
		}

		EditorPrefsFloat homeLeftBarScroll = new EditorPrefsFloat(KeyPrefix + "LeftBarScroll");
		EditorPrefsFloat homeCrewLogsScroll = new EditorPrefsFloat(KeyPrefix + "CrewLogsScroll");
		EditorPrefsString homeSelectedEncounterPath = new EditorPrefsString(KeyPrefix + "HomeSelectedEncounter");
		EditorPrefsEnum<HomeStates> homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);
		EditorPrefsInt homeSelectedToolbar = new EditorPrefsInt(KeyPrefix + "HomeSelectedState");

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus encounterListStatus;
		SaveModel[] encounterList = new SaveModel[0];

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus selectedEncounterStatus;
		EncounterInfoModel selectedEncounter;
		bool selectedEncounterModified;

		void OnHomeEnable()
		{
			encounterListStatus = RequestStatus.Cancel;
			selectedEncounterStatus = RequestStatus.Cancel;

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
			switch (encounterListStatus)
			{
				case RequestStatus.Cancel:
					LoadEncounterList();
					return;
				case RequestStatus.Unknown:
					return;
			}
			if (string.IsNullOrEmpty(homeSelectedEncounterPath.Value)) return;

			switch (selectedEncounterStatus)
			{
				case RequestStatus.Cancel:
					LoadSelectedEncounter(encounterList.FirstOrDefault(m => m.Path == homeSelectedEncounterPath.Value));
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
				OnHomeRightPane(selectedEncounter);
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
					if (GUILayout.Button("New")) NewEncounter();
					if (GUILayout.Button("Refresh")) LoadEncounterList();
				}
				GUILayout.EndHorizontal();

				homeLeftBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLeftBarScroll), false, true).y;
				{
					var isAlternate = false;
					foreach (var info in encounterList) OnDrawEncounterInfo(info, ref isAlternate);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		void OnHomeRightPane(EncounterInfoModel model = null)
		{
			if (model == null)
			{
				GUILayout.BeginVertical();
				{
					EditorGUILayout.HelpBox("Select an encounter to edit.", MessageType.Info);
				}
				GUILayout.EndVertical();
				return;
			}

			string[] names =
			{
				"General",
				"Crew Logs"
			};

			GUILayout.BeginVertical();
			{
				var encounterName = string.IsNullOrEmpty(model.Name) ? "< No Name > " : model.Name;
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Editing: " + encounterName);
					GUI.enabled = selectedEncounterModified;
					if (GUILayout.Button("Save", GUILayout.Width(64f))) SaveSelectedEncounter(selectedEncounter);
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				homeSelectedToolbar.Value = GUILayout.Toolbar(Mathf.Min(homeSelectedToolbar, names.Length - 1), names);

				switch(homeSelectedToolbar.Value)
				{
					case 0:
						OnHomeSelectedGeneral(model);
						break;
					case 1:
						OnHomeSelectedCrewLogs(model);
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized index", MessageType.Error);
						break;
				}
			}
			GUILayout.EndVertical();
		}

		void OnHomeSelectedGeneral(EncounterInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.OrderWeight.Value = EditorGUILayout.FloatField("Order Weight", model.OrderWeight.Value, GUILayout.ExpandWidth(true));
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();
				model.RandomWeightMultiplier.Value = EditorGUILayout.FloatField("Random Weight Multiplier", model.RandomWeightMultiplier.Value);
				model.EncounterId.Value = model.SetMetaKey(MetaKeyConstants.EncounterInfo.EncounterId, EditorGUILayout.TextField("Encounter Id", model.EncounterId.Value));
				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The internal description for notes and production purposes."), model.Description.Value, leftOffset: false);
				model.Hook.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Hook", "The description given to the player before entering this encounter."), model.Hook.Value, leftOffset: false);

				model.Trigger.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(new GUIContent("Trigger", "What triggers this encounter to appear upon entering a system with it."),"- Select a Trigger -", model.Trigger.Value);
				if (model.Trigger.Value == EncounterTriggers.Unknown) EditorGUILayout.HelpBox("A trigger for this encounter must be selected.", MessageType.Error);
				model.AssignedToBody.Value = EditorGUILayoutExtensions.ToggleButton(new GUIContent("Assigned To Body", "If true, this encounter is associated with a specific body in the specified system."), model.AssignedToBody.Value);

				var alternateColor = Color.grey;

				EditorGUILayoutValueFilter.Field(new GUIContent("Filtering", "These checks determine if the encounter will be selected."), model.Filtering, alternateColor);

				model.ValidSystems.Value = EditorGUILayoutExtensions.EnumArray(
					"Valid Systems",
					model.ValidSystems.Value,
					"- Select a SystemType -"
				);
				model.ValidBodies.Value = EditorGUILayoutExtensions.EnumArray(
					"Valid Bodies",
					model.ValidBodies.Value,
					"- Select a BodyType -",
					color: alternateColor
				);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedEncounterModified);
		}

		void OnHomeSelectedCrewLogs(EncounterInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Log Count: " + model.Logs.All.Value.Count()+" |", GUILayout.ExpandWidth(false));
					GUILayout.Label("Append New Log:", GUILayout.ExpandWidth(false));
					AppendNewLog(EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Log Type -", EncounterLogTypes.Unknown), model);
					GUILayout.Label("Hold 'Ctrl' to rearrange entries.", GUILayout.ExpandWidth(false));
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
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedEncounterModified);

			homeCrewLogsScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeCrewLogsScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					var deleted = string.Empty;
					var beginning = string.Empty;

					EncounterLogModel indexToBeginning = null;
					EncounterLogModel indexToEnding = null;
					EncounterLogModel indexSwap0 = null;
					EncounterLogModel indexSwap1 = null;

					var isMoving = Event.current.control;

					var sorted = model.Logs.All.Value.OrderBy(l => l.Index.Value).ToList();
					var sortedCount = sorted.Count;
					EncounterLogModel lastLog = null;
					for (var i = 0; i < sortedCount; i++)
					{
						var log = sorted[i];
						var nextLog = (i + 1 < sortedCount) ? sorted[i + 1] : null;
						int currMoveDelta;

						if (OnLogBegin(i, sortedCount, model, log, isMoving, out currMoveDelta, ref beginning)) deleted = log.LogId;

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
				EditorGUIExtensions.EndChangeCheck(ref selectedEncounterModified);
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
				case EncounterLogTypes.Inventory:
					return NewEncounterLog<InventoryEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Switch:
					return NewEncounterLog<SwitchEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Button:
					return NewEncounterLog<ButtonEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				case EncounterLogTypes.Encyclopedia:
					return NewEncounterLog<EncyclopediaEncounterLogModel>(infoModel, nextIndex, isBeginning).LogId.Value;
				default:
					Debug.LogError("Unrecognized EncounterLogType:" + logType);
					break;
			}
			return null;
		}

		void NewEncounter()
		{
			var info = SaveLoadService.Create<EncounterInfoModel>();
			info.EncounterId.Value = info.SetMetaKey(MetaKeyConstants.EncounterInfo.EncounterId, Guid.NewGuid().ToString());
			info.RandomWeightMultiplier.Value = 1f;
			info.Name.Value = string.Empty;
			info.Meta.Value = info.Name;
			info.Trigger.Value = EncounterTriggers.Automatic;
			SaveLoadService.Save(info, OnNewEncounterInfo);
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

		void LoadEncounterList()
		{
			encounterListStatus = RequestStatus.Unknown;
			SaveLoadService.List<EncounterInfoModel>(OnLoadEncounterList);
		}

		void LoadSelectedEncounter(SaveModel model)
		{
			if (model == null)
			{
				homeSelectedEncounterPath.Value = null;
				selectedEncounterStatus = RequestStatus.Failure;
				return;
			}
			selectedEncounterStatus = RequestStatus.Unknown;
			homeSelectedEncounterPath.Value = model.Path;
			SaveLoadService.Load<EncounterInfoModel>(model, OnLoadSelectedEncounter);
		}

		void SaveSelectedEncounter(EncounterInfoModel model)
		{
			SaveLoadService.Save(model, OnSaveSelectedEncounter, false);
		}

		void OnSaveSelectedEncounter(SaveLoadRequest<EncounterInfoModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			selectedEncounter = result.TypedModel;
			selectedEncounterModified = false;
			encounterListStatus = RequestStatus.Cancel;
		}

		void OnNewEncounterInfo(SaveLoadRequest<EncounterInfoModel> result)
		{
			selectedEncounterStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			encounterListStatus = RequestStatus.Cancel;
			homeSelectedEncounterPath.Value = result.TypedModel.Path;
			selectedEncounter = result.TypedModel;
		}

		void OnLoadEncounterList(SaveLoadArrayRequest<SaveModel> result)
		{
			encounterListStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			encounterList = result.Models;

			if (selectedEncounter == null) return;
			if (encounterList.FirstOrDefault(e => e.Path.Value == selectedEncounter.Path.Value) != null) return;
			OnDeselectEncounter();
		}

		void OnDeselectEncounter()
		{
			homeState.Value = HomeStates.Browsing;
			selectedEncounterStatus = RequestStatus.Failure;
			selectedEncounter = null;
			selectedEncounterModified = false;
		}

		void OnDrawEncounterInfo(SaveModel info, ref bool isAlternate)
		{
			var isSelected = homeSelectedEncounterPath.Value == info.Path.Value;
			if (isSelected || isAlternate) EditorGUILayoutExtensions.PushBackgroundColor(isSelected ? Color.blue : Color.grey);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (!isSelected && isAlternate) EditorGUILayoutExtensions.PopBackgroundColor();
			{
				var infoPath = info.IsInternal ? info.InternalPath : info.Path;
				var infoId = info.GetMetaKey(MetaKeyConstants.EncounterInfo.EncounterId);
				var infoName = infoId;
				if (string.IsNullOrEmpty(infoName)) infoName = "< No EncounterId >";
				else if (20 < infoName.Length) infoName = infoName.Substring(0, 20) + "...";

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical();
					{
						GUILayout.Label(new GUIContent(string.IsNullOrEmpty(info.Meta) ? "< No Meta >" : info.Meta, "Name is set by Meta field."), EditorStyles.boldLabel, GUILayout.Height(14f));
						if (GUILayout.Button(new GUIContent(infoName, "Copy EncounterId of " + infoPath)))
						{
							EditorGUIUtility.systemCopyBuffer = infoId;
							ShowNotification(new GUIContent("Copied EncounterId to Clipboard"));
						}
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{
						if (GUILayout.Button("Edit"))
						{
							LoadSelectedEncounter(info);
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

		void OnLoadSelectedEncounter(SaveLoadRequest<EncounterInfoModel> result)
		{
			GUIUtility.keyboardControl = 0;
			selectedEncounterStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			var model = result.TypedModel;
			selectedEncounterModified = false;
			homeState.Value = HomeStates.Selected;
			homeSelectedEncounterPath.Value = model.Path;
			selectedEncounter = model;
		}
	}
}