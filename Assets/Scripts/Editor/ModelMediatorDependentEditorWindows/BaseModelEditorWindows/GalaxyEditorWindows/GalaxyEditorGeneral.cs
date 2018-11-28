using System.IO;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GalaxyEditorWindow
	{
		DevPrefsInt generalPreviewSize;
		EditorPrefsBool generalPreviewMinimized;
		EditorPrefsFloat generalPreviewBarScroll;

		void GeneralConstruct()
		{
			var currPrefix = KeyPrefix + "General";

			generalPreviewSize = new DevPrefsInt(currPrefix + "PreviewSize");
			generalPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");
			generalPreviewBarScroll = new EditorPrefsFloat(currPrefix + "PreviewBarScroll");

			RegisterToolbar("General", GeneralToolbar);
		}

		void GeneralToolbar(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				model.IsPlayable.Value = EditorGUILayout.Toggle(new GUIContent("Is Playable", "Can the player start a game in this galaxy?"), model.IsPlayable.Value);

				model.GalaxyId.Value = model.SetMetaKey(MetaKeyConstants.GalaxyInfo.GalaxyId, EditorGUILayout.TextField("Galaxy Id", model.GalaxyId.Value));

				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;

				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The internal description for notes and production purposes."), model.Description.Value, leftOffset: false);

				model.EncyclopediaEntryId.Value = EditorGUILayout.TextField(new GUIContent("Encyclopedia Entry Id", "The encyclopedia entry opened when viewing the details of this galaxy."), model.EncyclopediaEntryId.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal(EditorStyles.helpBox);
			{
				GUILayout.FlexibleSpace();
				var previewText = generalPreviewMinimized.Value ? "v" : "^";
				if (generalPreviewMinimized.Value) GUILayout.Label("Expand Preview");
				else
				{
					GUILayout.Label(new GUIContent("Preview Size"), GUILayout.Width(78f));
					generalPreviewSize.Value = Mathf.Clamp(EditorGUILayout.IntSlider(generalPreviewSize.Value, PreviewConstants.MinimumSize, PreviewConstants.MaximumSize, GUILayout.Width(150f)), PreviewConstants.MinimumSize, PreviewConstants.MaximumSize);
				}
				if (GUILayout.Button(previewText, GUILayout.Width(24f))) generalPreviewMinimized.Value = !generalPreviewMinimized.Value;
			}
			GUILayout.EndHorizontal();

			if (generalPreviewMinimized.Value) return;

			generalPreviewBarScroll.Value = GUILayout.BeginScrollView(new Vector2(generalPreviewBarScroll, 0f), true, false, GUILayout.MinHeight(generalPreviewSize + 46f)).x;
			{
				GUILayout.BeginHorizontal();
				{
					var biggest = 0;
					foreach (var kv in model.Textures)
					{
						if (kv.Value == null) continue;
						biggest = Mathf.Max(biggest, Mathf.Max(kv.Value.width, kv.Value.height));
					}

					var editorButtonStyle = new GUIStyle(EditorStyles.miniButton);

					foreach (var kv in model.Textures)
					{
						if (kv.Value == null) continue;
						GUILayout.BeginVertical();
						{
							var largestDimension = Mathf.Max(kv.Value.width, kv.Value.height);
							var isUpsized = largestDimension < generalPreviewSize;
							var isDownsized = generalPreviewSize < largestDimension;

							var labelText = kv.Key + " | " + kv.Value.width + " x " + kv.Value.height;

							if (isUpsized) labelText += " ( Scaled Up )";
							else if (isDownsized) labelText += " ( Scaled Down )";

							if (isUpsized || isDownsized) EditorGUILayoutExtensions.PushColor(Color.yellow);
							var labelClicked = GUILayout.Button(labelText);
							if (isUpsized || isDownsized) EditorGUILayoutExtensions.PopColor();

							var currStyle = new GUIStyle(editorButtonStyle);
							currStyle.normal.background = kv.Value;
							currStyle.active.background = kv.Value;

							if (GUILayout.Button(GUIContent.none, currStyle, GUILayout.Width(generalPreviewSize), GUILayout.Height(generalPreviewSize)) || labelClicked)
							{
								var textureWithExtension = kv.Key + ".png";
								var texturePath = Path.Combine(model.IsInternal ? model.InternalSiblingDirectory : model.SiblingDirectory, textureWithExtension);
								EditorUtility.FocusProjectWindow();
								Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
							}
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
	}
}