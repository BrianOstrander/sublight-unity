using System.IO;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GamemodeEditorWindow
	{
		void GeneralConstruct()
		{
			//var currPrefix = KeyPrefix + "General";

			RegisterToolbar("General", GeneralToolbar);
		}

		void GeneralToolbar(GamemodeInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.OrderWeight.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Order Weight", "Specifies the order in which the gamemode is listed."), model.OrderWeight.Value, GUILayout.ExpandWidth(true)));
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();

				model.IsInDevelopment.Value = EditorGUILayout.Toggle("In Development", model.IsInDevelopment.Value);
				model.Id.Value = EditorGUILayout.TextField("Gamemode Id", model.Id.Value);
				
				model.Title.Value = EditorGUILayout.TextField(new GUIContent("Title", "The larger category this gamemode belongs to."), model.Title.Value);
				model.SubTitle.Value = EditorGUILayout.TextField(new GUIContent("SubTitle", "The name of this gamemode visible to the player."), model.SubTitle.Value);
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The description given to the player of this gamemode."), model.Description.Value, leftOffset: false);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			var icon = model.Icon;

			if (icon == null)
			{
				EditorGUILayout.HelpBox("No file named \"" + GamemodeInfoModel.TextureNames.Icon + ".png\" could be found.", MessageType.Error);
			}
			else
			{
				GUILayout.BeginHorizontal();
				{
					var currStyle = new GUIStyle(EditorStyles.miniButton);
					currStyle.normal.background = icon;
					currStyle.active.background = icon;

					if (GUILayout.Button(GUIContent.none, currStyle, GUILayout.Width(256f), GUILayout.Height(256f)))
					{
						var textureWithExtension = GamemodeInfoModel.TextureNames.Icon + ".png";
						var texturePath = Path.Combine(model.IsInternal ? model.InternalSiblingDirectory : model.SiblingDirectory, textureWithExtension);
						EditorUtility.FocusProjectWindow();
						Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
					}
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}