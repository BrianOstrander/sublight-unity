using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class LanguageDatabaseService
	{
		struct TaggedLanguage
		{
			public string LanguageId;
			public string[] Tags;
			public SaveModel Save;
			public LanguageDatabaseModel Loaded;
			public int Order;
			public bool Enabled;

			public bool IsLoaded { get { return Loaded != null; } }

			public TaggedLanguage(
				string languageId,
				string[] tags,
				SaveModel save
			)
			{
				LanguageId = languageId;
				Tags = tags;
				Save = save;
				Loaded = null;
				Order = -1;
				Enabled = false;
			}
		}

		IModelMediator modelMediator;
		ILogService logger;
		CallbackService callbacks;
		Func<PreferencesModel> currentPreferences;

		List<TaggedLanguage> languages = new List<TaggedLanguage>();
		LanguageListModel currentLanguage;

		public LanguageDatabaseService(
			IModelMediator modelMediator,
			ILogService logger,
			CallbackService callbacks,
			Func<PreferencesModel> currentPreferences
		)
		{
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (logger == null) throw new ArgumentNullException("logger");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (currentPreferences == null) throw new ArgumentNullException("currentPreferences");

			this.modelMediator = modelMediator;
			this.logger = logger;
			this.callbacks = callbacks;
			this.currentPreferences = currentPreferences;
		}

		#region Initialization
		public void Initialize(Action<RequestStatus> done)
		{
			modelMediator.List<LanguageDatabaseModel>(result => OnListLanguages(result, done));
		}

		void OnListLanguages(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing languages failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			foreach (var save in result.Models)
			{
				var languageId = save.GetMetaKey(MetaKeyConstants.LanguageDatabase.LanguageId);
				if (string.IsNullOrEmpty(languageId))
				{
					Debug.LogError("Language at path " + save.Path.Value + " has null or empty LanguageId, skipping...");
					continue;
				}
				var tags = (save.GetMetaKey(MetaKeyConstants.LanguageDatabase.Tags) ?? string.Empty).Split(',');
				if (tags.None())
				{
					Debug.LogError("Language at path " + save.Path.Value + " has to tags, skipping...");
					continue;
				}
				tags = tags.Select(t => t.ToLower()).ToArray();
				languages.Add(new TaggedLanguage(languageId, tags, save));
			}

			UpdateTags(currentPreferences().LanguageTagOrder.Value.ToList(), () => OnUpdateTagsDone(done));
		}

		void OnUpdateTagsDone(Action<RequestStatus> done)
		{
			done(RequestStatus.Success);
		}
		#endregion

		void UpdateTags(List<KeyValuePair<string, int>> tags, Action done)
		{
			currentLanguage = new LanguageListModel();
			OnUpdateTagsToggle(tags, done);
		}

		void OnUpdateTagsToggle(List<KeyValuePair<string, int>> tags, Action done)
		{
			if (tags.None())
			{
				OnUpdateTagsToggleDone(done);
				return;
			}

			var current = tags.First();
			var currentKey = current.Key.ToLower();
			var currentValue = current.Value;

			tags.RemoveAt(0);

			for (var i = 0; i < languages.Count; i++)
			{
				var language = languages[i];
				language.Enabled = language.Tags.Contains(currentKey);
				if (!language.Enabled) continue;

				language.Order = Mathf.Max(language.Order, currentValue);
			}

			OnUpdateTagsToggle(tags, done);
		}

		void OnUpdateTagsToggleDone(Action done)
		{
			var enabledLanguages = languages.Where(l => l.Enabled);
			OnUpdateTagsLoad(
				enabledLanguages.Where(l => l.IsLoaded).ToList(),
				enabledLanguages.Where(l => !l.IsLoaded).ToList(),
				done
			);
		}

		void OnUpdateTagsLoad(List<TaggedLanguage> loaded, List<TaggedLanguage> remaining, Action done)
		{
			if (remaining.None())
			{
				OnUpdateTagsBuild(loaded, done);
				return;
			}

			var current = remaining.First();
			remaining.RemoveAt(0);

			modelMediator.Load<LanguageDatabaseModel>(current.Save, result =>
			{
				if (result.Status != RequestStatus.Success)
				{
					Debug.LogError("Loading language failed with status " + result.Status + " and error: " + result.Error);
					OnUpdateTagsLoad(loaded, remaining, done);
					return;
				}
				if (result.TypedModel.Ignore.Value)
				{
					OnUpdateTagsLoad(loaded, remaining, done);
					return;
				}
				current.Loaded = result.TypedModel;
				loaded.Add(current);
				OnUpdateTagsLoad(loaded, remaining, done);
			});
		}

		void OnUpdateTagsBuild(List<TaggedLanguage> loaded, Action done)
		{
			if (loaded.None())
			{
				OnUpdateTagsBuildDone(done);
				return;
			}

			var current = loaded.First();
			loaded.RemoveAt(0);

			currentLanguage.Apply(current.Loaded.Language, current.Order, () => OnUpdateTagsBuild(loaded, done));
		}

		void OnUpdateTagsBuildDone(Action done)
		{
			Debug.Log("probably going to update all existing models in wild here");
			if (done != null) done();
		}

		#region Utility
		public void DebugCurrentLanguage()
		{
			var all = "All entries:";
			foreach (var entry in currentLanguage.Entries)
			{
				var value = entry.Value.Value;
				var shortened = (string.IsNullOrEmpty(value) || value.Length < 32) ? value : value.Substring(0, 32);
				all += "\n" + entry.Value.Order + " - " + entry.Key + " : " + shortened;
			}
			Debug.Log(all);
		}
		#endregion
	}
}