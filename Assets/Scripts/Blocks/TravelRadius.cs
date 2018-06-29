using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	/// <summary>
	/// Travel radius, expressed in universe units.
	/// </summary>
	[Serializable]
	public struct TravelRadius
	{
		public static TravelRadius Zero { get { return new TravelRadius(0f, 0f, 0f); } }

		[JsonProperty] public readonly float SafeRadius;
		[JsonProperty] public readonly float DangerRadius;
		[JsonProperty] public readonly float MaximumRadius;

		public TravelRadius(float safeRadius, float dangerRadius, float maximumRadius)
		{
			SafeRadius = safeRadius;
			DangerRadius = dangerRadius;
			MaximumRadius = maximumRadius;
		}
	}
}