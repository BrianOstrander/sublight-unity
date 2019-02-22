namespace LunraGames.SubLight
{
	public class GlobalKeys : KeyDefinitions
	{
		#region Booleans
		#endregion

		#region Integers
		#endregion

		#region Strings
		public readonly String PersistentId;
		#endregion

		#region Floats
		#endregion

		public GlobalKeys() : base(KeyValueTargets.Global)
		{
			Booleans = new Boolean[]
			{

			};

			Integers = new Integer[]
			{

			};

			Strings = new String[]
			{
				Create(
					ref PersistentId,
					"persistent_id",
					"The persistent id assigned upon initial installation."
				)
			};

			Floats = new Float[]
			{

			};
		}
	}
}