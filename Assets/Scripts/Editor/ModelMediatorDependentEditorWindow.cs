using System;

using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	public abstract class ModelMediatorDependentEditorWindow : EditorWindow
	{
		protected readonly string KeyPrefix;

		protected Action Enable = ActionExtensions.Empty;
		protected Action Disable = ActionExtensions.Empty;
		protected Action Gui = ActionExtensions.Empty;
		protected Action<float> EditorUpdate = ActionExtensions.GetEmpty<float>();
		protected Action InspectorUpdate = ActionExtensions.Empty;
		protected Action<Action<RequestStatus>> Save = ActionExtensions.GetEmpty<Action<RequestStatus>>();

		DateTime? lastEditorUpdate;

		EditorModelMediator editorSaveLoadService;
		protected IModelMediator SaveLoadService
		{
			get
			{
				if (editorSaveLoadService == null)
				{
					editorSaveLoadService = new EditorModelMediator(true);
					editorSaveLoadService.Initialize(BuildPreferences.Instance.Info, OnSaveLoadInitialized);
				}
				return editorSaveLoadService;
			}
		}

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

		void OnSaveLoadInitialized(RequestStatus status)
		{
			if (status == RequestStatus.Success) return;
			Debug.LogError("Editor time save load service returned: " + status);
		}
		#endregion
	}
}