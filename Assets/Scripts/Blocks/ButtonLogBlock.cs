using System;

namespace LunraGames.SubLight
{
	/// <summary>
	/// The information that results from parsing and filtering button entries
	/// in a ButtonEncounterLogModel.
	/// </summary>
	public struct ButtonLogBlock
	{
		public string Message;
		public bool Used;
		public bool Interactable;
		public Action Click;

		public ButtonLogBlock(
			string message,
			bool used,
			bool interactable,
			Action click
		)
		{
			Message = message;
			Used = used;
			Interactable = interactable;
			Click = click;
		}
	}
}