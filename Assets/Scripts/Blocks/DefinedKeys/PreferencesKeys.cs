using System.Linq;

namespace LunraGames.SubLight
{
	public class PreferencesKeys : KeyDefinitions
	{
		#region Booleans
		public readonly Boolean IgnoreTutorial;
		#endregion

		#region Integers
		public readonly Integer InterfaceScale;
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
				)
			};

			Integers = new Integer[]
			{
				Create(
					ref InterfaceScale,
					"interface_scale",
					"Scale of the current interface.",
					created: AppendReloadHomeRequired
				)
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