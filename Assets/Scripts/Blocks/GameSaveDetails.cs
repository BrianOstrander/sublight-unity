using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct GameSaveDetails
	{
		public string TransitHistoryId;
		public DateTime Time;
		public TimeSpan ElapsedTime;

		public bool IsCompleted;
		public EncounterEvents.GameComplete.Conditions CompleteCondition;
		public KeyValueListModel CompleteKeyValues;

		public GameSaveDetails(
			string transitHistoryId,
			DateTime time,
			TimeSpan elapsedTime
		)
		{
			TransitHistoryId = transitHistoryId;
			Time = time;
			ElapsedTime = elapsedTime;

			IsCompleted = false;
			CompleteCondition = EncounterEvents.GameComplete.Conditions.Unknown;
			CompleteKeyValues = new KeyValueListModel();
		}
	}
}