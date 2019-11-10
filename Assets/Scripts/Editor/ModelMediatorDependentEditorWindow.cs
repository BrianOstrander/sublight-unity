using System;

using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	public abstract class ModelMediatorDependentEditorWindow : EditorWindow
	{
		public readonly string KeyPrefix;

		protected Action Enable = ActionExtensions.Empty;
		protected Action Disable = ActionExtensions.Empty;
		protected Action Gui = ActionExtensions.Empty;
		protected Action<float> EditorUpdate = ActionExtensions.GetEmpty<float>();
		protected Action InspectorUpdate = ActionExtensions.Empty;
		protected Action<Action<RequestStatus>> Save = ActionExtensions.GetEmpty<Action<RequestStatus>>();

		DateTime? lastEditorUpdate;

		public ModelMediatorDependentEditorWindow(string keyPrefix)
		{
			KeyPrefix = keyPrefix;
		}

		#region Events
		void OnEnable()
		{
			Enable();
		}

		void OnDisable()
		{
			Disable();
		}

		void OnGUI()
		{
			Gui();
		}

		void Update()
		{
			var now = DateTime.Now;
			lastEditorUpdate = lastEditorUpdate ?? now;
			EditorUpdate((float)(now - lastEditorUpdate.Value).TotalSeconds);
			lastEditorUpdate = now;
		}

		void OnInspectorUpdate()
		{
			InspectorUpdate();
		}
		#endregion
	}
}