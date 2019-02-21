using System;

namespace LunraGames.SubLight
{
	public class CelestialSystemKeys : KeyDefinitions
	{
		static class Suffixes
		{
			public const string Propellant = "propellant";
			public const string Rations = "rations";
		}

		enum ResourceProperties
		{
			Unknown = 0,
			Generated = 10,
			Remaining = 20
		}

		#region Booleans
		#endregion

		#region Integers
		public readonly Integer GeneratedPropellant;
		public readonly Integer RemainingPropellant;
		#endregion

		#region Strings
		#endregion

		#region Floats
		public readonly Float GeneratedRations;
		public readonly Float RemainingRations;
		#endregion

		public CelestialSystemKeys() : base(KeyValueTargets.CelestialSystem)
		{
			Booleans = new Boolean[]
			{

			};

			Integers = new Integer[]
			{
				CreateResource(ref GeneratedPropellant, Suffixes.Propellant, ResourceProperties.Generated),
				CreateResource(ref RemainingPropellant, Suffixes.Propellant, ResourceProperties.Remaining)
			};

			Strings = new String[]
			{

			};

			Floats = new Float[]
			{
				CreateResource(ref GeneratedRations, Suffixes.Rations, ResourceProperties.Generated),
				CreateResource(ref RemainingRations, Suffixes.Rations, ResourceProperties.Remaining),
			};
		}

		Float CreateResource(
			ref Float definition,
			string resourceSuffix,
			ResourceProperties property
		)
		{
			string key;
			string notes;
			CreateResourceDetails(resourceSuffix, property, out key, out notes);

			return Create(ref definition, key, notes, true);
		}

		Integer CreateResource(
			ref Integer definition,
			string resourceSuffix,
			ResourceProperties property
		)
		{
			string key;
			string notes;
			CreateResourceDetails(resourceSuffix, property, out key, out notes);

			return Create(ref definition, key, notes, true);
		}

		void CreateResourceDetails(
			string resourceSuffix,
			ResourceProperties property,
			out string key,
			out string notes
		)
		{
			switch (property)
			{
				case ResourceProperties.Generated:
					key = "generated_" + resourceSuffix;
					notes = "The amount of " + resourceSuffix + " assigned to a system when it was generated.";
					break;
				case ResourceProperties.Remaining:
					key = "remaining_" + resourceSuffix;
					notes = "The amount of " + resourceSuffix + " remaining in a system.";
					break;
				default:
					throw new ArgumentOutOfRangeException("property", "Unrecognized ResourceProperty: " + property);
			}
		}
	}
}