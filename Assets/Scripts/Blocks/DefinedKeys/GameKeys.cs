namespace LunraGames.SubLight
{
	public class GameKeys : DefinedKeys
	{
		#region Booleans
		#endregion

		#region Integers
		#endregion

		#region Strings
		#endregion

		#region Floats
		public readonly Float DistanceFromBegin;
		public readonly Float DistanceToEnd;
		public readonly Float DistanceTraveled;
		public readonly Float FurthestTransit;

		public readonly Float YearsElapsedGalactic;
		public readonly Float YearsElapsedShip;
		public readonly Float YearsElapsedDelta;
		#endregion

		public GameKeys() : base(KeyValueTargets.Game)
		{
			Booleans = new Boolean[]
			{

			};

			Integers = new Integer[]
			{

			};

			Strings = new String[]
			{

			};

			Floats = new Float[]
			{
				Create(
					ref DistanceFromBegin,
					"distance_from_begin",
					"How far away in universe units is the player from where they started."
				),
				Create(
					ref DistanceToEnd,
					"distance_to_end",
					"How many universe units away is the end of the game."
				),
				Create(
					ref DistanceTraveled,
					"distance_traveled",
					"How many universe units has the player traveled in total."
				),
				Create(
					ref FurthestTransit,
					"furthest_transit",
					"The farthest distance, in universe units, ever traveled by the player in a single transit."
				),

				Create(
					ref YearsElapsedGalactic,
					"years_elapsed_galactic",
					"Years elapsed in-game from the galactic reference point."
				),
				Create(
					ref YearsElapsedShip,
					"years_elapsed_ship",
					"Years elapsed in-game from the ship reference point."
				),
				Create(
					ref YearsElapsedDelta,
					"years_elapsed_delta",
					"The ship time subtracted from the galactic time. This value will always be greater than or equal to zero."
				)
			};
		}
	}
}