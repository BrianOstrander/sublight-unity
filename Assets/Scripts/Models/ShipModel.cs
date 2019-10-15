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
		#endregion

		#region NonSerialized

		#endregion

		public ShipModel()
		{
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
		}

		#region Utility
		#endregion
	}
}