using UnityEngine;

namespace LunraGames.SubLight
{
	// TODO: Use Result and ResultArray instead!
	public struct RequestResult
	{
		public static RequestResult Success(string message = null) { return new RequestResult(RequestStatus.Success, message); }
		public static RequestResult Failure(string message) { return new RequestResult(RequestStatus.Failure, message); }
		public static RequestResult Cancel(string message = null) { return new RequestResult(RequestStatus.Cancel, message); }

		public readonly RequestStatus Status;
		public readonly string Message;

		public bool IsNotSuccess { get { return Status != RequestStatus.Success; } }

		public RequestResult(
			RequestStatus status,
			string message
		)
		{
			Status = status;
			Message = message;
		}

		public RequestResult Log()
		{
			var result = Status + ": " + (string.IsNullOrEmpty(Message) ? (Message == null ? "< null >" : "< empty >") : Message);
			switch (Status)
			{
				case RequestStatus.Failure: Debug.LogError(result); break;
				case RequestStatus.Cancel: Debug.LogWarning(result); break;
				case RequestStatus.Success: Debug.Log(result); break;
				default: Debug.LogError("Unrecognized RequestStatus: " + Status + ", result:\n" + result); break;
			}
			return this;
		}
	}
}