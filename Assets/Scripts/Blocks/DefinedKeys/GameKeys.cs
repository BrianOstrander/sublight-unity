using System.Linq;

namespace LunraGames.SubLight
{
	public class GameKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean IgnoreSystemOutOfRangeWarning;
		public readonly Boolean UnlockedDeception;
		public readonly Boolean UnlockedEmpathy;
		public readonly Boolean UnlockedAggression;
		#endregion

		#region Integers
		public readonly Integer SurfaceProbeCount;
		public readonly Integer SurfaceProbeScanLevel;
		public readonly Integer TransitHistoryCount;
		#endregion

		#region Strings
		public readonly String GamemodeId;

		public readonly String NavigationSelection;
		public readonly String NavigationSelectionName;
		public readonly String NavigationHighlight;
		public readonly String NavigationHighlightName;
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

		public readonly Float TransitRange;

		public readonly Float TransitVelocity;
		#endregion

		public GameKeys() : base(KeyValueTargets.Game)
		{
			Booleans = new Boolean[]
			{
				Create(
					ref IgnoreSystemOutOfRangeWarning,
					"ignore_system_out_of_range_warning",
					"True if the player has been warned about clicking on out of range systems already."
				),
				Create(
					ref UnlockedDeception,
					"unlocked_deception",
					"True if the player has unlocked deception.",
					true
				),
				Create(
					ref UnlockedEmpathy,
					"unlocked_empathy",
					"True if the player has unlocked empathy.",
					true
				),
				Create(
					ref UnlockedAggression,
					"unlocked_aggression",
					"True if the player has unlocked aggression.",
					true
				)
			};

			Integers = new Integer[]
			{
				Create(
					ref SurfaceProbeCount,
					"surface_probe_count",
					"The number of surface probes remaining.",
					true
				),
				Create(
					ref SurfaceProbeScanLevel,
					"surface_probe_scan_level",
					"todo desc",
					true // TODO: should be readable???
				),
				Create(
					ref TransitHistoryCount,
					"transit_history_count",
					"The number of systems visited."
				)
			};

			Strings = new String[]
			{
				Create(
					ref GamemodeId,
					"gamemode_id",
					"The id of the active gamemode."
				),
				Create(
					ref NavigationSelection,
					"navigation_selection",
					GetNavigationSystemNotes("selection"),
					true
				),
				Create(
					ref NavigationSelectionName,
					"navigation_selection_name",
					"The name of the current navigation selection."
				),
				Create(
					ref NavigationHighlight,
					"navigation_highlight",
					GetNavigationSystemNotes("highlight")
				),
				Create(
					ref NavigationHighlightName,
					"navigation_highlight_name",
					"The name of the current navigation highlight."
				)
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
					ref Population,
					"population",
					"The ship's current population.",
					true
				),
				Create(
					ref TransitRange,
					"transit_range",
					"The range of this ship in universe units."
				),
				Create(
					ref TransitVelocity,
					"transit_velocity",
					"The ship's velocity, as a fraction of light speed, that the ship can travel. Only values between zero and one are valid."
				)
			};

			Enumerations = new IEnumeration[]
			{

			};
		}

		static string GetNavigationSystemNotes(string interaction)
		{
			return "The condensed id of the current celestial system " + interaction + ", null or empty if none. " +
				"This information is serialized as:\n\t\"Format_SectorX_SectorY_SectorZ_Index\"\n" +
				"The only format currently supported is \"coordinate\", though others may be added in the future.";
		}
	}
}