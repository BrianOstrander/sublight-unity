namespace LunraGames.SubLight
{
	public class PreferencesKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean IgnoreTutorial;
		#endregion

		#region Integers
		#endregion

		#region Strings
		#endregion

		#region Floats
		#endregion

		public PreferencesKeys() : base(KeyValueTargets.Preferences)
		{
			Booleans = new Boolean[]
			{
				Create(
					ref IgnoreTutorial,
					"ignore_tutorial",
					"True if the initial tutorial should be skipped.",
					true
				)
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
		}
	}
}