﻿using UnityEditor;

using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class InventoryReferenceEditorWindow
	{
		void OnEditCrewShared(CrewInventoryModel model)
		{
			EditorGUI.BeginChangeCheck();
			{
				model.SupportedBodies.Value = EditorGUILayoutExtensions.EnumArray(
					new GUIContent("Supported Bodies", "A list of all valid bodies this vessel can explore."),
					model.SupportedBodies.Value,
					"- Select a BodyType -"
				);
			}
			selectedReferenceModified |= EditorGUI.EndChangeCheck();
		}
	}
}