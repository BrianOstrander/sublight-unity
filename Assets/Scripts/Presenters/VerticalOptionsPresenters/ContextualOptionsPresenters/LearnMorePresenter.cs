using System;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class LearnMorePresenter : ContextualOptionsPresenter
	{
		public static LearnMorePresenter CreateDefault()
		{
			return new LearnMorePresenter(
				new LearnMoreLanguageBlock
				{
					Title = LanguageStringModel.Override("Learn More"),
					Description = LanguageStringModel.Override("Follow the progress of SubLight, and learn what's next!"),
					Back = LanguageStringModel.Override("Back"),

					OpeningUrl = LanguageStringModel.Override("Transmitting..."),

					Blog = LanguageStringModel.Override("Development Blog"),
					Discord = LanguageStringModel.Override("Discord"),
					Twitter = LanguageStringModel.Override("Twitter")
				}
			);
		}

		LearnMoreLanguageBlock language;

		public LearnMorePresenter(
			LearnMoreLanguageBlock language
		)
		{
			this.language = language;
		}

		protected override void OnShow()
		{
			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.LearnMore),
				LabelVerticalOptionsEntry.CreateBody(language.Description),
				ButtonVerticalOptionsEntry.CreateButton(language.Blog.Value, CloseThenClick(() => OnClickLink(BuildPreferences.Instance.BlogUrl))),
				ButtonVerticalOptionsEntry.CreateButton(language.Discord.Value, CloseThenClick(() => OnClickLink(BuildPreferences.Instance.DiscordUrl))),
				ButtonVerticalOptionsEntry.CreateButton(language.Twitter.Value, CloseThenClick(() => OnClickLink(BuildPreferences.Instance.TwitterUrl))),
				ButtonVerticalOptionsEntry.CreateButton(language.Back.Value, OnClickBack)
			);

			App.Analytics.ScreenVisit(AnalyticsService.ScreenNames.LearnMore);
		}

		Action CloseThenClick(Action done)
		{
			return () =>
			{
				if (View.TransitionState != TransitionStates.Shown) return;

				SM.PushBlocking(
					blockingDone =>
					{
						View.Closed += blockingDone;
						CloseView();
					},
					"ClosingLearnMoreMenuForClick"
				);

				SM.Push(
					done,
					"LearnMoreClick"
				);
			};
		}

		#region Events
		void OnClickLink(string url)
		{
			// Closing this view should already be on the stack.

			SM.PushBlocking(
				done =>
				{
					View.Reset();
					View.Shown += done;
					View.SetEntries(
						VerticalOptionsThemes.Warning,
						LabelVerticalOptionsEntry.CreateTitle(language.OpeningUrl.Value, VerticalOptionsIcons.OpeningUrl)
					);
					ShowView();
				},
				"ShowingOpenLink"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.5f),
				"WaitingForOpenLink"
			);

			SM.Push(
				() => Application.OpenURL(url),
				"OpeningLink"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.25f),
				"WaitingToReshowLearnMore"
			);

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingUrlContextToShowLearnMore"
			);

			SM.Push(
				ReShow,
				"ShowLearnMore"
			);
		}
		#endregion

	}
}