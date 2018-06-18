using UnityEngine;

namespace LunraGames.SpaceFarm.Models
{
	public class GameModel : Model
	{
		/// <summary>
		/// The gameplay canvas all UI elements are parented to.
		/// </summary>
		public readonly ModelProperty<Transform> GameplayCanvas = new ModelProperty<Transform>();
		/// <summary>
		/// The game seed.
		/// </summary>
		public readonly ModelProperty<int> Seed = new ModelProperty<int>();
		/// <summary>
		/// The day time.
		/// </summary>
		public readonly ModelProperty<DayTime> DayTime = new ModelProperty<DayTime>();
		/// <summary>
		/// The speed of the ship, in universe units per day, whether or not
		/// it's curently in motion.
		/// </summary>
		public readonly ModelProperty<float> Speed = new ModelProperty<float>();
		/// <summary>
		/// The game universe.
		/// </summary>
		public readonly ModelProperty<UniverseModel> Universe = new ModelProperty<UniverseModel>();
		/// <summary>
		/// The sector the camera is looking at.
		/// </summary>
		public readonly ModelProperty<UniversePosition> FocusedSector = new ModelProperty<UniversePosition>();
		/// <summary>
		/// The game ship.
		/// </summary>
		public readonly ModelProperty<ShipModel> Ship = new ModelProperty<ShipModel>();
		/// <summary>
		/// The speed at which the destruction expands, in universe units per
		/// day.
		/// </summary>
		public readonly ModelProperty<float> DestructionSpeed = new ModelProperty<float>();
		/// <summary>
		/// The total destruction radius, in universe units.
		/// </summary>
		public readonly ModelProperty<float> DestructionRadius = new ModelProperty<float>();
	}
}