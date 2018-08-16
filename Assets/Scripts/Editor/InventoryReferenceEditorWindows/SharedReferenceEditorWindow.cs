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
				model.Hidden.Value = EditorGUILayout.Toggle("Hidden", model.Hidden.Value);
				model.InventoryId.Value = reference.SetMetaKey(MetaKeyConstants.InventoryReference.InventoryId, EditorGUILayout.TextField("Inventory Id", model.InventoryId.Value));
				model.Name.Value = EditorGUILayout.TextField("Name", model.Name.Value);
				reference.Meta.Value = model.Name;
				model.Description.Value = EditorGUILayout.TextField("Description", model.Description.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref selectedReferenceModified);

			GUILayout.Box(GUIContent.none, EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.Height(16f));
		}
	}
}