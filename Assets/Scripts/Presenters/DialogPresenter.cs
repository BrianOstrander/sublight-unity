using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class DialogPresenter : Presenter<IDialogView>
	{
		LanguageStringModel alertTitle;
		LanguageStringModel confirmTitle;

		LanguageStringModel okayDefault;
		LanguageStringModel yesDefault;
		LanguageStringModel noDefault;
		LanguageStringModel cancelDefault;

		DialogRequest lastRequest;

		public DialogPresenter(
			LanguageStringModel alertTitle,
			LanguageStringModel confirmTitle,

			LanguageStringModel okayDefault,
			LanguageStringModel yesDefault,
			LanguageStringModel noDefault,
			LanguageStringModel cancelDefault
		)
		{
			this.alertTitle = alertTitle;
			this.confirmTitle = confirmTitle;

			this.okayDefault = okayDefault;
			this.yesDefault = yesDefault;
			this.noDefault = noDefault;
			this.cancelDefault = cancelDefault;

			App.Callbacks.DialogRequest += OnDialogRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;
		}

		void Show()
		{
			if (View.Visible) return;
			if (App.Callbacks.LastPlayState.State != PlayState.States.Paused) App.Callbacks.PlayState(PlayState.Paused);

			View.Reset();

			View.DialogType = lastRequest.DialogType;
			View.Style = lastRequest.Style;

			LanguageStringModel defaultTitle = alertTitle;
			LanguageStringModel defaultSuccess = yesDefault;
			LanguageStringModel defaultFailure = noDefault;
			LanguageStringModel defaultCancel = cancelDefault;

			switch (lastRequest.DialogType)
			{
				case DialogTypes.Confirm:
					defaultTitle = alertTitle;
					defaultSuccess = okayDefault;
					break;
				case DialogTypes.ConfirmDeny:
					defaultTitle = confirmTitle;
					defaultSuccess = yesDefault;
					defaultFailure = noDefault;
					break;
				case DialogTypes.ConfirmDenyCancel:
					defaultTitle = confirmTitle;
					defaultSuccess = yesDefault;
					defaultFailure = noDefault;
					defaultCancel = cancelDefault;
					break;
				default:
					Debug.LogError("Unrecognized style: " + lastRequest.Style);
					break;
			}

			View.Title = lastRequest.Title ?? defaultTitle;
			View.Message = lastRequest.Message ?? string.Empty;
			View.SuccessText = lastRequest.SuccessText ?? defaultSuccess;
			View.FailureText = lastRequest.FailureText ?? defaultFailure;
			View.CancelText = lastRequest.CancelText ?? defaultCancel;

			View.CancelClick = OnCancelClick;
			View.FailureClick = OnFailureClick;
			View.SuccessClick = OnSuccessClick;

			ShowView();
		}

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
			switch (request.State)
			{
				case DialogRequest.States.Request:
					if (lastRequest.State == DialogRequest.States.Active)
					{
						Debug.LogWarning("Unable to request a new dialog to open while waiting for another one.");
						return;
					}
					App.Callbacks.DialogRequest(lastRequest = request.Duplicate(DialogRequest.States.Active));
					break;
				case DialogRequest.States.Active:
					Show();
					break;
			}
		}

		void OnCancelClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose(RequestStatus.Cancel);
		}

		void OnFailureClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose(RequestStatus.Failure);
		}

		void OnSuccessClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose(RequestStatus.Success);
		}

		void OnClose(RequestStatus status)
		{
			App.Callbacks.DialogRequest(lastRequest = lastRequest.Duplicate(DialogRequest.States.Completing));

			View.Closed += () => OnClosed(status);
			CloseView();
		}

		void OnClosed(RequestStatus status)
		{
			var finalRequest = lastRequest.Duplicate(DialogRequest.States.Complete);
			lastRequest = default(DialogRequest);
			App.Callbacks.DialogRequest(finalRequest);

			switch(status)
			{
				case RequestStatus.Cancel: 
					finalRequest.Cancel(); 
					break;
				case RequestStatus.Failure:
					finalRequest.Failure();
					break;
				case RequestStatus.Success:
					finalRequest.Success();
					break;
			}
			finalRequest.Done(status);
		}
		#endregion

	}
}