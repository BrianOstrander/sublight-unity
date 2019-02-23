using UnityEngine.Analytics;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PreferencesPresenter : ContextualOptionsPresenter
	{
		public static PreferencesPresenter CreateDefault()
		{
			return new PreferencesPresenter(
				new PreferencesLanguageBlock
				{
					Title = LanguageStringModel.Override("Preferences"),
					Back = LanguageStringModel.Override("Back"),

					AnalyticsEnabled = LanguageStringModel.Override("Disable Analytics"),
					AnalyticsDisabled = LanguageStringModel.Override("Enable Analytics"),

					VersionPrefix = LanguageStringModel.Override("SubLight Version ")
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
			ButtonVerticalOptionsEntry analyticsEntry = null;

			if (Analytics.enabled)
			{
				analyticsEntry = ButtonVerticalOptionsEntry.CreateButton(
					language.AnalyticsEnabled.Value,
					() => OnClickToggleAnalytics(false)
				);
			}
			else
			{
				analyticsEntry = ButtonVerticalOptionsEntry.CreateButton(
					language.AnalyticsDisabled.Value,
					() => OnClickToggleAnalytics(true)
				);
			}

			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Preferences),
				LabelVerticalOptionsEntry.CreateBody(language.VersionPrefix.Value + BuildPreferences.Instance.Info.Version),
				analyticsEntry,
				ButtonVerticalOptionsEntry.CreateButton(language.Back.Value, OnClickBack)
			);
		}

		#region Events
		void OnClickToggleAnalytics(bool enable)
		{
			if (NotInteractable) return;

			Analytics.enabled = enable;

			ReShowInstant();
		}
		#endregion

	}
}