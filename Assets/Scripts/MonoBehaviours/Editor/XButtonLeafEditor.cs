using UnityEditor;

namespace LunraGames.SubLight
{
	[CustomEditor(typeof(XButtonLeaf), true)]
	[CanEditMultipleObjects]
	public class XButtonLeafEditor : Editor
	{
		SerializedProperty globalStyleProperty;
		SerializedProperty localStyleProperty;
		SerializedProperty targetTogglesProperty;
		SerializedProperty targetTransfomsProperty;
		SerializedProperty targetGraphicsProperty;
		SerializedProperty targetMeshRenderersProperty;

		void OnEnable() 
		{
			globalStyleProperty = serializedObject.FindProperty("GlobalStyle");
			localStyleProperty = serializedObject.FindProperty("LocalStyle");
			targetTogglesProperty = serializedObject.FindProperty("targetToggles");
			targetTransfomsProperty = serializedObject.FindProperty("targetTransforms");
			targetGraphicsProperty = serializedObject.FindProperty("targetGraphics");
			targetMeshRenderersProperty = serializedObject.FindProperty("targetMeshRenderers");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			XButtonSharedEditor.DrawAutoStyleWarnings();

			var hasGlobal = globalStyleProperty.objectReferenceValue != null;
			if (!hasGlobal) EditorGUILayout.HelpBox("Local style will be used if no global style is specified", MessageType.Info);
			EditorGUILayout.PropertyField(globalStyleProperty);
			if (!hasGlobal) EditorGUILayout.PropertyField(localStyleProperty);

			EditorGUILayout.HelpBox("Avoid specifying transforms with raycastable components", MessageType.Info);
			EditorGUILayout.PropertyField(targetTogglesProperty, true);
			EditorGUILayout.PropertyField(targetTransfomsProperty, true);
			EditorGUILayout.PropertyField(targetGraphicsProperty, true);
			EditorGUILayout.PropertyField(targetMeshRenderersProperty, true);

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}