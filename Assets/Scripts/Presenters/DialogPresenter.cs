using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DialogPresenter : Presenter<IDialogView>
	{
		DialogRequest lastRequest;

		public DialogPresenter()
		{
			App.Callbacks.DialogRequest += OnDialogRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.DialogRequest -= OnDialogRequest;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();

			View.DialogType = lastRequest.DialogType;
			View.Title = lastRequest.Title;
			View.Message = lastRequest.Message;
			View.CancelText = lastRequest.CancelText;
			View.FailureText = lastRequest.FailureText;
			View.SuccessText = lastRequest.SuccessText;

			View.CancelClick = OnCancelClick;
			View.FailureClick = OnFailureClick;
			View.SuccessClick = OnSuccessClick;

			ShowView(App.CanvasRoot, true);
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
			View.Closed += () => OnClose(RequestStatus.Cancel);
			CloseView();
		}

		void OnFailureClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.Closed += () => OnClose(RequestStatus.Failure);
			CloseView();
		}

		void OnSuccessClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.Closed += () => OnClose(RequestStatus.Success);
			CloseView();
		}

		void OnClose(RequestStatus status)
		{
			switch(status)
			{
				case RequestStatus.Cancel: 
					lastRequest.Cancel(); 
					break;
				case RequestStatus.Failure:
					lastRequest.Failure();
					break;
				case RequestStatus.Success:
					lastRequest.Success();
					break;
			}
			lastRequest.Done(status);
			App.Callbacks.DialogRequest(lastRequest = lastRequest.Duplicate(DialogRequest.States.Complete));
		}
		#endregion

	}
}