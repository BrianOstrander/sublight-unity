namespace LunraGames.SubLight
{
	public class GameKeys : DefinedKeys
	{
		#region Booleans
		#endregion

		#region Integers - Read & Write
		public readonly Integer Rationing;
		public readonly Integer TransitsWithoutRations;
		public readonly Integer TransitsWithOverPopulation;
		public readonly Integer TransitsWithUnderPopulation;
		#endregion

		#region Strings
		#endregion

		#region Floats - Write Only
		public readonly Float DistanceFromBegin;
		public readonly Float DistanceToEnd;
		public readonly Float DistanceTraveled;
		public readonly Float FurthestTransit;

		public readonly Float YearsElapsedGalactic;
		public readonly Float YearsElapsedShip;
		public readonly Float YearsElapsedDelta;

		public readonly Float PopulationMinimum;
		public readonly Float PopulationMaximum;

		public readonly Float RationsMaximum;
		#endregion

		#region Floats - Read & Write
		public readonly Float Population;

		public readonly Float Rations;
		#endregion

		public GameKeys() : base(KeyValueTargets.Game)
		{
			Booleans = new Boolean[]
			{

			};

			Integers = new Integer[]
			{
				// -- Read & Write
				Create(
					ref Rationing,
					"rationing",
					"How severe the rationing is, zero is sufficient, less than zero is insufficient, and more than zero is plentiful.",
					true
				),
				Create(
					ref TransitsWithoutRations,
					"transits_without_rations",
					"How many transits without even the meagerest of rations. Resets upon a transit with at least meager rations.",
					true
				),
				Create(
					ref TransitsWithOverPopulation,
					"transits_with_over_population",
					"How many transits with too many people onboard. Resets to zero once there is enough space for everyone.",
					true
				),
				Create(
					ref TransitsWithUnderPopulation,
					"transits_with_under_population",
					"How many transits with less population than the minimum. Rests to zero once there the minimum number of people is reached.",
					true
				)
			};

			Strings = new String[]
			{

			};

			Floats = new Float[]
			{
				// -- Write Only
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
				),
				Create(
					ref PopulationMinimum,
					"population_minimum",
					"The minimum population required to run the ship."
				),
				Create(
					ref PopulationMaximum,
					"population_maximum",
					"The maximum population the ship can support."
				),
				Create(
					ref RationsMaximum,
					"rations_maximum",
					"The maximum rations the ship can store."
				),
				// -- Read & Write
				Create(
					ref Population,
					"population",
					"The ship's current population.",
					true
				),
				Create(
					ref Rations,
					"rations",
					"The ship's current store of rations.",
					true
				)
			};
		}
	}
}