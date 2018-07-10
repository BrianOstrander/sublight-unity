using System;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
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
		EditorPrefsString homeSelectedEncounterPath = new EditorPrefsString(KeyPrefix + "HomeSelectedEncounter");
		EditorPrefsEnum<HomeStates> homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);

		RequestStatus encounterListStatus;
		SaveModel[] encounterList = new SaveModel[0];

		RequestStatus selectedEncounterStatus;
		EncounterInfoModel selectedEncounter;
		bool selectedEncounterModified;

		void OnHomeEnable()
		{
			encounterListStatus = RequestStatus.Cancel;
			selectedEncounterStatus = RequestStatus.Cancel;
		}

		void OnHome()
		{
			OnCheckStatus();
			switch(homeState.Value)
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
			switch(encounterListStatus)
			{
				case RequestStatus.Cancel:
					LoadEncounterList();
					return;
				case RequestStatus.Unknown:
					return;
			}
			if (string.IsNullOrEmpty(homeSelectedEncounterPath.Value)) return;

			switch(selectedEncounterStatus)
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
					foreach (var info in encounterList) OnDrawEncounterInfo(info);
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
					EditorGUILayout.HelpBox("Select an encounter to edit.", MessageType.Info);
				}
				GUILayout.EndVertical();
				return;
			}

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

				EditorGUI.BeginChangeCheck();
				{
					model.EncounterId.Value = EditorGUILayout.TextField("Encounter Id", model.EncounterId.Value);
					model.Name.Value = EditorGUILayout.TextField("Name", model.Name.Value);
					model.Meta.Value = model.Name;
					model.Description.Value = EditorGUILayout.TextField("Description", model.Description.Value);
					model.CompletedEncountersRequired.Value = EditorGUILayoutExtensions.StringArray(
						"Completed Encounters Required",
						model.CompletedEncountersRequired.Value,
						"- Encounter Id -"
					);
					model.ValidSystems.Value = EditorGUILayoutExtensions.EnumArray(
						"Valid Systems",
						model.ValidSystems.Value,
						"- Select a SystemType -"
					);
					model.ValidBodies.Value = EditorGUILayoutExtensions.EnumArray(
						"Valid Bodies",
						model.ValidBodies.Value,
						"- Select a BodyType -"
					);
					model.ValidProbes.Value = EditorGUILayoutExtensions.EnumArray(
						"Valid Probes",
						model.ValidProbes.Value,
						"- Select a ProbeType -",
						options: InventoryValidator.Probes
					);
					model.ValidCrews.Value = EditorGUILayoutExtensions.EnumArray(
						"Valid Crews",
						model.ValidCrews.Value,
						"- Select a CrewType -",
						options: InventoryValidator.Crews
					);
				}
				selectedEncounterModified |= EditorGUI.EndChangeCheck();

			}
			GUILayout.EndVertical();
		}

		void NewEncounter()
		{
			var info = SaveLoadService.Create<EncounterInfoModel>();
			info.EncounterId.Value = Guid.NewGuid().ToString();
			info.Name.Value = string.Empty;
			info.Meta.Value = info.Name;
			SaveLoadService.Save(info, OnNewEncounterInfo);
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
			SaveLoadService.Save(model, OnSaveSelectedEncounter);
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
		}

		void OnDrawEncounterInfo(SaveModel info)
		{
			var infoPath = info.IsInternal ? info.InternalPath : info.Path;
			var infoName = Path.GetFileName(infoPath);
			GUILayout.Label(new GUIContent(infoName, infoPath));
			EditorGUILayout.LabelField("Meta", string.IsNullOrEmpty(info.Meta) ? "< No Meta >" : info.Meta);
			GUILayout.BeginHorizontal();
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
			GUILayout.EndHorizontal();
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