using System.Linq;

namespace LunraGames.SubLight
{
	public class PreferencesKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean IgnoreTutorial;
		public readonly Boolean InterfaceLarge;
		#endregion

		#region Integers
		#endregion

		#region Strings
		#endregion

		#region Floats
		#endregion

		public IKeyDefinition[] ReloadGameRequired { get; private set; }
		public IKeyDefinition[] ReloadHomeRequired { get; private set; }
		public IKeyDefinition[] RestartRequired { get; private set; }

		public PreferencesKeys() : base(KeyValueTargets.Preferences)
		{
			ReloadGameRequired = new IKeyDefinition[0];
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

		T AppendReloadGameRequired<T>(T instance)
			where T : IKeyDefinition
		{
			ReloadGameRequired = ReloadGameRequired.Append(instance).ToArray();
			return instance;
		}

		T AppendReloadHomeRequired<T>(T instance)
			where T : IKeyDefinition
		{
			ReloadHomeRequired = ReloadHomeRequired.Append(instance).ToArray();
			ReloadGameRequired = ReloadGameRequired.Append(instance).ToArray();
			return instance;
		}

		T AppendRestartRequired<T>(T instance)
			where T : IKeyDefinition
		{
			RestartRequired = RestartRequired.Append(instance).ToArray();
			return instance;
		}
	}
}