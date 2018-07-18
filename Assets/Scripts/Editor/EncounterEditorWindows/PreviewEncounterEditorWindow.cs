using System.Linq;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Presenters;

namespace LunraGames.SpaceFarm
{
	public partial class EncounterEditorWindow
	{
		string encounterPreviewPresenterId;
		ContainerEncounterLogPresenter encounterPreviewPresenter;

		void OnHomeSelectedPreview(EncounterInfoModel model)
		{
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Play game to preview encounters.", MessageType.Info);
				return;
			}
			if (App.SM == null || App.SM.CurrentState == StateMachine.States.Initialize || App.SM.CurrentEvent != StateMachine.Events.Idle)
			{
				EditorGUILayout.HelpBox("Waiting for game to initialize...", MessageType.Info);
				return;
			}

			if (encounterPreviewPresenter == null)
			{
				EditorGUILayoutExtensions.PushColor(Color.red);
				GUILayout.Label("Preview presenter null.");
			}
			else if (encounterPreviewPresenterId != model.EncounterId.Value)
			{
				EditorGUILayoutExtensions.PushColor(Color.yellow);
				GUILayout.Label("Preview presenter instantiated with a different model.");
			}
			else
			{
				EditorGUILayoutExtensions.PushColor(Color.green);
				GUILayout.Label("Preview Presenter Instantiated.");
			}
			EditorGUILayoutExtensions.PopColor();

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Show"))
				{
					Debug.Log("todo: call encounter focus event");
					/*
					if (encounterPreviewPresenter == null)
					{
						encounterPreviewPresenter = new ContainerEncounterLogPresenter(new GameModel(), model);
						encounterPreviewPresenterId = model.EncounterId;
					}
					else
					{
						App.P.UnRegister(encounterPreviewPresenter);
						encounterPreviewPresenter = new ContainerEncounterLogPresenter(new GameModel(), model);
						encounterPreviewPresenterId = model.EncounterId;
					}
					encounterPreviewPresenter.Show();
					*/
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}