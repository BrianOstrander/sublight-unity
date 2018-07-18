using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DialogPresenter : Presenter<IDialogView>
	{
		DialogRequest lastRequest;
		SpeedRequest lastSpeedChange;
		bool wasShaded;
		bool wasObscured;
		bool hasPoppedEscape;

		public DialogPresenter()
		{
			App.Callbacks.DialogRequest += OnDialogRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;
		}

		public void Show()
		{
			if (View.Visible) return;
			if (App.Callbacks.LastPlayState.State != PlayState.States.Paused) App.Callbacks.PlayState(PlayState.Paused);

			wasShaded = App.Callbacks.LastShadeRequest.IsShaded;
			wasObscured = App.Callbacks.LastObscureCameraRequest.IsObscured;
			hasPoppedEscape = false;

			lastSpeedChange = App.Callbacks.LastSpeedRequest;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);

			View.Reset();

			EscapeEntry escape;
			if (wasShaded != wasObscured) Debug.LogWarning("Values wasShaded and wasObscured not equal, may cause unexpected behaviour.");

			if (wasShaded && wasObscured) escape = new EscapeEntry(OnEscape);
			else
			{
				App.Callbacks.ShadeRequest(ShadeRequest.Shade);
				App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.Obscure);
				escape = new EscapeEntry(OnEscape, false, false);
			}

			View.Shown += () => App.Callbacks.PushEscape(escape);

			View.DialogType = lastRequest.DialogType;
			View.Title = lastRequest.Title;
			View.Message = lastRequest.Message;
			View.CancelText = lastRequest.CancelText;
			View.FailureText = lastRequest.FailureText;
			View.SuccessText = lastRequest.SuccessText;

			View.CancelClick = OnCancelClick;
			View.FailureClick = OnFailureClick;
			View.SuccessClick = OnSuccessClick;

			ShowView(App.OverlayCanvasRoot);
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

		void OnEscape()
		{
			hasPoppedEscape = true;
			OnClose(RequestStatus.Cancel);
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
			if (!hasPoppedEscape) App.Callbacks.PopEscape();
			if (!(wasShaded && wasObscured))
			{
				if (App.Callbacks.LastPlayState.State != PlayState.States.Playing) App.Callbacks.PlayState(PlayState.Playing);
				App.Callbacks.ShadeRequest(ShadeRequest.UnShade);
				App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.UnObscure);
			}
			View.Closed += () => OnClosed(status);
			CloseView();
		}

		void OnClosed(RequestStatus status)
		{
			App.Callbacks.SpeedRequest(lastSpeedChange.Duplicate(SpeedRequest.States.Request));
			App.Callbacks.DialogRequest(lastRequest = lastRequest.Duplicate(DialogRequest.States.Complete));
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
		}
		#endregion

	}
}