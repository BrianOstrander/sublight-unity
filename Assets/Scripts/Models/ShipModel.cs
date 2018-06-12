namespace LunraGames.SpaceFarm.Models
{
	public class ShipModel : Model
	{
		public readonly ModelProperty<SystemModel> LastSystem = new ModelProperty<SystemModel>();
		public readonly ModelProperty<SystemModel> NextSystem = new ModelProperty<SystemModel>();
		public readonly ModelProperty<SystemModel> CurrentSystem = new ModelProperty<SystemModel>();
		public readonly ModelProperty<UniversePosition> Position = new ModelProperty<UniversePosition>();

		/// <summary>
		/// Basically the speed of the ship, expressed in universe units per year.
		/// </summary>
		public readonly ModelProperty<float> Speed = new ModelProperty<float>();
		/// <summary>
		/// The years worth of rations on board.
		/// </summary>
		public readonly ModelProperty<float> Rations = new ModelProperty<float>();
		/// <summary>
		/// The travel radius of this ship, expressed as a ratio of speed and rations.
		/// </summary>
		public readonly ModelProperty<TravelRadius> TravelRadius = new ModelProperty<TravelRadius>();
	}
}