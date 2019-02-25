namespace LunraGames.SubLight
{
	public class GlobalKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean HasShownUserAnalyticsWarning;
		public readonly Boolean HasAskedForFeedback;
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
				),
				Create(
					ref HasAskedForFeedback,
					"has_asked_for_feedback",
					"True if the player has been asked for feedback already."
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