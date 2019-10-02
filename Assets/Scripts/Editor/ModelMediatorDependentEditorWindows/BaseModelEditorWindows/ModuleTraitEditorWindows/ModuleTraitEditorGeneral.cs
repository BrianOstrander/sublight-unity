using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class ModuleTraitEditorWindow
	{
		void GeneralConstruct()
		{
			//var currPrefix = KeyPrefix + "General";

			RegisterToolbar("General", GeneralToolbar);
		}

		void GeneralToolbar(ModuleTraitModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();

				DrawIdField(model);
				
				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The name of this trait visible to the player."), model.Name.Value);
				
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The description of this trait given to the player."), model.Description.Value, leftOffset: false);

				model.Severity.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					new GUIContent("Severity", "Critical traits will cause game ending encounters to occur."),
					"- Severity -",
					model.Severity.Value,
					Color.red
				);

				model.CompatibleModuleTypes.Value = EditorGUILayoutExtensions.EnumArray<ModuleTypes>(
					new GUIContent("Compatible Modules", "All modules types this trait can be applied to."),
					model.CompatibleModuleTypes.Value
				);
				
				// TODO: Other fields here...
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

		}
	}
}