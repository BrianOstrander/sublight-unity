using System;

namespace LunraGames.SubLight.Presenters
{
	public abstract class ContextualOptionsPresenter : VerticalOptionsPresenter
	{
		Action<bool> setFocus;
		Action back;

		protected bool NotInteractable
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
			//CloseView(true);

			Show(
				setFocus,
				back,
				true,
				false
			);
		}

		#region Events
		protected void OnClickBack()
		{
			if (NotInteractable) return;

			View.Closed += back;

			CloseView();
		}
		#endregion

	}
}