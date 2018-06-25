using UnityEngine;

namespace LunraGames.SpaceFarm {

	public interface IMonoBehaviour {

		Transform transform { get; }
		GameObject gameObject { get; }
	}
}