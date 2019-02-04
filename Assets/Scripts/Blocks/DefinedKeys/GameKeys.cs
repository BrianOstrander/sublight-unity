namespace LunraGames.SubLight
{
	public class GameKeys : DefinedKeys
	{
		#region Booleans
		#endregion

		#region Integers
		#endregion

		#region Strings
		#endregion

		#region Floats
		public readonly Float LightYearsFromBegin;
		public readonly Float LightYearsToEnd;
		#endregion

		public GameKeys() : base(KeyValueTargets.Game)
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
				Create(
					ref LightYearsFromBegin,
					"LightYearsFromBegin",
					"How far in light years has the player traveled since beginning the game."
				),
				Create(
					ref LightYearsToEnd,
					"LightYearsToEnd",
					"How many light years away is the end of the game."
				)
			};
		}
	}
}