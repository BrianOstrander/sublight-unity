namespace LunraGames.SubLight
{
	public class EncounterKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean DisableDefaultShipView;
		#endregion

		#region Integers
		#endregion

		#region Strings
		#endregion

		#region Floats
		#endregion

		public EncounterKeys() : base(KeyValueTargets.Encounter)
		{
			Booleans = new Boolean[]
			{
				Create(
					ref DisableDefaultShipView,
					"disable_default_ship_view",
					"True if the default ship view should not be shown when in the ship focus.",
					true
				),
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

			Enumerations = new IEnumeration[]
			{

			};
		}
	}
}