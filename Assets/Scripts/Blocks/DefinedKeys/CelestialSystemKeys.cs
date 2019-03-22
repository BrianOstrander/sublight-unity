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
		public readonly Integer PlanetCount;

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

		public readonly Integer AnomalyIndex;
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
					ref PlanetCount,
					"planet_count",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),

				Create(
					ref HabitableAtmosphere,
					"habitable_atmosphere",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref HabitableGravity,
					"habitable_gravity",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref HabitableTemperature,
					"habitable_temperature",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref HabitableWater,
					"habitable_water",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref HabitableResources,
					"habitable_resources",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),

				Create(
					ref ScanLevelAtmosphere,
					"scan_level_atmosphere",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref ScanLevelGravity,
					"scan_level_gravity",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref ScanLevelTemperature,
					"scan_level_temperature",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref ScanLevelWater,
					"scan_level_water",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),
				Create(
					ref ScanLevelResources,
					"scan_level_resources",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
				),

				Create(
					ref AnomalyIndex,
					"anomaly_index",
					"desc todo",
					true // TODO: SHOULD THIS ACTUALLY BE WRITABLE?
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