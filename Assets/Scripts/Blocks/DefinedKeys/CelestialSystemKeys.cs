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
		#endregion

		#region Integers
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
		#endregion

		#region Floats
		#endregion

		#region Resources
		public readonly Resource Rations;
		public readonly Resource Propellant;
		public readonly Resource Metallics;
		#endregion

		public CelestialSystemKeys() : base(KeyValueTargets.CelestialSystem)
		{
			Booleans = new Boolean[]
			{

			};

			Integers = new Integer[]
			{
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

			};

			Floats = new Float[]
			{

			};

			Rations = new Resource(KeyDefines.Resources.Rations, this);
			Propellant = new Resource(KeyDefines.Resources.Propellant, this);
			Metallics = new Resource(KeyDefines.Resources.Metallics, this);
		}
	}
}