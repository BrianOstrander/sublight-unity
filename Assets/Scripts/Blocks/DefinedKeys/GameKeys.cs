namespace LunraGames.SubLight
{
	public class GameKeys : KeyDefinitions
	{
		#region Booleans
		#endregion

		#region Integers - Read & Write
		public readonly Integer Rationing;
		public readonly Integer RationingMinimum;
		public readonly Integer RationingMaximum;

		public readonly Integer TransitsWithoutRations;
		public readonly Integer TransitsWithoutRationsMaximum;
		public readonly Integer TransitsWithoutRationsUntilFailure;

		public readonly Integer TransitsWithOverPopulation;
		public readonly Integer TransitsWithOverPopulationMaximum;
		public readonly Integer TransitsWithOverPopulationUntilFailure;

		public readonly Integer TransitsWithUnderPopulation;
		public readonly Integer TransitsWithUnderPopulationMaximum;
		public readonly Integer TransitsWithUnderPopulationUntilFailure;

		public readonly Integer Propellant;
		public readonly Integer PropellantMaximum;
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

		public readonly Float PreviousTransitYearsElapsedShip;

		public readonly Float Population;
		public readonly Float PopulationMinimum;
		public readonly Float PopulationMaximumMultiplier;
		public readonly Float PopulationMaximum;
		public readonly Float PopulationRationingMultiplier;

		public readonly Float ShipPopulationMinimum;
		public readonly Float ShipPopulationMaximum;

		public readonly Float Rations;
		public readonly Float RationsMaximum;
		public readonly Float RationsConsumptionMultiplier;

		public readonly Float TransitRangeMinimum;
		public readonly Float TransitVelocityMinimum;
		#endregion

		public GameKeys() : base(KeyValueTargets.Game)
		{
			Booleans = new Boolean[]
			{

			};

			Integers = new Integer[]
			{
				Create(
					ref Rationing,
					"rationing",
					"How severe the rationing is, zero is sufficient, less than zero is insufficient, and more than zero is plentiful.",
					true
				),
				Create(
					ref RationingMinimum,
					"rationing_minimum",
					"The minimum possible amount rationing can be set to, should be less than zero.",
					true
				),
				Create(
					ref RationingMaximum,
					"rationing_maximum",
					"The maximum possible amount rationing can be set to, should be greater than zero.",
					true
				),
				Create(
					ref TransitsWithoutRations,
					"transits_without_rations",
					"How many transits without even the meagerest of rations. Resets upon a transit with at least meager rations.",
					true
				),
				Create(
					ref TransitsWithoutRationsMaximum,
					"transits_without_rations_maximum",
					"The maximum number of transits without rations before failure.",
					true
				),
				Create(
					ref TransitsWithoutRationsUntilFailure,
					"transits_without_rations_until_failure",
					"How many more transits without rations can the ark survive."
				),
				Create(
					ref TransitsWithOverPopulation,
					"transits_with_over_population",
					"How many transits with too many people onboard. Resets to zero once there is enough space for everyone.",
					true
				),
				Create(
					ref TransitsWithOverPopulationMaximum,
					"transits_with_over_population_maximum",
					"The maximum number of transits with overpopulation before failure.",
					true
				),
				Create(
					ref TransitsWithOverPopulationUntilFailure,
					"transits_with_over_population_until_failure",
					"How many more transits with overpopulation can the ark survive."
				),
				Create(
					ref TransitsWithUnderPopulation,
					"transits_with_under_population",
					"How many transits with less population than the minimum. Rests to zero once there the minimum number of people is reached.",
					true
				),
				Create(
					ref TransitsWithUnderPopulationMaximum,
					"transits_with_under_population_maximum",
					"The maximum number of transits with underpopulation before failure.",
					true
				),
				Create(
					ref TransitsWithUnderPopulationUntilFailure,
					"transits_with_under_population_until_failure",
					"How many more transits with underpopulation can the ark survive."
				),
				Create(
					ref Propellant,
					"propellant",
					"Current amount of propellant on board.",
					true
				),
				Create(
					ref PropellantMaximum,
					"propellant_maximum",
					"The maximum propellant the ark can store.",
					true
				),
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
				),
				Create(
					ref PreviousTransitYearsElapsedShip,
					"previous_transit_years_elapsed_ship",
					"The number of years elapsed during the last transit, from the ship reference point."
				),
				Create(
					ref PopulationMinimum,
					"population_minimum",
					"The minimum population allowed in the game.",
					true
				),
				Create(
					ref PopulationMaximumMultiplier,
					"population_maximum_multiplier",
					"The multiplier used to find the maximum population allowed in the game, multiplied with the ship's maximum population.",
					true
				),
				Create(
					ref PopulationMaximum,
					"population_maximum",
					"The current population maximum allowed in game, using the ship's maximum and the population maximum multiplier."
				),
				Create(
					ref PopulationRationingMultiplier,
					"population_rationing_multiplier",
					"The change in population by rationing level, when rations are insufficient or plentiful.",
					true
				),
				Create(
					ref ShipPopulationMinimum,
					"ship_population_minimum",
					"The minimum population required to operate the ship, below this and underpopulation will be a problem.",
					true
				),
				Create(
					ref ShipPopulationMaximum,
					"ship_population_maximum",
					"The maximum population allowed aboard the ship before overpopulation becomes a problem.",
					true
				),
				Create(
					ref RationsMaximum,
					"rations_maximum",
					"The maximum rations the ship can store.",
					true
				),
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
				),
				Create(
					ref RationsConsumptionMultiplier,
					"rations_consumption_multiplier",
					"The amount of rations 1 population consumes per year when rationing is zero.",
					true
				),
				Create(
					ref TransitRangeMinimum,
					"transit_range_minimum",
					"The minimum range of this ark in universe units.",
					true
				),
				Create(
					ref TransitVelocityMinimum,
					"transit_velocity_minimum",
					"The minimum velocity, as a fraction of light speed, that the ark can travel. Only values between zero and one are valid.",
					true
				)
			};
		}
	}
}