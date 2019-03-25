using System;
using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class ChangelogPresenter : ContextualOptionsPresenter
	{
		public static ChangelogPresenter CreateDefault()
		{
			return new ChangelogPresenter(
				new ChangelogLanguageBlock
				{
					Title = LanguageStringModel.Override("Changelog"),
					Back = LanguageStringModel.Override("Back"),

					ChangeFeature = LanguageStringModel.Override("FEATURE"),
					ChangeImprovement = LanguageStringModel.Override("IMPROVEMENT"),
					ChangeFix = LanguageStringModel.Override("FIX"),
					ChangeDeprecation = LanguageStringModel.Override("DEPRECATION"),

					GetDate = (year, month, day) => year+"-"+month+"-"+day
				}
			);
		}

		ChangelogLanguageBlock language;

		public ChangelogPresenter(
			ChangelogLanguageBlock language
		)
		{
			this.language = language;
		}

		protected override void OnShow()
		{
			var changelog = App.BuildPreferences.Current;

			var changes = string.Empty;
			var isFirst = true;
			foreach (var change in changelog.Entries.OrderBy(e => e.Index))
			{
				if (!isFirst) changes += "<br>";
				changes += GetDescription(change);

				isFirst = false;
			}

			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Changelog),
				LabelVerticalOptionsEntry.CreateHeader(changelog.Title),
				LabelVerticalOptionsEntry.CreateBody(GetDescription(changelog)),
				LabelVerticalOptionsEntry.CreateBody(changes),
				ButtonVerticalOptionsEntry.CreateButton(language.Back.Value, OnClickBack)
			);

			App.Analytics.ScreenVisit(AnalyticsService.ScreenNames.Changelog);
		}

		string GetDescription(
			Changelog changelog
		)
		{
			var result = string.Empty;

			result += "<b>" + changelog.Cyle + " " + changelog.ReleaseType + " Release " + language.GetDate(changelog.Year, changelog.Month, changelog.Day)+"</b>";
			result += "<br>";
			result += changelog.Description;

			return result;
		}

		string GetDescription(
			Changelog.Entry entry
		)
		{
			var result = string.Empty;

			var change = "CHANGE";

			switch (entry.Change)
			{
				case Changelog.Changes.Feature: change = language.ChangeFeature.Value; break;
				case Changelog.Changes.Improvement: change = language.ChangeImprovement.Value; break;
				case Changelog.Changes.Fix: change = language.ChangeFix.Value; break;
				case Changelog.Changes.Deprecation: change = language.ChangeDeprecation.Value; break;
				default:
					Debug.LogError("Unrecognized Change: " + entry.Change);
					break;
			}

			result += "<b>[" + change + "]</b> " + entry.Description;

			return result;
		}

		#region Events

		#endregion
	}
}