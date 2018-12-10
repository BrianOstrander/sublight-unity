using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TravelRange
	{
		public readonly float Minimum;
		public readonly float Ship;
		public readonly float MinimumTotal;
		public readonly float Multiplier;
		public readonly float Total;

		public readonly float MinimumLightYears;
		public readonly float ShipLightYears;
		public readonly float MinimumTotalLightYears;
		public readonly float TotalLightYears;

		public TravelRange(
			float minimum,
			float ship,
			float multiplier
		)
		{
			Minimum = minimum;
			Ship = ship;
			MinimumTotal = minimum + ship;
			Multiplier = multiplier;
			Total = multiplier * MinimumTotal;

			MinimumLightYears = UniversePosition.ToLightYearDistance(Minimum);
			ShipLightYears = UniversePosition.ToLightYearDistance(Ship);
			MinimumTotalLightYears = UniversePosition.ToLightYearDistance(MinimumTotal);
			TotalLightYears = UniversePosition.ToLightYearDistance(Total);
		}

		public TravelRange NewMinimum(float minimum)
		{
			return new TravelRange(minimum, Ship, Multiplier);
		}

		public TravelRange NewShip(float ship)
		{
			return new TravelRange(Minimum, ship, Multiplier);
		}

		public TravelRange NewMultiplier(float multiplier)
		{
			return new TravelRange(Minimum, Ship, multiplier);
		}

		public TravelRange Duplicate(
			float? minumum = null,
			float? ship = null,
			float? multiplier = null
		)
		{
			return new TravelRange(
				minumum ?? Minimum,
				ship ?? Ship,
				multiplier ?? Multiplier
			);
		}
	}
}