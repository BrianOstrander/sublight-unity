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
		public readonly Integer AudioLevelsInitializedVersion;
		#endregion

		#region Strings
		#endregion

		#region Floats
		public readonly Float UserAudioLevel;
		public readonly Float AmbientAudioLevel;
		public readonly Float EffectsAudioLevel;
		public readonly Float MusicAudioLevel;
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
					canRead: false,
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
				Create(
					ref AudioLevelsInitializedVersion,
					"audio_levels_initialized_version",
					"The version the audio levels were last initialized at, zero means they've never been initialized.",
					canRead: false
				)
			};

			Strings = new String[]
			{

			};

			Floats = new Float[]
			{
				Create(
					ref UserAudioLevel,
					"user_audio_level",
					"The user audio level from 0.0 to 1.0. This is the master for all others.",
					canRead: false
				),
				Create(
					ref AmbientAudioLevel,
					"ambient_audio_level",
					"The ambient audio level from 0.0 to 1.0.",
					canRead: false
				),
				Create(
					ref EffectsAudioLevel,
					"effects_audio_level",
					"The effects audio level from 0.0 to 1.0.",
					canRead: false
				),
				Create(
					ref MusicAudioLevel,
					"music_audio_level",
					"The music audio level from 0.0 to 1.0.",
					canRead: false
				)
			};

			Enumerations = new IEnumeration[]
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