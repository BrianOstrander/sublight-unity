using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class InventoryReferenceEditorWindow
	{
		void OnRefrenceHeader<S, T>(S reference, string typeName)
			where S : InventoryReferenceModel<T>
			where T : InventoryModel
		{
			var model = reference.Model.Value;

			var referenceName = string.IsNullOrEmpty(model.Name) ? "< No Name > " : model.Name;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Editing "+typeName+": " + referenceName);
				GUI.enabled = selectedReferenceModified;
				if (GUILayout.Button("Save", GUILayout.Width(64f))) SaveSelectedReference(reference);
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();

			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					model.RandomWeightMultiplier.Value = EditorGUILayout.FloatField("Random Weight Multiplier", model.RandomWeightMultiplier.Value, GUILayout.ExpandWidth(true));
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();

				model.InventoryId.Value = reference.SetMetaKey(MetaKeyConstants.InventoryReference.InventoryId, EditorGUILayout.TextField("Inventory Id", model.InventoryId.Value));
				model.Name.Value = EditorGUILayout.TextField("Name", model.Name.Value);
				reference.Meta.Value = model.Name;
				model.Description.Value = EditorGUILayout.TextField("Description", model.Description.Value);
				model.Tags.Value = EditorGUILayoutExtensions.StringArray(
					"Tags",
					model.Tags.Value
				);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedReferenceModified);

			GUILayout.Box(GUIContent.none, EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.Height(16f));
		}
	}
}