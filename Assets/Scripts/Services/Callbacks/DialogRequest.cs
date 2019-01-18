using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct DialogRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Completing = 30, // Called when the dialog is closing.
			Complete = 40 // Called when the dialog is finally closed.
		}

		/// <summary>
		/// Creates an Alert request.
		/// </summary>
		/// <returns>The alert.</returns>
		/// <param name="message">Message.</param>
		/// <param name="title">Title.</param>
		/// <param name="confirmClick">Done.</param>
		public static DialogRequest Confirm(
			LanguageStringModel message,
			DialogStyles style = DialogStyles.Neutral,
			LanguageStringModel title = null,
			Action confirmClick = null
		)
		{
			return new DialogRequest(
				States.Request,
				DialogTypes.Confirm,
				style,
				title,
				message,
				success: confirmClick
			);
		}

		/// <summary>
		/// Creates a CancelConfirm request.
		/// </summary>
		/// <returns>The confirm.</returns>
		/// <param name="message">Message.</param>
		/// <param name="title">Title.</param>
		/// <param name="denyClick">Cancel.</param>
		/// <param name="confirmClick">Confirm.</param>
		/// <param name="done">Done.</param>
		public static DialogRequest ConfirmDeny(
			LanguageStringModel message,
			DialogStyles style = DialogStyles.Neutral,
			LanguageStringModel title = null,
			Action confirmClick = null,
			Action denyClick = null,
			Action<RequestStatus> done = null
		)
		{
			return new DialogRequest(
				States.Request,
				DialogTypes.ConfirmDeny,
				style,
				title,
				message,
				cancel: denyClick,
				success: confirmClick,
				done: done
			);
		}

		/// <summary>
		/// Creates a CancelDenyConfirm request.
		/// </summary>
		/// <returns>The deny confirm.</returns>
		/// <param name="message">Message.</param>
		/// <param name="title">Title.</param>
		/// <param name="cancelClick">Cancel.</param>
		/// <param name="denyClick">Deny.</param>
		/// <param name="confirmClick">Confirm.</param>
		/// <param name="done">Done.</param>
		public static DialogRequest ConfirmDenyCancel(
			LanguageStringModel message,
			DialogStyles style = DialogStyles.Neutral,
			LanguageStringModel title = null,
			Action confirmClick = null,
			Action denyClick = null,
			Action cancelClick = null,
			LanguageStringModel cancelText = null,
			LanguageStringModel denyText = null,
			LanguageStringModel confirmText = null,
			Action<RequestStatus> done = null
		)
		{
			return new DialogRequest(
				States.Request,
				DialogTypes.ConfirmDenyCancel,
				style,
				title,
				message,
				cancelText: cancelText,
				failureText: denyText,
				successText: confirmText,
				cancel: cancelClick,
				failure: denyClick,
				success: confirmClick,
				done: done
			);
		}

		public readonly States State;
		public readonly DialogTypes DialogType;
		public readonly DialogStyles Style;
		public readonly LanguageStringModel Title;
		public readonly LanguageStringModel Message;
		public readonly LanguageStringModel CancelText;
		public readonly LanguageStringModel FailureText;
		public readonly LanguageStringModel SuccessText;
		public readonly Action Cancel;
		public readonly Action Failure;
		public readonly Action Success;
		public readonly Action<RequestStatus> Done;

		public DialogRequest(
			States state,
			DialogTypes dialogType,
			DialogStyles style,
			LanguageStringModel title,
			LanguageStringModel message,
			LanguageStringModel cancelText = null,
			LanguageStringModel failureText = null,
			LanguageStringModel successText = null,
			Action cancel = null,
			Action failure = null,
			Action success = null,
			Action<RequestStatus> done = null
		)
		{
			State = state;
			DialogType = dialogType;
			Style = style == DialogStyles.Unknown ? DialogStyles.Neutral : style;
			Title = title;
			Message = message;
			CancelText = cancelText;
			FailureText = failureText;
			SuccessText = successText;
			Done = done ?? ActionExtensions.GetEmpty<RequestStatus>();
			Cancel = cancel ?? ActionExtensions.Empty;
			Failure = failure ?? ActionExtensions.Empty;
			Success = success ?? ActionExtensions.Empty;
		}

		public DialogRequest Duplicate(States state = States.Unknown)
		{
			return new DialogRequest(
				state == States.Unknown ? State : state,
				DialogType,
				Style,
				Title,
				Message,
				CancelText,
				FailureText,
				SuccessText,
				Cancel,
				Failure,
				Success,
				Done
			);
		}
	}
}