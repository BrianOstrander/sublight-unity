using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using LunraGames;

namespace LunraGames.SubLight
{
	[CustomEditor(typeof(XButton), true)]
	[CanEditMultipleObjects]
	public class XButtonEditor : Editor
	{
		SerializedProperty interactableProperty;
		SerializedProperty globalSoundsProperty;
		SerializedProperty localSoundsProperty;
		SerializedProperty fadeDurationProperty;
		SerializedProperty leafsProperty;
		SerializedProperty onEnterProperty;
		SerializedProperty onExitProperty;
		SerializedProperty onClickProperty;
		SerializedProperty onDownProperty;
		SerializedProperty onDragBeginProperty;
		SerializedProperty onDragEndProperty;

		void OnEnable() 
		{
			interactableProperty = serializedObject.FindProperty("m_Interactable");
			globalSoundsProperty = serializedObject.FindProperty("globalSounds");
			localSoundsProperty = serializedObject.FindProperty("localSounds");
			fadeDurationProperty = serializedObject.FindProperty("fadeDuration");
			leafsProperty = serializedObject.FindProperty("leafs");
			onEnterProperty = serializedObject.FindProperty("onEnter");
			onExitProperty = serializedObject.FindProperty("onExit");
			onClickProperty = serializedObject.FindProperty("onClick");
			onDownProperty = serializedObject.FindProperty("onDown");
			onDragBeginProperty = serializedObject.FindProperty("onDragBegin");
			onDragEndProperty = serializedObject.FindProperty("onDragEnd");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			XButtonSharedEditor.DrawAutoStyleWarnings();

			EditorGUILayout.PropertyField(interactableProperty, new GUIContent(interactableProperty.displayName));

			var hasGlobal = globalSoundsProperty.objectReferenceValue != null;
			if (!hasGlobal) EditorGUILayout.HelpBox("Local sounds will be used if no global style is specified", MessageType.Info);
			EditorGUILayout.PropertyField(globalSoundsProperty);
			if (!hasGlobal) EditorGUILayout.PropertyField(localSoundsProperty);

			EditorGUILayout.PropertyField(fadeDurationProperty, true);
			EditorGUILayout.PropertyField(leafsProperty, true);

			if (targets.Length == 1) GUI.enabled = true;
			else
			{
				EditorGUILayout.HelpBox("Select a single XButton to add child or sibling leafs", MessageType.Warning);
				GUI.enabled = false;
			}

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Add Child & Sibling Leafs")) AddChildAndSiblingLeafs();
				if (GUILayout.Button("Create Sibling Leaf")) CreateSiblingLeaf();
			}
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			EditorGUILayout.PropertyField(onEnterProperty);
			EditorGUILayout.PropertyField(onExitProperty);
			EditorGUILayout.PropertyField(onClickProperty);
			EditorGUILayout.PropertyField(onDownProperty);
			EditorGUILayout.PropertyField(onDragBeginProperty);
			EditorGUILayout.PropertyField(onDragEndProperty);

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}

		void AddChildAndSiblingLeafs()
		{
			var typedTarget = target as XButton;

			var children = typedTarget.transform.GetDescendants<XButtonLeaf>();
			children.AddRange(typedTarget.GetComponents<XButtonLeaf>());

			var existingLeafIds = new List<int>();
			for (var i = 0; i < leafsProperty.arraySize; i++)
			{
				var leaf = leafsProperty.GetArrayElementAtIndex(i);
				if (leaf != null) existingLeafIds.Add(leaf.objectReferenceInstanceIDValue);
			}

			foreach (var child in children.Where(c => !existingLeafIds.Contains(c.GetInstanceID())))
			{
				AddLeaf(child);
			}
		}

		void CreateSiblingLeaf()
		{
			var typedTarget = target as XButton;
			var leaf = typedTarget.gameObject.AddComponent<XButtonLeaf>();
			AddLeaf(leaf);
		}

		void AddLeaf(XButtonLeaf leaf)
		{
			var index = leafsProperty.arraySize;
			leafsProperty.InsertArrayElementAtIndex(index);
			leafsProperty.GetArrayElementAtIndex(index).objectReferenceValue = leaf;
		}
	}
}