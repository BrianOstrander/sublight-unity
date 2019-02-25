using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TransitRange
	{
		public static TransitRange Default { get { return new TransitRange(); } }

		[JsonProperty] public readonly float Minimum;
		[JsonProperty] public readonly float Ship;
		[JsonProperty] public readonly float Total;

		[JsonProperty] public readonly float MinimumLightYears;
		[JsonProperty] public readonly float ShipLightYears;
		[JsonProperty] public readonly float TotalLightYears;

		TransitRange(
			float minimum,
			float ship
		)
		{
			Minimum = minimum;
			Ship = ship;
			Total = minimum + ship;

			MinimumLightYears = UniversePosition.ToLightYearDistance(Minimum);
			ShipLightYears = UniversePosition.ToLightYearDistance(Ship);
			TotalLightYears = UniversePosition.ToLightYearDistance(Total);
		}

		public TransitRange NewMinimum(float minimum)
		{
			return new TransitRange(minimum, Ship);
		}

		public TransitRange NewShip(float ship)
		{
			return new TransitRange(Minimum, ship);
		}

		public TransitRange Duplicate(
			float? minumum = null,
			float? ship = null
		)
		{
			return new TransitRange(
				minumum ?? Minimum,
				ship ?? Ship
			);
		}
	}
}