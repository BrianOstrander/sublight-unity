using System.Linq;

namespace LunraGames.SubLight
{
	public class PreferencesKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean IgnoreTutorial;
		public readonly Boolean InterfaceLarge;
		public readonly Boolean IsDemoMode;
		#endregion

		#region Integers
		#endregion

		#region Strings
		#endregion

		#region Floats
		#endregion

		public IKeyDefinition[] BlockedDuringGame { get; private set; }
		public IKeyDefinition[] ReloadHomeRequired { get; private set; }
		public IKeyDefinition[] RestartRequired { get; private set; }

		public PreferencesKeys() : base(KeyValueTargets.Preferences)
		{
			BlockedDuringGame = new IKeyDefinition[0];
			ReloadHomeRequired = new IKeyDefinition[0];
			RestartRequired = new IKeyDefinition[0];

			Booleans = new Boolean[]
			{
				Create(
					ref IgnoreTutorial,
					"ignore_tutorial",
					"True if the initial tutorial should be skipped.",
					true
				),
				Create(
					ref InterfaceLarge,
					"interface_large",
					"True if the interface should be large. This is temporary until proper integer based scaling is added.",
					created: AppendReloadHomeRequired
				),
				Create(
					ref IsDemoMode,
					"is_demo_mode",
					"True if the game is in demo mode. This causes certain values to be changed upon entering a game, while other functions are ignored."
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

		T AppendBlockedDuringGame<T>(T instance)
			where T : IKeyDefinition
		{
			BlockedDuringGame = BlockedDuringGame.Append(instance).ToArray();
			return instance;
		}

		T AppendReloadHomeRequired<T>(T instance)
			where T : IKeyDefinition
		{
			ReloadHomeRequired = ReloadHomeRequired.Append(instance).ToArray();
			BlockedDuringGame = BlockedDuringGame.Append(instance).ToArray();
			return instance;
		}

		T AppendRestartRequired<T>(T instance)
			where T : IKeyDefinition
		{
			RestartRequired = RestartRequired.Append(instance).ToArray();
			BlockedDuringGame = BlockedDuringGame.Append(instance).ToArray();
			return instance;
		}
	}
}