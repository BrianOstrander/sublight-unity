using System;

namespace LunraGames.SubLight
{
	public class CelestialSystemKeys : KeyDefinitions
	{
		static class Suffixes
		{
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
		#endregion

		#region Strings
		#endregion

		#region Floats - Read & Write
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

			};

			Strings = new String[]
			{

			};

			Floats = new Float[]
			{
				// -- Read & Write
				CreateResource(ref GeneratedRations, Suffixes.Rations, ResourceProperties.Generated),
				CreateResource(ref GeneratedRations, Suffixes.Rations, ResourceProperties.Remaining),
			};
		}

		Float CreateResource(
			ref Float definition,
			string resourceSuffix,
			ResourceProperties property
		)
		{
			switch (property)
			{
				case ResourceProperties.Generated:
					return Create(
						ref definition,
						"generated_" + resourceSuffix,
						"The amount of " + resourceSuffix + " assigned to a system when it was generated."
					);
				case ResourceProperties.Remaining:
					return Create(
						ref definition,
						"remaining_" + resourceSuffix,
						"The amount of " + resourceSuffix + " remaining in a system."
					);
				default:
					throw new ArgumentOutOfRangeException("property", "Unrecognized ResourceProperty: " + property);
			}
		}
	}
}