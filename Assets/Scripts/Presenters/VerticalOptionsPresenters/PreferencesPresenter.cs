using System;

using UnityEngine.Analytics;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PreferencesPresenter : VerticalOptionsPresenter
	{
		public static PreferencesPresenter CreateDefault()
		{
			return new PreferencesPresenter(
				new PreferencesLanguageBlock
				{
					Title = LanguageStringModel.Override("Preferences"),
					Back = LanguageStringModel.Override("Back"),

					AnalyticsEnabled = LanguageStringModel.Override("Disable Analytics"),
					AnalyticsDisabled = LanguageStringModel.Override("Enable Analytics")
				}
			);
		}

		PreferencesLanguageBlock language;

		Action<bool> setFocus;
		Action back;

		public PreferencesPresenter(
			PreferencesLanguageBlock language
		)
		{
			this.language = language;
		}

		protected override void OnUnBind()
		{

		}

		bool NotInteractable
		{
			get
			{
				return View.TransitionState != TransitionStates.Shown;
			}
		}

		public void Show(
			Action<bool> setFocus,
			Action back,
			bool instant = false,
			bool reFocus = true
		)
		{
			if (setFocus == null) throw new ArgumentNullException("setFocus");
			if (back == null) throw new ArgumentNullException("back");

			this.setFocus = setFocus;
			this.back = back;

			if (reFocus) setFocus(instant);

			View.Reset();

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
				analyticsEntry,
				ButtonVerticalOptionsEntry.CreateButton(language.Back.Value, OnClickBack)
			);

			ShowView(instant: instant);
		}

		#region Events
		void OnClickToggleAnalytics(bool enable)
		{
			if (NotInteractable) return;

			Analytics.enabled = enable;

			CloseView(true);

			Show(
				setFocus,
				back,
				true,
				false
			);
		}

		void OnClickBack()
		{
			if (NotInteractable) return;

			View.Closed += back;

			CloseView();
		}
		#endregion

	}
}