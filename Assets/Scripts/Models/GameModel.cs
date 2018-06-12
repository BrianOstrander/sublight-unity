using UnityEngine;

namespace LunraGames.SpaceFarm.Models
{
	public class GameModel : Model
	{
		public readonly ModelProperty<Transform> GameplayCanvas = new ModelProperty<Transform>();

		public readonly ModelProperty<int> Seed = new ModelProperty<int>();
		public readonly ModelProperty<DayTime> DayTime = new ModelProperty<DayTime>();
		public readonly ModelProperty<float> Speed = new ModelProperty<float>();
		public readonly ModelProperty<UniverseModel> Universe = new ModelProperty<UniverseModel>();
		public readonly ModelProperty<UniversePosition> FocusedSector = new ModelProperty<UniversePosition>();
		public readonly ModelProperty<ShipModel> Ship = new ModelProperty<ShipModel>(); 
	}
}