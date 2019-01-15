using System.IO;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class EncounterEditorWindow
	{
		void GeneralConstruct()
		{
			var currPrefix = KeyPrefix + "General";

			RegisterToolbar("General", GeneralToolbar);
		}

		void GeneralToolbar(EncounterInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.OrderWeight.Value = EditorGUILayout.FloatField("Order Weight", model.OrderWeight.Value, GUILayout.ExpandWidth(true));
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();
				model.RandomWeightMultiplier.Value = EditorGUILayout.FloatField("Random Weight Multiplier", model.RandomWeightMultiplier.Value);
				model.EncounterId.Value = model.SetMetaKey(MetaKeyConstants.EncounterInfo.EncounterId, EditorGUILayout.TextField("Encounter Id", model.EncounterId.Value));
				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The internal description for notes and production purposes."), model.Description.Value, leftOffset: false);
				model.Hook.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Hook", "The description given to the player before entering this encounter."), model.Hook.Value, leftOffset: false);

				model.Trigger.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(new GUIContent("Trigger", "What triggers this encounter to appear upon entering a system with it."), "- Select a Trigger -", model.Trigger.Value);
				if (model.Trigger.Value == EncounterTriggers.Unknown) EditorGUILayout.HelpBox("A trigger for this encounter must be selected.", MessageType.Error);

				var alternateColor = Color.grey;

				EditorGUILayoutValueFilter.Field(new GUIContent("Filtering", "These checks determine if the encounter will be selected."), model.Filtering, alternateColor);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
		}
	}
}