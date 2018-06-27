namespace LunraGames.SpaceFarm
{
	// TODO: Create an actual string system that supports multiple languages.
	public static class Strings
	{
		public const string ConfirmToMainMenu = "Are you sure you want to go back to the main menu?";
		public const string ExplanationTitle = "Disaster!";
		public const string Explanation = "Every communication channel is set ablaze with frantic messages from sol: Earth has been destroyed. A wave of destruction is spreading out into the universe, destroying everything in its path. Avoid the wave of destruction at all costs!";

		public const string OutOfRations = "Out of rations.";
		public const string DestroyedByVoid = "Destroyed by void.";

		public const string WonInfoTitle = "You Won";
		public const string WonInfo = "You made it to the black hole and sling shot yourself to Andromeda, good job!";
		
		public const string HomeInfoTitle = "Earth";
		public const string HomeInfo = "Earth is gone, and Sol along with it... there's nothing left to go back to...";

		public const string EndInfoTitle = "Black Hole";
		public const string EndInfo = "Getting to the black hole is our only chance of escape!";

		public static string ArrivedIn(string system) { return "Arrived in " + system; }
		public static string ArrivedDetails(float rations) { return "Aquired " + rations + " rations"; }
	}
}