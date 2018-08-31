﻿using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public abstract class LanguageDependentEditorWindow : ModelMediatorDependentEditorWindow
	{
		EditorPrefsString languageDependentSelectedPath;

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus saveLanguageDependentListStatus;
		SaveModel[] saveLanguageDependentList = new SaveModel[0];

		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Deselected or failed to load
		RequestStatus selectedLanguageDependentStatus;
		LanguageDatabaseModel selectedLanguageDependent;
		bool selectedLanguageDependentModified;
		bool selectedLanguageDependentLastWarning;

		public LanguageDependentEditorWindow(string keyPrefix) : base(keyPrefix)
		{
			languageDependentSelectedPath = new EditorPrefsString(KeyPrefix + "LanguageDependentSelectedPath");
			Enable += OnLanguageDependentEnable;
			Gui += OnLanguageDependentGui;
		}

		#region Events
		void OnLanguageDependentEnable()
		{
			saveLanguageDependentListStatus = RequestStatus.Cancel;
			selectedLanguageDependentStatus = RequestStatus.Cancel;
		}

		void OnLanguageDependentGui()
		{
			switch (saveLanguageDependentListStatus)
			{
				case RequestStatus.Cancel:
					LoadLanguageDependentSaveList();
					return;
				case RequestStatus.Unknown:
					return;
			}
			if (string.IsNullOrEmpty(languageDependentSelectedPath.Value)) return;

			switch (selectedLanguageDependentStatus)
			{
				case RequestStatus.Cancel:
					LoadLanguageDependentSelected(saveLanguageDependentList.FirstOrDefault(m => m.Path == languageDependentSelectedPath.Value));
					return;
			}
		}

		void LoadLanguageDependentSaveList()
		{
			saveLanguageDependentListStatus = RequestStatus.Unknown;
			SaveLoadService.List<LanguageDatabaseModel>(OnLoadSaveList);
		}

		void LoadLanguageDependentSelected(SaveModel model)
		{
			if (model == null)
			{
				languageDependentSelectedPath.Value = null;
				selectedLanguageDependentStatus = RequestStatus.Failure;
				return;
			}
			selectedLanguageDependentStatus = RequestStatus.Unknown;
			languageDependentSelectedPath.Value = model.Path;
			SaveLoadService.Load<LanguageDatabaseModel>(model, OnLoadLanguageDependentSelected);
		}

		void OnLoadSaveList(SaveLoadArrayRequest<SaveModel> result)
		{
			saveLanguageDependentListStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			saveLanguageDependentList = result.Models;

			if (selectedLanguageDependent == null) return;
			if (saveLanguageDependentList.FirstOrDefault(e => e.Path.Value == selectedLanguageDependent.Path.Value) != null) return;
			OnDeselect();
		}

		void OnLoadLanguageDependentSelected(SaveLoadRequest<LanguageDatabaseModel> result)
		{
			selectedLanguageDependentStatus = result.Status;
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				return;
			}
			var model = result.TypedModel;
			selectedLanguageDependentModified = false;
			languageDependentSelectedPath.Value = model.Path;
			selectedLanguageDependent = model;
		}

		void OnDeselect()
		{
			selectedLanguageDependentStatus = RequestStatus.Failure;
			selectedLanguageDependent = null;
			selectedLanguageDependentModified = false;
		}

		protected void LanguageSelector()
		{
			switch (saveLanguageDependentListStatus)
			{
				case RequestStatus.Success: break;
				default:
					GUILayout.Label("Loading Languages...");
					return;
			}

			var labels = new List<string>(new string[] { "- Select Language -"});
			var values = new List<SaveModel>(new SaveModel[] { null });

			foreach (var language in saveLanguageDependentList)
			{
				labels.Add(language.Meta.Value);
				values.Add(language);
			}

			var index = 0;
			if (selectedLanguageDependentStatus == RequestStatus.Success)
			{
				for (var i = 0; i < values.Count; i++)
				{
					if (values[i] == null) continue;
					if (selectedLanguageDependent.LanguageId.Value == values[i].GetMetaKey(MetaKeyConstants.LanguageDatabase.LanguageId))
					{
						index = i;
						break;
					}
				}
			}
			var selection = values[EditorGUILayout.Popup(index, labels.ToArray())];

			if (selection == null && selectedLanguageDependentStatus == RequestStatus.Success)
			{
				languageDependentSelectedPath.Value = null;
				OnDeselect();
			}
			else if (selection != null && (selectedLanguageDependentStatus != RequestStatus.Success || selection.GetMetaKey(MetaKeyConstants.LanguageDatabase.LanguageId) != selectedLanguageDependent.LanguageId.Value))
			{
				LoadLanguageDependentSelected(selection);
			}

			var currentWarning = selectedLanguageDependentLastWarning;
			if (Event.current.type == EventType.Layout) selectedLanguageDependentLastWarning = currentWarning = (selection == null);

			if (currentWarning) EditorGUILayout.HelpBox("A language must be selected.", MessageType.Error);
		}
		#endregion
	}
}