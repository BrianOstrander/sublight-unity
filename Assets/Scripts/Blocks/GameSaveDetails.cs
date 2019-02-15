using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct GameSaveDetails
	{
		[JsonProperty] public readonly string TransitHistoryId;
		[JsonProperty] public readonly DateTime Time;
		[JsonProperty] public readonly TimeSpan ElapsedTime;

		public GameSaveDetails(
			string transitHistoryId,
			DateTime time,
			TimeSpan elapsedTime
		)
		{
			TransitHistoryId = transitHistoryId;
			Time = time;
			ElapsedTime = elapsedTime;
		}
	}
}