using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class InventoryReferenceEditorWindow
	{
		enum HomeStates
		{
			Unknown = 0,
			Browsing = 10,
			Selected = 20
		}

		EditorPrefsFloat homeLeftBarScroll = new EditorPrefsFloat(KeyPrefix + "LeftBarScroll");

		EditorPrefsBool homeLeftBarShowModules = new EditorPrefsBool(KeyPrefix + "LeftBarShowModules");
		EditorPrefsBool homeLeftBarShowCrewOrbitals = new EditorPrefsBool(KeyPrefix + "LeftBarShowCrewOrbitals");

		EditorPrefsEnum<SaveTypes> homeSelectedSaveType = new EditorPrefsEnum<SaveTypes>(KeyPrefix + "SelectedSaveType");
		EditorPrefsString homeSelectedPath = new EditorPrefsString(KeyPrefix + "HomeSelectedReference");

		EditorPrefsEnum<HomeStates> homeState = new EditorPrefsEnum<HomeStates>(KeyPrefix + "HomeState", HomeStates.Browsing);

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		Dictionary<SaveTypes, RequestStatus> referenceListStatuses;
		Dictionary<SaveTypes, SaveModel[]> referenceLists;

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus selectedReferenceStatus;
		object selectedReference;
		bool selectedReferenceModified;

		void OnHomeEnable()
		{
			referenceListStatuses = new Dictionary<SaveTypes, RequestStatus>();
			referenceLists = new Dictionary<SaveTypes, SaveModel[]>
			{
				{ SaveTypes.ModuleReference, new SaveModel[0] },
				{ SaveTypes.OrbitalCrewReference, new SaveModel[0] }
			};

			OnHomeSetReferenceListStatuses();
			selectedReferenceStatus = RequestStatus.Cancel;

			EditorApplication.modifierKeysChanged += OnHomeModifierKeysChanged;
		}

		void OnHomeDisable()
		{
			EditorApplication.modifierKeysChanged -= OnHomeModifierKeysChanged;
		}

		void OnHomeSetReferenceListStatuses(RequestStatus status = RequestStatus.Cancel)
		{
			referenceListStatuses.Clear();
			referenceListStatuses.Add(SaveTypes.ModuleReference, status);
			referenceListStatuses.Add(SaveTypes.OrbitalCrewReference, status);
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
			if (referenceListStatuses.Any(kv => kv.Value == RequestStatus.Cancel))
			{
				foreach (var kv in referenceListStatuses.ToList())
				{
					if (kv.Value == RequestStatus.Cancel) referenceListStatuses[kv.Key] = RequestStatus.Unknown;
				}
				LoadReferenceList();
				return;
			}
			if (referenceListStatuses.Any(kv => kv.Value == RequestStatus.Unknown))
			{
				return;
			}

			if (string.IsNullOrEmpty(homeSelectedPath.Value)) return;

			switch (selectedReferenceStatus)
			{
				case RequestStatus.Cancel:
					LoadSelectedReference(referenceLists[homeSelectedSaveType.Value].FirstOrDefault(m => m.Path == homeSelectedPath.Value));
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
				OnHomeRightPane(selectedReference);
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
					var newOptions = new SaveTypes[] {
						SaveTypes.Unknown,
						SaveTypes.ModuleReference,
						SaveTypes.OrbitalCrewReference
					};

					var newSelection = EditorGUILayoutExtensions.HelpfulEnumPopup("- Create New Inventory Reference -", SaveTypes.Unknown, newOptions);
					if (newSelection != SaveTypes.Unknown) NewReference(newSelection);

					if (GUILayout.Button("Refresh"))
					{
						OnHomeSetReferenceListStatuses(RequestStatus.Unknown);
						LoadReferenceList();
					}
				}
				GUILayout.EndHorizontal();

				homeLeftBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, homeLeftBarScroll), false, true).y;
				{
					foreach (var kv in referenceListStatuses)
					{
						EditorPrefsBool isShowing = null;
						switch(kv.Key)
						{
							case SaveTypes.ModuleReference:
								isShowing = homeLeftBarShowModules;
								break;
							case SaveTypes.OrbitalCrewReference:
								isShowing = homeLeftBarShowCrewOrbitals;
								break;
							default:
								Debug.LogError("Unexpected SaveType: " + kv.Key);
								break;
						}
						if (isShowing == null) continue;
						isShowing.Value = EditorGUILayout.Foldout(isShowing.Value, kv.Key.ToString());
						if (!isShowing.Value) continue;

						var currentList = referenceLists[kv.Key];
						if (currentList.Length == 0)
						{
							EditorGUILayout.HelpBox("No Inventory References of this type.", MessageType.Info);
						}
						else
						{
							GUILayout.BeginHorizontal();
							{
								GUILayout.Space(16f);
								GUILayout.BeginVertical();
								{
									var isAlternate = false;
									foreach (var reference in referenceLists[kv.Key]) OnDrawReferenceEntry(reference, ref isAlternate);
								}
								GUILayout.EndVertical();
							}
							GUILayout.EndHorizontal();
						}
					}
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		void OnHomeRightPane(object reference = null)
		{
			if (reference == null)
			{
				GUILayout.BeginVertical();
				{
					EditorGUILayout.HelpBox("Select an inventory reference to edit.", MessageType.Info);
				}
				GUILayout.EndVertical();
				return;
			}
			GUILayout.BeginVertical();
			{
				switch (homeSelectedSaveType.Value)
				{
					case SaveTypes.ModuleReference:
						OnEditModule(reference as ModuleReferenceModel);
						break;
					case SaveTypes.OrbitalCrewReference:
						OnEditReference(reference as OrbitalCrewReferenceModel);
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized SaveType: " + homeSelectedSaveType.Value, MessageType.Error);
						break;
				}
			}
			GUILayout.EndVertical();
		}

		void NewReference(SaveTypes referenceType)
		{
			switch(referenceType)
			{
				case SaveTypes.ModuleReference:
					ConfigureReference<ModuleReferenceModel, ModuleInventoryModel>(SaveLoadService.Create<ModuleReferenceModel>());
					break;
				case SaveTypes.OrbitalCrewReference:
					ConfigureReference<OrbitalCrewReferenceModel, OrbitalCrewInventoryModel>(SaveLoadService.Create<OrbitalCrewReferenceModel>());
					break;
				default:
					Debug.LogError("Unrecognized SaveType: " + referenceType);
					break;
			}
		}

		void ConfigureReference<S, T>(S reference) 
			where S : InventoryReferenceModel<T>
			where T : InventoryModel, new()
		{
			reference.Model.Value = new T();
			reference.Model.Value.InventoryId.Value = Guid.NewGuid().ToString();
			reference.Model.Value.Name.Value = string.Empty;
			SaveLoadService.Save(reference, OnNewReference);
		}

		void LoadReferenceList()
		{
			foreach(var kv in referenceListStatuses.Where(e => e.Value == RequestStatus.Unknown).ToList())
			{
				switch(kv.Key)
				{
					case SaveTypes.ModuleReference:
						SaveLoadService.List<ModuleReferenceModel>(result => OnLoadReferenceList(kv.Key, result));
						break;
					case SaveTypes.OrbitalCrewReference:
						SaveLoadService.List<OrbitalCrewReferenceModel>(result => OnLoadReferenceList(kv.Key, result));
						break;
					default:
						Debug.LogError("Unrecognized SaveType: " + kv.Key);
						break;
				}
			}
		}

		void LoadSelectedReference(SaveModel reference)
		{
			if (reference == null)
			{
				OnDeselectReference();
				return;
			}
			homeSelectedSaveType.Value = reference.SaveType;
			homeSelectedPath.Value = reference.Path;
			selectedReferenceStatus = RequestStatus.Unknown;

			switch(reference.SaveType)
			{
				case SaveTypes.ModuleReference:
					SaveLoadService.Load<ModuleReferenceModel>(reference, OnLoadSelectedReference);
					break;
				case SaveTypes.OrbitalCrewReference:
					SaveLoadService.Load<OrbitalCrewReferenceModel>(reference, OnLoadSelectedReference);
					break;
				default:
					Debug.LogError("Unrecognized SaveType: " + reference.SaveType);
					break;
			}
		}

		void SaveSelectedReference<T>(T reference)
			where T : SaveModel
 		{
			SaveLoadService.Save(reference, OnSaveSelectedReference, false);
		}

		void OnSaveSelectedReference<T>(SaveLoadRequest<T> result) where T : SaveModel
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			homeSelectedSaveType.Value = result.Model.SaveType;
			selectedReference = result.TypedModel;
			selectedReferenceModified = false;
			selectedReferenceStatus = RequestStatus.Cancel;
			homeSelectedPath.Value = result.Model.Path.Value;

		}

		void OnNewReference<T>(SaveLoadRequest<T> result) where T : SaveModel
		{
			selectedReferenceStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			AssetDatabase.Refresh();
			homeSelectedSaveType.Value = result.Model.SaveType;
			selectedReference = result.TypedModel;
			selectedReferenceModified = false;
			selectedReferenceStatus = RequestStatus.Cancel;
			homeSelectedPath.Value = result.Model.Path.Value;

			referenceListStatuses[result.Model.SaveType] = RequestStatus.Cancel;
		}

		void OnLoadReferenceList(SaveTypes saveType, SaveLoadArrayRequest<SaveModel> result)
		{
			referenceListStatuses[saveType] = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			referenceLists[saveType] = result.Models;

			if (selectedReference == null || referenceListStatuses.Any(kv => kv.Value == RequestStatus.Unknown)) return;
			if (referenceLists[homeSelectedSaveType.Value].FirstOrDefault(r => r.Path.Value == homeSelectedPath.Value) != null) return;
			OnDeselectReference();
		}

		void OnDeselectReference()
		{
			homeState.Value = HomeStates.Browsing;
			selectedReferenceStatus = RequestStatus.Failure;
			selectedReference = null;
			selectedReferenceModified = false;
			homeSelectedPath.Value = null;
			homeSelectedSaveType.Value = SaveTypes.Unknown;
		}

		void OnDrawReferenceEntry(SaveModel reference, ref bool isAlternate)
		{
			if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.grey);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (isAlternate) EditorGUILayoutExtensions.PopColor();
			{
				var infoPath = reference.IsInternal ? reference.InternalPath : reference.Path;
				var infoName = Path.GetFileNameWithoutExtension(infoPath);
				if (20 < infoName.Length) infoName = infoName.Substring(0, 20) + "...";
				else infoName += Path.GetExtension(infoPath);

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical();
					{
						GUILayout.Label(new GUIContent(string.IsNullOrEmpty(reference.Meta) ? "< No Meta >" : reference.Meta, "Name is set by Meta field."), EditorStyles.boldLabel);
						GUILayout.Label(new GUIContent(infoName, infoPath));
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{
						if (GUILayout.Button("Edit"))
						{
							LoadSelectedReference(reference);
						}
						if (GUILayout.Button("Select In Project"))
						{
							EditorUtility.FocusProjectWindow();
							Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(reference.InternalPath);
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			isAlternate = !isAlternate;
		}

		void OnLoadSelectedReference(SaveLoadRequest<ModuleReferenceModel> result)
		{
			OnLoadSelectedReferenceUnTyped(result);
		}

		void OnLoadSelectedReference(SaveLoadRequest<OrbitalCrewReferenceModel> result)
		{
			OnLoadSelectedReferenceUnTyped(result);
		}

		void OnLoadSelectedReferenceUnTyped<T>(SaveLoadRequest<T> result) where T : SaveModel
		{
			GUIUtility.keyboardControl = 0;
			selectedReferenceStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			homeSelectedSaveType.Value = result.Model.SaveType;
			selectedReferenceModified = false;
			homeState.Value = HomeStates.Selected;
			homeSelectedPath.Value = result.Model.Path;
			selectedReference = result.TypedModel;
		}
	}
}