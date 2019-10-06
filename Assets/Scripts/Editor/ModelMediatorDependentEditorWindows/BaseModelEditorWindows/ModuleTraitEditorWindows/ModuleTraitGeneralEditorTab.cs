using System;
using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ModuleTraitGeneralEditorTab : ModelEditorTab<ModuleTraitEditorWindow, ModuleTraitModel>
	{
		const float CheckCooldown = 0.5f;
		
		ModuleTraitModel currentModel;

		DateTime? checkTime;

		public ModuleTraitGeneralEditorTab(ModuleTraitEditorWindow window) : base(window, "General") { }

		public override void Gui(ModuleTraitModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();

				EditorGUILayoutModel.Id(model);

				model.FamilyIds.Value = EditorGUILayoutExtensions.StringArray(
					new GUIContent("Family Ids", "The family ids this module trait belongs to."),
					model.FamilyIds.Value
				);
				
				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The name of this trait visible to the player."), model.Name.Value);
				
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The description of this trait given to the player."), model.Description.Value, leftOffset: false);

				model.Severity.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					new GUIContent("Severity", "Critical traits will cause game ending encounters to occur."),
					"- Severity -",
					model.Severity.Value,
					Color.red
				);

				model.CompatibleModuleTypes.Value = EditorGUILayoutExtensions.EnumArray(
					new GUIContent("Compatible Modules", "All modules types this trait can be applied to. Leaving this empty will allow it to be added to any Module Type."),
					model.CompatibleModuleTypes.Value
				);

				model.IncompatibleIds.Value = EditorGUILayoutExtensions.StringArray(
					new GUIContent("Incompatible Ids", "Ids of module traits that cannot be applied to the same module as this trait."),
					model.IncompatibleIds.Value
				);
				
				model.IncompatibleFamilyIds.Value = EditorGUILayoutExtensions.StringArray(
					new GUIContent("Incompatible Family Ids", "Ids of family module traits that cannot be applied to the same module as this trait."),
					model.IncompatibleIds.Value
				);
			}
			EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
		}
		
		#region General Events
		void OnGeneralAfterLoadSelection(ModuleTraitModel model)
		{
			if (currentModel != null)
			{
				currentModel.IncompatibleIds.Changed -= OnGeneralIncompatibleTraitIds;
			}
			currentModel = model;
			checkTime = DateTime.MinValue;
			
			currentModel.IncompatibleIds.Changed += OnGeneralIncompatibleTraitIds;
			
			GeneralUpdateCheckTime();
		}
		#endregion
		
		#region Model Events
		void OnGeneralIncompatibleTraitIds(string[] value) => GeneralResetCheckTime();

		#endregion

		void GeneralUpdateCheckTime()
		{
			if (checkTime.HasValue && DateTime.Now < checkTime) return;
			checkTime = null;
			
			
		}
		
		void GeneralResetCheckTime() => checkTime = DateTime.Now + TimeSpan.FromSeconds(CheckCooldown);
	}
}