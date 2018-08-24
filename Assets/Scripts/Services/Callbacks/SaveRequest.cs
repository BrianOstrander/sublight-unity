using System;

namespace LunraGames.SubLight
{
	public struct SaveRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static SaveRequest Request(Action<SaveRequest> done = null) { return new SaveRequest(States.Request, done: done); }
		public static SaveRequest Success(SaveRequest request) { return request.Duplicate(States.Complete, RequestStatus.Success, null); }
		public static SaveRequest Failure(SaveRequest request, string error) { return request.Duplicate(States.Complete, RequestStatus.Failure, error); }

		public readonly States State;
		public readonly RequestStatus Status;
		public readonly string Error;
		public readonly Action<SaveRequest> Done;

		SaveRequest(
			States state,
			RequestStatus status = RequestStatus.Success,
			string error = null,
			Action<SaveRequest> done = null
		)
		{
			State = state;
			Status = status;
			Error = error;
			Done = done ?? ActionExtensions.GetEmpty<SaveRequest>();
		}

		SaveRequest Duplicate(
			States state,
			RequestStatus status,
			string error
		)
		{
			return new SaveRequest(
				state,
				status,
				error,
				Done
			);
		}
	}
}