using System;

using UnityEngine;
using UnityEngine.Analytics;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PreferencesPresenter : ContextualOptionsPresenter
	{
		public static PreferencesPresenter CreateDefault()
		{
			var defaultToggle = new ToggleOptionLanguageBlock
			{
				MessageEnabled = LanguageStringModel.Override("ENABLED"),
				MessageDisabled = LanguageStringModel.Override("DISABLED")
			};

			return new PreferencesPresenter(
				new PreferencesLanguageBlock
				{
					Title = LanguageStringModel.Override("Preferences"),
					Back = LanguageStringModel.Override("Back"),

					Analytics = defaultToggle.Duplicate(LanguageStringModel.Override("Analytics")),
					Tutorial = defaultToggle.Duplicate(LanguageStringModel.Override("Tutorial")),

					VersionPrefix = LanguageStringModel.Override("SubLight Version "),
					Gameplay = LanguageStringModel.Override("Gameplay")
				}
			);
		}

		PreferencesLanguageBlock language;

		public PreferencesPresenter(
			PreferencesLanguageBlock language
		)
		{
			this.language = language;
		}

		protected override void OnUnBind()
		{

		}

		protected override void OnShow()
		{
			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Preferences),
				LabelVerticalOptionsEntry.CreateBody(language.VersionPrefix.Value + BuildPreferences.Instance.Info.Version),
				GetToggle(
					language.Analytics,
					() => Analytics.enabled,
					value => Analytics.enabled = value
				),
				LabelVerticalOptionsEntry.CreateHeader(language.Gameplay),
				GetToggle(
					language.Tutorial,
					() => !App.MetaKeyValues.Get(KeyDefines.Preferences.IgnoreTutorial),
					value => App.MetaKeyValues.Set(KeyDefines.Preferences.IgnoreTutorial, !value)
				),
				ButtonVerticalOptionsEntry.CreateButton(language.Back.Value, OnClickBack)
			);
		}

		ButtonVerticalOptionsEntry GetToggle(
			ToggleOptionLanguageBlock toggleLanguage,
			Func<bool> getValue,
			Action<bool> setValue
		)
		{
			if (getValue == null) throw new ArgumentNullException("getValue");
			if (setValue == null) throw new ArgumentNullException("setValue");

			var result = ButtonVerticalOptionsEntry.CreateToggle(
				toggleLanguage.Message,
				toggleLanguage.GetValue(getValue()),
				OnClickToggleNotSet
			);

			result.Click = () =>
			{
				if (NotInteractable) return;
				var newValue = !getValue();
				result.SetMessages(toggleLanguage.Message, toggleLanguage.GetValue(newValue));
				setValue(newValue);
			};

			return result;
		}


		#region Events
		void OnClickToggleNotSet()
		{
			Debug.LogError("Toggle click was not set");
		}
		#endregion

	}
}