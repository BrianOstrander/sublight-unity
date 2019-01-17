using System;

namespace LunraGames.SubLight
{
	/// <summary>
	/// The information that results from parsing and filtering dialog entries
	/// in a DialogEncounterLogModel.
	/// </summary>
	public struct DialogLogBlock
	{
		public string Title;
		public string Message;
		public DialogTypes DialogType;
		public DialogStyles DialogStyle;

		public string SuccessText;
		public string FailureText;
		public string CancelText;

		public Action SuccessClick;
		public Action FailureClick;
		public Action CancelClick;

		public DialogLogBlock(
			string title,
			string message,
			DialogTypes dialogType,
			DialogStyles dialogStyle,
			string successText,
			string failureText,
			string cancelText,
			Action succesClick,
			Action failureClick,
			Action cancelClick
		)
		{
			Title = title;
			Message = message;
			DialogType = dialogType;
			DialogStyle = dialogStyle;

			SuccessText = successText;
			FailureText = failureText;
			CancelText = cancelText;

			SuccessClick = succesClick ?? ActionExtensions.Empty;
			FailureClick = failureClick ?? ActionExtensions.Empty;
			CancelClick = cancelClick ?? ActionExtensions.Empty;
		}

		public DialogLogBlock Duplicate(
			Action<Action> wrappedSuccessClick,
			Action<Action> wrappedFailureClick,
			Action<Action> wrappedCancelClick
		)
		{
			var oldSuccessClick = SuccessClick;
			var oldFailureClick = FailureClick;
			var oldCancelClick = CancelClick;

			return new DialogLogBlock(
				Title,
				Message,
				DialogType,
				DialogStyle,
				SuccessText,
				FailureText,
				CancelText,
				() => wrappedSuccessClick(oldSuccessClick),
				() => wrappedFailureClick(oldFailureClick),
				() => wrappedCancelClick(oldCancelClick)
			);
		}
	}
}