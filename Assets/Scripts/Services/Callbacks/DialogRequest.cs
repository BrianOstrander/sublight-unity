﻿using System;

namespace LunraGames.SubLight
{
	public struct DialogRequest
	{
		static class Defaults
		{
			public const string AlertTitle = "Alert";
			public const string CancelConfirmTitle = "Confirm";
			public const string CancelDenyConfirmTitle = "Confirm";

			public const string Cancel = "Cancel";
			public const string Failure = "Deny";
			public const string Success = "Okay";
		}

		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		/// <summary>
		/// Creates an Alert request.
		/// </summary>
		/// <returns>The alert.</returns>
		/// <param name="message">Message.</param>
		/// <param name="title">Title.</param>
		/// <param name="done">Done.</param>
		public static DialogRequest Alert(
			string message, 
			string title = null,
			Action done = null
		)
		{
			return new DialogRequest(
				States.Request,
				DialogTypes.Alert,
				title ?? Defaults.AlertTitle,
				message,
				successText: Defaults.Success,
				success: done
			);
		}

		/// <summary>
		/// Creates a CancelConfirm request.
		/// </summary>
		/// <returns>The confirm.</returns>
		/// <param name="message">Message.</param>
		/// <param name="title">Title.</param>
		/// <param name="cancel">Cancel.</param>
		/// <param name="confirm">Confirm.</param>
		/// <param name="done">Done.</param>
		public static DialogRequest CancelConfirm(
			string message,
			string title = null,
			Action cancel = null,
			Action confirm = null,
			Action<RequestStatus> done = null
		)
		{
			return new DialogRequest(
				States.Request,
				DialogTypes.CancelConfirm,
				title ?? Defaults.AlertTitle,
				message,
				cancelText: Defaults.Cancel,
				successText: Defaults.Success,
				cancel: cancel,
				success: confirm,
				done: done
			);
		}

		/// <summary>
		/// Creates a CancelDenyConfirm request.
		/// </summary>
		/// <returns>The deny confirm.</returns>
		/// <param name="message">Message.</param>
		/// <param name="title">Title.</param>
		/// <param name="cancel">Cancel.</param>
		/// <param name="deny">Deny.</param>
		/// <param name="confirm">Confirm.</param>
		/// <param name="done">Done.</param>
		public static DialogRequest CancelDenyConfirm(
			string message,
			string title = null,
			Action cancel = null,
			Action deny = null,
			Action confirm = null,
			string cancelText = null,
			string denyText = null,
			string confirmText = null,
			Action<RequestStatus> done = null
		)
		{
			return new DialogRequest(
				States.Request,
				DialogTypes.CancelDenyConfirm,
				title ?? Defaults.AlertTitle,
				message,
				cancelText: cancelText ?? Defaults.Cancel,
				failureText: denyText ?? Defaults.Failure,
				successText: confirmText ?? Defaults.Success,
				cancel: cancel,
				failure: deny,
				success: confirm,
				done: done
			);
		}

		public readonly States State;
		public readonly DialogTypes DialogType;
		public readonly string Title;
		public readonly string Message;
		public readonly string CancelText;
		public readonly string FailureText;
		public readonly string SuccessText;
		public readonly Action Cancel;
		public readonly Action Failure;
		public readonly Action Success;
		public readonly Action<RequestStatus> Done;

		DialogRequest(
			States state,
			DialogTypes dialogType,
			string title,
			string message,
			string cancelText = null,
			string failureText = null,
			string successText = null,
			Action cancel = null,
			Action failure = null,
			Action success = null,
			Action<RequestStatus> done = null
		)
		{
			State = state;
			DialogType = dialogType;
			Title = title ?? string.Empty;
			Message = message ?? string.Empty;
			CancelText = cancelText ?? string.Empty;
			FailureText = failureText ?? string.Empty;
			SuccessText = successText ?? string.Empty;
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