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
	public partial class GalaxyEditorWindow
	{
		EditorPrefsInt targetsSelectedPreview;
		EditorPrefsInt targetsPreviewSize;
		EditorPrefsBool targetsPreviewMinimized;

		void TargetsConstruct()
		{
			var currPrefix = KeyPrefix + "Targets";

			targetsSelectedPreview = new EditorPrefsInt(currPrefix + "SelectedPreview");
			targetsPreviewSize = new EditorPrefsInt(currPrefix + "PreviewSize");
			targetsPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");

			RegisterToolbar("Targets", TargetsToolbar);
		}

		void TargetsToolbar(GalaxyInfoModel model)
		{
			if (HorizontalPreviewSupported())
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical();
					{
						TargetsToolbarPrimary(model);
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
					{
						TargetsToolbarSecondary(model);
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				TargetsToolbarPrimary(model);
				GUILayout.FlexibleSpace();
				TargetsToolbarSecondary(model);
			}
		}

		void TargetsToolbarPrimary(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.GalaxyRadius = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Galaxy Radius", "Galaxy radius in sectors"), model.GalaxyRadius, GUILayout.Width(250f)));
					GUILayout.Label("( " + UniversePosition.ToLightYearDistance(model.GalaxyRadius).ToString("N0") + " Light Years )");
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				model.ClusterOrigin = EditorGUILayoutUniversePosition.FieldSector("Cluster Origin", model.ClusterOrigin);
				EditorGUILayoutExtensions.PushBackgroundColor(Color.yellow);
				model.GalaxyOrigin = EditorGUILayoutUniversePosition.FieldSector("Galaxy Origin", model.GalaxyOrigin);
				EditorGUILayoutExtensions.PopBackgroundColor();

				var foundBegin = false;
				var foundEnd = false;

				var wasBegin = model.GetPlayerBegin(out foundBegin);
				var wasEnd = model.GetPlayerEnd(out foundEnd);

				if (!foundBegin)
				{
					EditorGUILayout.HelpBox("Specify a begin sector and system in the Specified Sectors tab", MessageType.Error);

				}
				else
				{
					EditorGUILayoutExtensions.PushBackgroundColor(Color.green);
					var newBegin = EditorGUILayoutUniversePosition.FieldSector("Player Begin", model.GetPlayerBegin());
					EditorGUILayoutExtensions.PopBackgroundColor();

					if (!newBegin.Equals(wasBegin)) model.SetPlayerBegin(newBegin);
				}

				if (!foundEnd)
				{
					EditorGUILayout.HelpBox("Specify an end sector and system in the Specified Sectors tab", MessageType.Error);
				}
				else
				{
					EditorGUILayoutExtensions.PushBackgroundColor(Color.red);
					var newEnd = EditorGUILayoutUniversePosition.FieldSector("Player End", model.GetPlayerEnd());
					EditorGUILayoutExtensions.PopBackgroundColor();

					if (!newEnd.Equals(wasEnd)) model.SetPlayerEnd(newEnd);
				}

				model.UniverseNormal.Value = EditorGUILayout.Vector3Field(new GUIContent("Universe Normal", "The up direction of this galaxy within the universe."), model.UniverseNormal.Value);
				model.AlertHeightMultiplier.Value = EditorGUILayout.FloatField(new GUIContent("Alert Height Multiplier", "The additional offset of any alerts on this galaxy."), model.AlertHeightMultiplier.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
		}

		void TargetsToolbarSecondary(GalaxyInfoModel model)
		{
			DrawPreviews(
				model,
				targetsSelectedPreview,
				targetsPreviewSize,
				targetsPreviewMinimized,
				true,
				clickPosition => TargetsPrimaryClickPreview(model, clickPosition),
				drawOnPreview: displayArea => DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyTargetStyle)
			);
		}

		void TargetsPrimaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition)
		{
			OptionDialogPopup.Show(
				"Set Target",
				new OptionDialogPopup.Entry[]
				{
					OptionDialogPopup.Entry.Create(
						"Galaxy Origin",
						() => { model.GalaxyOriginNormal = clickPosition; ModelSelectionModified = true; },
						color: Color.yellow
					),
					OptionDialogPopup.Entry.Create(
						"Player Start",
						() => TargetsPrimaryClickSetPlayerBegin(model, clickPosition),
						color: Color.green
					),
					OptionDialogPopup.Entry.Create(
						"Game End",
						() => TargetsPrimaryClickSetPlayerEnd(model, clickPosition),
						color: Color.red
					)
				},
				description: "Select the following position to assign the value of ( " + clickPosition.x + " , " + clickPosition.z + " ) to."
			);
		}

		void TargetsPrimaryClickSetPlayerBegin(GalaxyInfoModel model, Vector3 clickPosition)
		{
			var found = false;
			model.GetPlayerBegin(out found);

			if (found)
			{
				model.PlayerBeginNormal = clickPosition;
				ModelSelectionModified = true;
				return;
			}
			EditorUtility.DisplayDialog("Missing System", "Specify a begin sector and system in the Specified Sectors tab first.", "Okay");
		}

		void TargetsPrimaryClickSetPlayerEnd(GalaxyInfoModel model, Vector3 clickPosition)
		{
			var found = false;
			model.GetPlayerEnd(out found);

			if (found)
			{
				model.PlayerEndNormal = clickPosition;
				ModelSelectionModified = true;
				return;
			}
			EditorUtility.DisplayDialog("Missing System", "Specify an end sector and system in the Specified Sectors tab first.", "Okay");
		}
	}
}