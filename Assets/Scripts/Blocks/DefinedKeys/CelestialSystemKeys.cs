using System;
using System.Linq;

namespace LunraGames.SubLight
{
	public class CelestialSystemKeys : KeyDefinitions
	{
		public class Resource
		{
			public readonly Float GatherMultiplier;
			public readonly Float GatheredAmount;
			public readonly Boolean Discovered;

			public Resource(
				string resource,
				CelestialSystemKeys instance
			)
			{
				instance.Floats = instance.Floats.Append(
					instance.Create(
						ref GatherMultiplier,
						"gather_multiplier_" + resource,
						"The multiplier applied to the ark's gather rate when gathering " + resource + "."
					)
				).ToArray();

				instance.Floats = instance.Floats.Append(
					instance.Create(
						ref GatheredAmount,
						"gather_amount_" + resource,
						"The of " + resource + " gathered in this system already, or zero if there was never any to begin with."
					)
				).ToArray();

				instance.Booleans = instance.Booleans.Append(
					instance.Create(
						ref Discovered,
						"discovered_" + resource,
						"True if the ark passed through this system after discovering " + resource + ". Allows newly discovered resources to be gathered upon returning to previously visited systems."
					)
				).ToArray();
			}
		}

		#region Booleans
		public readonly Boolean Visited;
		public readonly Boolean Specified;
		public readonly Boolean PlayerBegin;
		public readonly Boolean PlayerEnd;
		public readonly Boolean IconPings;
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
		#endregion

		#region Floats
		public readonly Float IconScale;
		#endregion

		#region Resources
		public readonly Resource Rations;
		public readonly Resource Propellant;
		public readonly Resource Metallics;
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

			Rations = new Resource(KeyDefines.Resources.Rations, this);
			Propellant = new Resource(KeyDefines.Resources.Propellant, this);
			Metallics = new Resource(KeyDefines.Resources.Metallics, this);

			Create(
				ref IconColor,
				"icon_color",
				"Color of the system's icon."
			);
		}
	}
}