namespace LunraGames.SpaceFarm.Models
{
	public class ShipModel : Model
	{
		public readonly ModelProperty<UniversePosition> Position = new ModelProperty<UniversePosition>();
		public readonly ModelProperty<TravelRadius> TravelRadius = new ModelProperty<TravelRadius>();
	}
}