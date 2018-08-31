using System;

using LunraGamesEditor;

using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	public partial class EncounterEditorWindow : EditorWindow
	{
		const string KeyPrefix = "LG_SF_EncounterEditor_";

		public enum States
		{
			Unknown = 0,
			Home = 10
		}

		EditorPrefsEnum<States> currentState = new EditorPrefsEnum<States>(KeyPrefix + "State", States.Home);

		EditorModelMediator editorSaveLoadService;
		IModelMediator SaveLoadService 
		{
			get 
			{
				if (editorSaveLoadService == null)
				{
					editorSaveLoadService = new EditorModelMediator(true);
					editorSaveLoadService.Initialize(BuildPreferences.Instance.Info, OnSaveLoadInitialized);
				}
				return editorSaveLoadService;
			}
		}

		void OnSaveLoadInitialized(RequestStatus status)
		{
			if (status == RequestStatus.Success) return;
			Debug.LogError("Editor time save load service returned: " + status);
		}

		[MenuItem("Window/SubLight/Encounter Editor")]
		static void Initialize()
		{
			GetWindow(typeof(EncounterEditorWindow), false, "Encounter Editor").Show();
		}

		void OnEnable()
		{
			OnHomeEnable();
		}

		void OnDisable()
		{
			OnHomeDisable();
		}

		void OnGUI()
		{
			Exception innerException = null;
			try
			{
				try
				{
					switch (currentState.Value)
					{
						case States.Home:
							OnHome();
							break;
						default:
							OnUnknown();
							break;
					}
				}
				catch (Exception originalException)
				{
					innerException = originalException;
					GUIUtility.ExitGUI();
				}
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch (ExitGUIException exitException)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				if (innerException.Message.StartsWith("Getting control ")) return;
				EditorGUILayout.HelpBox("Exception occured: \n" + innerException.Message, MessageType.Error);
				Debug.LogException(innerException);
				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Print Exception")) Debug.LogException(innerException);
					GUI.color = Color.red;
					if (GUILayout.Button("Reset")) Reset();
				}
				GUILayout.EndHorizontal();
			}
		}

		void OnUnknown()
		{
			GUILayout.BeginVertical();
			{
				GUILayout.Label("Unknown state: " + currentState.Value);
				if (GUILayout.Button("Reset")) Reset();
			}
			GUILayout.EndVertical();
		}

		void Reset()
		{
			currentState.Value = States.Home;
		}

		void Space()
		{
			GUILayout.Space(16f);
		}
	}
}