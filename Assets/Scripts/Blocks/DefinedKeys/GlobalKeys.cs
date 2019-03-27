namespace LunraGames.SubLight
{
	public class GlobalKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean IgnoreUserAnalyticsWarning;
		public readonly Boolean IgnoreFeedbackRequest;
		#endregion

		#region Integers
		public readonly Integer PreviousChangelogVersion;
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
					ref IgnoreUserAnalyticsWarning,
					"ignore_user_analytics_warning",
					"True if the player been shown an analytics warning in compliance with GDPR."
				),
				Create(
					ref IgnoreFeedbackRequest,
					"ignore_feedback_request",
					"True if the player has been asked for feedback already."
				)
			};

			Integers = new Integer[]
			{
				Create(
					ref PreviousChangelogVersion,
					"previous_changelog_version",
					"The version of the game when a changelog was last shown to the player."
				)
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

			Enumerations = new IEnumeration[]
			{

			};
		}
	}
}