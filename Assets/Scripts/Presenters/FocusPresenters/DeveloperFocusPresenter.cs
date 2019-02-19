using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public abstract class DeveloperFocusPresenter<V, D> : FocusPresenter<V, D>
		where V : class, IView
		where D : SetFocusDetails<D>, new()
	{
		protected abstract DeveloperViews DeveloperView { get; }

		protected GameModel Model { get; private set; }

		DeveloperViews[] previouslyVisible;

		protected override bool CanShow()
		{
			return previouslyVisible.Any(e => e == DeveloperView);
		}

		public DeveloperFocusPresenter(GameModel model)
		{
			Model = model;

			previouslyVisible = model.Context.DeveloperViewsEnabled.Values;

			Model.Context.DeveloperViewsEnabled.Entries.Changed += OnDeveloperViewsEnabled;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Context.DeveloperViewsEnabled.Entries.Changed -= OnDeveloperViewsEnabled;
		}

		#region Events
		void OnDeveloperViewsEnabled(StackListModel<DeveloperViews>.Entry[] developerViews)
		{
			var wasEnabled = previouslyVisible.Any(e => e == DeveloperView);
			previouslyVisible = developerViews.Select(e => e.Value).ToArray();
			var isEnabled = previouslyVisible.Any(e => e == DeveloperView);

			if (wasEnabled == isEnabled || !FocusLayerEnabled) return;

			ForceShowClose(isEnabled);
		}
		#endregion
	}
}