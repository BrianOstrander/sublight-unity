using UnityEngine;

namespace LunraGames.SpaceFarm.Models
{
	public class GameModel : Model
	{
		public readonly ModelProperty<Transform> GameplayCanvas = new ModelProperty<Transform>();

		public readonly ModelProperty<int> Seed = new ModelProperty<int>();
		public readonly ModelProperty<DayTime> DayTime = new ModelProperty<DayTime>();
		public readonly ModelProperty<float> Speed = new ModelProperty<float>();

	}
}