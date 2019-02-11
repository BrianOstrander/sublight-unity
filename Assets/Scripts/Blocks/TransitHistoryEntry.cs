using System;

using Newtonsoft.Json;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TransitHistoryEntry
	{
		public static TransitHistoryEntry Begin(
			DateTime enterTime,
			SystemModel beginSystem
		)
		{
			return new TransitHistoryEntry(
				enterTime,
				RelativeDayTime.Zero,
				beginSystem.Name.Value,
				beginSystem.Position.Value,
				beginSystem.Index.Value,
				0,
				0f,
				0f
			);
		}

		public static TransitHistoryEntry Create(
			DateTime enterTime,
			RelativeDayTime relativeEnterTime,
			SystemModel previousSystem,
			SystemModel nextSystem,
			TransitHistoryEntry lastEntry
		)
		{
			var transitDistance = UniversePosition.Distance(previousSystem.Position.Value, nextSystem.Position.Value);
			return new TransitHistoryEntry(
				enterTime,
				relativeEnterTime,
				nextSystem.Name.Value,
				nextSystem.Position.Value,
				nextSystem.Index.Value,
				lastEntry.TransitCount + 1,
				transitDistance,
				lastEntry.TotalTransitDistance + transitDistance
			);
		}

		[JsonProperty] public readonly DateTime EnterTime;
		[JsonProperty] public readonly RelativeDayTime RelativeEnterTime;
		[JsonProperty] public readonly string SystemName;
		[JsonProperty] public readonly UniversePosition SystemPosition;
		[JsonProperty] public readonly int SystemIndex;
		[JsonProperty] public readonly int TransitCount;
		[JsonProperty] public readonly float TransitDistance;
		[JsonProperty] public readonly float TotalTransitDistance;

		TransitHistoryEntry(
			DateTime enterTime,
			RelativeDayTime relativeEnterTime,
			string systemName,
			UniversePosition systemPosition,
			int systemIndex,
			int transitCount,
			float transitDistance,
			float totalTransitDistance
		)
		{
			EnterTime = enterTime;
			RelativeEnterTime = relativeEnterTime;
			SystemName = systemName;
			SystemPosition = systemPosition;
			SystemIndex = systemIndex;
			TransitCount = transitCount;
			TransitDistance = transitDistance;
			TotalTransitDistance = totalTransitDistance;
		}

	}
}