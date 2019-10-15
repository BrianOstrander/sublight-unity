using System.Linq;

namespace LunraGames.SubLight
{
	public struct ResultArray<T>
	{
		public readonly RequestStatus Status;
		public readonly Result<T>[] Results;
		public readonly string Error;

		public T[] Payloads => Results.Select(r => r.Payload).ToArray();

		public static ResultArray<T> Success(Result<T>[] results)
		{
			return new ResultArray<T>(
				RequestStatus.Success,
				results
			);
		}

		public static ResultArray<T> Failure(Result<T>[] results, string error)
		{
			return new ResultArray<T>(
				RequestStatus.Failure,
				results,
				error
			);
		}

		ResultArray(
			RequestStatus status,
			Result<T>[] results,
			string error = null
		)
		{
			Status = status;
			Results = results;
			Error = error;
		}

		public override string ToString()
		{
			var result = "ResultArray<" + typeof(T).Name + ">.Status : " + Status;
			switch (Status)
			{
				case RequestStatus.Success: break;
				default:
					result += " - " + Error;
					break;
			}
			result += "\n--- Payloads ---\n";

			foreach (var entry in Results) result += entry.ToString();

			return result;
		}
	}
}