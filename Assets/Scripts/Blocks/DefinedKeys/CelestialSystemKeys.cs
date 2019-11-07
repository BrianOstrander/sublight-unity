using System;
using System.Linq;

namespace LunraGames.SubLight
{
	public class CelestialSystemKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean Visited;
		public readonly Boolean Specified;
		public readonly Boolean PlayerBegin;
		public readonly Boolean PlayerEnd;
		public readonly Boolean IconPings;
		public readonly Boolean HasSpecifiedWaypoint;
		#endregion

		#region Integers
		public readonly Integer Index;
		public readonly Integer Seed;

		public readonly Integer ScannableBodyIndex;

		public readonly Integer HabitableAtmosphere;
		public readonly Integer HabitableGravity;
		public readonly Integer HabitableTemperature;
		public readonly Integer HabitableWater;
		public readonly Integer HabitableResources;

		public readonly Integer ScanLevelAtmosphere;
		public readonly Integer ScanLevelGravity;
		public readonly Integer ScanLevelTemperature;
		public readonly Integer ScanLevelWater;
		public readonly Integer ScanLevelResources;
		#endregion

		#region Strings
		public readonly String Name;
		public readonly String ClassificationSecondary;

		public readonly String SpecifiedWaypointName;
		public readonly String SpecifiedWaypointId;
		#endregion

		#region Floats
		public readonly Float IconScale;
		#endregion

		#region HsvColors
		public readonly HsvaColor IconColor;
		#endregion

		#region Enumerations
		public readonly Enumeration<SystemClassifications> ClassificationPrimary;
		#endregion

		public CelestialSystemKeys() : base(KeyValueTargets.CelestialSystem)
		{
			Booleans = new Boolean[]
			{
				Create(
					ref Visited,
					"visited",
					"True if the system has been visited at least once."
				),
				Create(
					ref Specified,
					"specified",
					"True if this system was specified in the galaxy model."
				),
				Create(
					ref PlayerBegin,
					"player_begin",
					"True if this system is where the player starts."
				),
				Create(
					ref PlayerEnd,
					"player_end",
					"True if this system is where the game ends."
				),
				Create(
					ref IconPings,
					"icon_pings",
					"True if this system emits radio pings. This also effects the types of encounters that occur.",
					true
				),
				Create(
					ref HasSpecifiedWaypoint,
					"has_specified_waypoint",
					"True if a waypoint has been specified in this system."
				)
			};

			Integers = new Integer[]
			{
				Create(
					ref Index,
					"index",
					"Index of the system within the sector."
				),
				Create(
					ref Seed,
					"seed",
					"Procedurally assigned seed used to generate this system."
				),
				Create(
					ref ScannableBodyIndex,
					"scannable_body_index",
					"The orbit of the scannable body for this system."
				),

				Create(
					ref HabitableAtmosphere,
					"habitable_atmosphere",
					"The habitability of scannable body's atmosphere."
				),
				Create(
					ref HabitableGravity,
					"habitable_gravity",
					"The habitability of scannable body's gravity."
				),
				Create(
					ref HabitableTemperature,
					"habitable_temperature",
					"The habitability of scannable body's temperature."
				),
				Create(
					ref HabitableWater,
					"habitable_water",
					"The habitability of scannable body's water."
				),
				Create(
					ref HabitableResources,
					"habitable_resources",
					"The habitability of scannable body's resources."
				),

				Create(
					ref ScanLevelAtmosphere,
					"scan_level_atmosphere",
					"The minimum upgrade level of the player's scanner to reveal this value."
				),
				Create(
					ref ScanLevelGravity,
					"scan_level_gravity",
					"The minimum upgrade level of the player's scanner to reveal this value."
				),
				Create(
					ref ScanLevelTemperature,
					"scan_level_temperature",
					"The minimum upgrade level of the player's scanner to reveal this value."
				),
				Create(
					ref ScanLevelWater,
					"scan_level_water",
					"The minimum upgrade level of the player's scanner to reveal this value."
				),
				Create(
					ref ScanLevelResources,
					"scan_level_resources",
					"The minimum upgrade level of the player's scanner to reveal this value."
				)
			};

			Strings = new String[]
			{
				Create(
					ref Name,
					"name",
					"Name of the system"
				),
				Create(
					ref ClassificationSecondary,
					"classification_secondary",
					"A description of the system based on the primary classification."
				),
				Create(
					ref SpecifiedWaypointName,
					"specified_waypoint_name",
					"The name of the waypoint specified in this system."
				),
				Create(
					ref SpecifiedWaypointId,
					"specified_waypoint_id",
					"The id used to reference the waypoint in this system."
				)
			};

			Floats = new Float[]
			{
				Create(
					ref IconScale,
					"icon_scale",
					"Scale of the system's icon from 0.0 to 1.0."
				),
				Create(
					ref IconScale,
					"icon_scale",
					"Scale of the system's icon from 0.0 to 1.0."
				)
			};

			Enumerations = new IEnumeration[]
			{
				Create(
					ref ClassificationPrimary,
					"classification_primary",
					"Primary classification for this system."
				)
			};

			Create(
				ref IconColor,
				"icon_color",
				"Color of the system's icon."
			);
		}
	}
}