using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ShipModel : Model
	{
		#region Serialized
		[JsonProperty] UniversePosition position;
		[JsonIgnore] public readonly ListenerProperty<UniversePosition> Position;

		[JsonProperty] int systemIndex;
		/// <summary>
		/// The index of the current system.
		/// </summary>
		/// <remarks>
		/// This should not be set manually outside of
		/// GameModel.SetCurrentSystem or GameService.
		/// </remarks>
		[JsonIgnore] public readonly ListenerProperty<int> SystemIndex;
		
		[JsonProperty] ModuleModel[] modules = new ModuleModel[0];
		[JsonIgnore] public readonly ListenerProperty<ModuleModel[]> Modules;
		#endregion

		public ShipModel()
		{
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
			Modules = new ListenerProperty<ModuleModel[]>(value => modules = value, () => modules, OnModules);
		}

		#region Events
		void OnModules(ModuleModel[] value)
		{
			// TODO: Change certain summed values, etc... (power consumption total, etc)
		}
		#endregion
	}
}