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
		DevPrefsInt targetsSelectedPreview;
		DevPrefsInt targetsPreviewSize;
		EditorPrefsBool targetsPreviewMinimized;

		void OnTargetsConstruct()
		{
			var currPrefix = KeyPrefix + "Targets";

			targetsSelectedPreview = new DevPrefsInt(currPrefix + "SelectedPreview");
			targetsPreviewSize = new DevPrefsInt(currPrefix + "PreviewSize");
			targetsPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");

			RegisterToolbar("Targets", OnTargetsToolbar);
		}

		void OnTargetsToolbar(GalaxyInfoModel model)
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
				EditorGUILayoutExtensions.PushBackgroundColor(Color.green);
				model.PlayerStart = EditorGUILayoutUniversePosition.FieldSector("Player Start", model.PlayerStart);
				EditorGUILayoutExtensions.PopBackgroundColor();
				EditorGUILayoutExtensions.PushBackgroundColor(Color.red);
				model.GameEnd = EditorGUILayoutUniversePosition.FieldSector("Game End", model.GameEnd);
				EditorGUILayoutExtensions.PopBackgroundColor();

				model.UniverseNormal.Value = EditorGUILayout.Vector3Field(new GUIContent("Universe Normal", "The up direction of this galaxy within the universe."), model.UniverseNormal.Value);
				model.AlertHeightMultiplier.Value = EditorGUILayout.FloatField(new GUIContent("Alert Height Multiplier", "The additional offset of any alerts on this galaxy."), model.AlertHeightMultiplier.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			GUILayout.FlexibleSpace();

			DrawPreviews(
				model,
				targetsSelectedPreview,
				targetsPreviewSize,
				targetsPreviewMinimized,
				true,
				clickPosition => OnTargetsPrimaryClickPreview(model, clickPosition),
				drawOnPreview: displayArea => DrawGalaxyTargets(model, displayArea, SubLightEditorConfig.Instance.GalaxyTargetStyle)
			);
		}

		void OnTargetsPrimaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition)
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
						() => { model.PlayerStartNormal = clickPosition; ModelSelectionModified = true; },
						color: Color.green
					),
					OptionDialogPopup.Entry.Create(
						"Game End",
						() => { model.GameEndNormal = clickPosition; ModelSelectionModified = true; },
						color: Color.red
					)
				},
				description: "Select the following position to assign the value of ( " + clickPosition.x + " , " + clickPosition.z + " ) to."
			);
		}
	}
}