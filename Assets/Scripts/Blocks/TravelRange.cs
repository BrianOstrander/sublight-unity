using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TravelRange
	{
		public readonly float Minimum;
		public readonly float Ship;
		public readonly float Total;

		public readonly float MinimumLightYears;
		public readonly float ShipLightYears;
		public readonly float TotalLightYears;

		public TravelRange(
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

		public TravelRange NewMinimum(float minimum)
		{
			return new TravelRange(minimum, Ship);
		}

		public TravelRange NewShip(float ship)
		{
			return new TravelRange(Minimum, ship);
		}

		public TravelRange Duplicate(
			float? minumum = null,
			float? ship = null
		)
		{
			return new TravelRange(
				minumum ?? Minimum,
				ship ?? Ship
			);
		}
	}
}