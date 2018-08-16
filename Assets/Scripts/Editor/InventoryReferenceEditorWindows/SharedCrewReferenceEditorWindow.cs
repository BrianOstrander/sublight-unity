using UnityEditor;

using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class InventoryReferenceEditorWindow
	{
		void OnEditCrewShared(CrewInventoryModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				model.SupportedBodies.Value = EditorGUILayoutExtensions.EnumArray(
					new GUIContent("Supported Bodies", "A list of all valid bodies this vessel can explore."),
					model.SupportedBodies.Value,
					"- Select a BodyType -"
				);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedReferenceModified);
		}
	}
}