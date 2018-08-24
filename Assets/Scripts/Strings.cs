namespace LunraGames.SubLight
{
	// TODO: Create an actual string system that supports multiple languages.
	public static class Strings
	{
		public static class CannotSaveReasons
		{
			public const string CurrentlySaving = "Currently saving.";
			public const string CurrentlyInEncounter = "Cannot save during an encounter.";
		}

		public const string ConfirmToMainMenu = "Are you sure you want to go back to the main menu?";
		public const string ExplanationTitle0 = "Disaster!";
		public const string Explanation0 = "Every communication channel is set ablaze with frantic messages from sol: Earth has been destroyed. A wave of destruction is spreading out into the universe, destroying everything in its path. Avoid the wave of destruction at all costs!";
		public const string ExplanationTitle1 = "Escape";
		public const string Explanation1 = "Follow the yellow arrow around your ship towards a distant black hole, it's the only chance of escape!";

		public const string OutOfRations = "Out of rations.";
		public const string OutOfFuel = "Not enough fuel in this system to continue.";
		public const string DestroyedByVoid = "Destroyed by void.";

		public const string WonInfoTitle = "You Won";
		public const string WonInfo = "You made it to the black hole and sling shot yourself to Andromeda, good job!";
		
		public const string HomeInfoTitle = "Earth";
		public const string HomeInfo = "Earth is gone, and Sol along with it... there's nothing left to go back to...";

		public const string EndInfoTitle = "Black Hole";
		public const string EndInfo = "Getting to the black hole is our only chance of escape!";

		public static string ArrivedIn(string system) { return "Arrived in " + system; }
		public static string ArrivedDetails(float rations, float fuel) { return "Aquired " + rations.ToString("F1") + " rations and "+fuel.ToString("F1")+" fuel"; }

		public static string Rations(float rations) { return rations.ToString("F2"); }
		public static string Fuel(float fuel) { return fuel.ToString("F2"); }
		public static string Speed(float speed) { return speed.ToString("F2"); }

		public static string GreekAlpha(int index, bool uppercase = false)
		{
			var result = string.Empty;
			switch(index)
			{
				case 0: result = "alpha"; break;
				case 1: result = "beta"; break;
				case 2: result = "gamma"; break;
				case 3: result = "delta"; break;
				case 4: result = "epsilon"; break;
				case 5: result = "zeta"; break;
				case 6: result = "eta"; break;
				case 7: result = "theta"; break;
				case 8: result = "iota"; break;
				case 9: result = "kappa"; break;
				case 10: result = "lambda"; break;
				case 11: result = "mu"; break;
				case 12: result = "nu"; break;
				case 13: result = "xi"; break;
				case 14: result = "omikron"; break;
				case 15: result = "pi"; break;
				case 16: result = "rho"; break;
				case 17: result = "sigma"; break;
				case 18: result = "tau"; break;
				case 19: result = "upsilon"; break;
				case 20: result = "phi"; break;
				case 21: result = "chi"; break;
				case 22: result = "psi"; break;
				case 23: result = "omega"; break;
				default: result = index.ToString(); break;
			}
			return uppercase ? char.ToUpper(result[0]) + result.Substring(1) : result;
		}
	}
}