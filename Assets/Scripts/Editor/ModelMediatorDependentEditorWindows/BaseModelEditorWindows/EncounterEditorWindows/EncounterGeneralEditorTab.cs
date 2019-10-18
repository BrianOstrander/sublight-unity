using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterGeneralEditorTab : ModelEditorTab<EncounterEditorWindow, EncounterInfoModel>
	{
		public EncounterGeneralEditorTab(EncounterEditorWindow window) : base(window, "General") { }

		public override void Gui(EncounterInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.OrderWeight.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Order Weight", "Specifies which grouping of encounters this is sorted and randomly chosen from."), model.OrderWeight.Value, GUILayout.ExpandWidth(true)));
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();
				model.RandomWeightMultiplier.Value = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Random Weight Multiplier", "The chance of it appearing relative to others among its Order Weight grouping."), model.RandomWeightMultiplier.Value));
				model.RandomAppearance.Value = Mathf.Clamp01(EditorGUILayout.FloatField(new GUIContent("Random Appearance", "Chance it won't be shown at all each time encounters are chosen."), model.RandomAppearance.Value));
				EditorGUILayoutModel.Id(model);
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The internal description for notes and production purposes."), model.Description.Value, leftOffset: false);

				Color? triggerColor = null;
				var triggerContent = new GUIContent("Trigger", "What triggers this encounter to appear upon entering a system with it.");
				switch (model.Trigger.Value)
				{
					case EncounterTriggers.Unknown:
						triggerColor = Color.red;
						triggerContent.tooltip = "A trigger for this encounter must be selected.";
						break;
					case EncounterTriggers.None:
						triggerColor = Color.yellow;
						triggerContent.tooltip = "This encounter will never be shown unless specified by a system.";
						break;
				}

				model.Trigger.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					triggerContent,
					"- Select a Trigger -",
					model.Trigger.Value,
					result => triggerColor
				);

				var alternateColor = Color.grey;

				EditorGUILayoutValueFilter.Field(new GUIContent("Filtering", "These checks determine if the encounter will be selected."), model.Filtering, alternateColor);
			}
			EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
		}
	}
}