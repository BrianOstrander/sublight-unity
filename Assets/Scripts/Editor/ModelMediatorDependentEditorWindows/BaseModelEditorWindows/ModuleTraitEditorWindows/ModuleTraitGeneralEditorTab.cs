using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ModuleTraitGeneralEditorTab : ModelEditorTab<ModuleTraitEditorWindow, ModuleTraitModel>
	{
		const float GeneralCheckCooldown = 0.5f;
		
		ModuleTraitModel generalCurrentModel;

		DateTime? generalCheckTime;

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
				
				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The name of this trait visible to the player."), model.Name.Value);
				
				model.Description.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Description", "The description of this trait given to the player."), model.Description.Value, leftOffset: false);

				model.Severity.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
					new GUIContent("Severity", "Critical traits will cause game ending encounters to occur."),
					"- Severity -",
					model.Severity.Value,
					Color.red
				);

				model.CompatibleModuleTypes.Value = EditorGUILayoutExtensions.EnumArray<ModuleTypes>(
					new GUIContent("Compatible Modules", "All modules types this trait can be applied to. Leaving this empty will allow it to be added to any Module Type."),
					model.CompatibleModuleTypes.Value
				);

				// Color? incompatibleTraitIdsValidation = model.IncompatibleTraitIds.Value.Any(i => string.IsNullOrEmpty(i))
				model.IncompatibleTraitIds.Value = EditorGUILayoutExtensions.StringArray(
					"Incompatible Trait Ids",
					model.IncompatibleTraitIds.Value
				);
				
				// TODO: Other fields here...
			}
			EditorGUIExtensions.EndChangeCheck(ref Window.ModelSelectionModified);
		}
		
		#region General Events
		void OnGeneralAfterLoadSelection(ModuleTraitModel model)
		{
			if (generalCurrentModel != null)
			{
				generalCurrentModel.IncompatibleTraitIds.Changed -= OnGeneralIncompatibleTraitIds;
			}
			generalCurrentModel = model;
			generalCheckTime = DateTime.MinValue;
			
			generalCurrentModel.IncompatibleTraitIds.Changed += OnGeneralIncompatibleTraitIds;
			
			GeneralUpdateCheckTime();
		}
		#endregion
		
		#region Model Events
		void OnGeneralIncompatibleTraitIds(string[] value) => GeneralResetCheckTime();

		#endregion

		void GeneralUpdateCheckTime()
		{
			if (generalCheckTime.HasValue && DateTime.Now < generalCheckTime) return;
			generalCheckTime = null;
			
			
		}
		
		void GeneralResetCheckTime() => generalCheckTime = DateTime.Now + TimeSpan.FromSeconds(GeneralCheckCooldown);
	}
}