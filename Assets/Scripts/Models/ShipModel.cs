using System;
using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ShipModel : Model
	{
		#region Serialized
		[JsonProperty] UniversePosition position;
		[JsonIgnore] public readonly ListenerProperty<UniversePosition> Position;

		// TODO: This should probably be wrapped into a struct with Position...
		[JsonProperty] int systemIndex;
		/// <summary>
		/// The index of the current system.
		/// </summary>
		/// <remarks>
		/// This should not be set manually outside of
		/// GameModel.SetCurrentSystem or GameService.
		/// </remarks>
		[JsonIgnore] public readonly ListenerProperty<int> SystemIndex;

		[JsonProperty] ShipStatistics statistics = ShipStatistics.Default;
		[JsonIgnore] public readonly ListenerProperty<ShipStatistics> Statistics;
		#endregion

		public ShipModel()
		{
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
			Statistics = new ListenerProperty<ShipStatistics>(value => statistics = value, () => statistics);
		}
	}
}