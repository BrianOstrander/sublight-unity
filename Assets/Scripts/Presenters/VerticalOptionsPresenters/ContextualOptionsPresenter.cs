using System;

namespace LunraGames.SubLight.Presenters
{
	public abstract class ContextualOptionsPresenter : VerticalOptionsPresenter
	{
		Action<bool> setFocus;
		Action back;

		public ContextualOptionsPresenter()
		{
			App.Callbacks.Escape += OnEscape;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.Escape -= OnEscape;
		}

		protected virtual bool NotInteractable
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

			OnShow();

			ShowView(instant: instant);
		}

		protected abstract void OnShow();

		protected void ReShow()
		{
			Show(
				setFocus,
				back,
				false,
				false
			);
		}

		protected void ReShowInstant()
		{
			Show(
				setFocus,
				back,
				true,
				false
			);
		}

		#region Events
		void OnEscape()
		{
			if (NotInteractable) return;

			OnClickBack();
		}

		protected virtual void OnClickBack()
		{
			if (NotInteractable) return;

			View.Closed += OnBack;

			CloseView();
		}

		protected void OnBack()
		{
			back();
		}
		#endregion

	}
}