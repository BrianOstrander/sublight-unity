using System;

using LunraGamesEditor;

using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	public partial class LanguageEditorWindow : ModelMediatorDependentEditorWindow
	{
		public enum States
		{
			Unknown = 0,
			Home = 10
		}

		EditorPrefsEnum<States> currentState;

		[MenuItem("Window/SubLight/Language Editor")]
		static void Initialize()
		{
			GetWindow(typeof(LanguageEditorWindow), false, "Language Editor").Show();
		}

		public LanguageEditorWindow() : base("LG_SF_LanguageEditor_")
		{
			currentState = new EditorPrefsEnum<States>(KeyPrefix + "State", States.Home);

			OnHomeConstruct();

			Gui += OnLanguageGUI;
		}

		void OnLanguageGUI()
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
	}
}