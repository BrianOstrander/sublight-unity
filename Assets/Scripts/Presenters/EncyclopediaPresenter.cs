using System.Collections.Generic;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class EncyclopediaPresenter : Presenter<IEncyclopediaView>
	{
		GameModel model;

		public EncyclopediaPresenter(GameModel model)
		{
			this.model = model;

			//App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void OnUnBind()
		{
			//App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = "Select an Article";
			View.BackClick = OnBackClick;

			var buttons = new List<LabelButtonBlock>();
			foreach (var title in model.Encyclopedia.Titles.Value) buttons.Add(new LabelButtonBlock(LanguageStringModel.Override(title), () => OnArticleClick(title)));

			View.ArticleEntries = buttons.ToArray();

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		/* TODO: Support the new focus system.
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Encyclopedia:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var encyclopediaFocus = focus as EncyclopediaFocusRequest;
					// We also only show up if our view is specified
					if (encyclopediaFocus.View != EncyclopediaFocusRequest.Views.Home) goto default;
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}
		*/

		void OnArticleClick(string title)
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			var article = model.Encyclopedia.GetArticle(title);

			View.Title = article.Title;
			var sections = new List<ArticleSectionBlock>();
			foreach (var section in article.Entries.Value) sections.Add(new ArticleSectionBlock(section.Header, section.Body));
			View.SectionEntries = sections.ToArray();
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			//App.Callbacks.FocusRequest(
			//	new SystemsFocusRequest(
			//		model.Ship.Value.Position.Value.SystemZero,
			//		model.Ship.Value.Position.Value
			//	)
			//);
		}
		#endregion
	}
}