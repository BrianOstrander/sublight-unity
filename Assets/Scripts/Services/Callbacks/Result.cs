using UnityEngine;

namespace LunraGames.SubLight
{
	public struct Result<T>
	{
		public readonly RequestStatus Status;
		public readonly T Payload;
		public readonly string Error;

		public static Result<T> Success(T payload)
		{
			return new Result<T>(
				RequestStatus.Success,
				payload
			);
		}

		public static Result<T> Failure(T payload, string error)
		{
			return new Result<T>(
				RequestStatus.Failure,
				payload,
				error
			);
		}

		Result(
			RequestStatus status,
			T payload,
			string error = null
		)
		{
			Status = status;
			Payload = payload;
			Error = error;
		}
		
		public Result<T> Log()
		{
			switch (Status)
			{
				case RequestStatus.Failure: Debug.LogError(this); break;
				case RequestStatus.Cancel: Debug.LogWarning(this); break;
				case RequestStatus.Success: Debug.Log(this); break;
				default: Debug.LogError("Unrecognized RequestStatus: " + Status + ", result:\n" + this); break;
			}
			return this;
		}

		public override string ToString()
		{
			var result = "Result<" + typeof(T).Name + ">.Status : " + Status;
			switch (Status)
			{
				case RequestStatus.Success: break;
				default:
					result += " - " + Error;
					break;
			}
			return result + "\n- Payload -\n" + Payload.ToReadableJson();
		}
	}
}