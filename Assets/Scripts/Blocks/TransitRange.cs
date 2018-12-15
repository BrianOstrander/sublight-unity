using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TransitRange
	{
		public static TransitRange Default { get { return new TransitRange(); } }

		public readonly float Minimum;
		public readonly float Ship;
		public readonly float Total;

		public readonly float MinimumLightYears;
		public readonly float ShipLightYears;
		public readonly float TotalLightYears;

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