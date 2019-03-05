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