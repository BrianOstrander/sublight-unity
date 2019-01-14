using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	[CustomEditor(typeof(View), true)]
	public class ViewEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (DefaultViews.Instance == null) OnNoDefaultViews();
			else OnDefaultViews();
			GUILayout.Space(16f);
			base.OnInspectorGUI();
		}

		void OnNoDefaultViews()
		{
			EditorGUILayout.HelpBox("The scriptable object for default views cannot be found", MessageType.Error);
		}

		void OnDefaultViews()
		{
			var prefabObject = PrefabUtility.GetCorrespondingObjectFromSource(target);

			if (prefabObject == null && PrefabUtility.GetPrefabInstanceHandle(target) != null) prefabObject = target;

			if (prefabObject == null)
			{
				EditorGUILayout.HelpBox("This view must be a prefab before it can be added to the list of default views", MessageType.Warning);
				return;
			}
			var isListed = DefaultViews.Instance.Contains(prefabObject);

			if (isListed) GUILayout.Label("This view is already a default view");
			else EditorGUILayout.HelpBox("Adding this view to the list of default views makes it accessable at runtime", MessageType.Warning);

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = !isListed;
				if (GUILayout.Button("Add")) DefaultViews.Instance.Add(prefabObject);
				GUI.enabled = isListed;
				if (GUILayout.Button("Remove")) DefaultViews.Instance.Remove(prefabObject);
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("See all default views")) Selection.activeObject = DefaultViews.Instance;
		}
	}
}