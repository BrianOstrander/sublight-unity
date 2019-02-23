namespace LunraGames.SubLight
{
	public class GlobalKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean HasShownUserAnalyticsWarning;
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
				Create(
					ref HasShownUserAnalyticsWarning,
					"has_shown_user_analytics_warning",
					"True if the user been shown an analytics warning in compliance with GDPR."
				)
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