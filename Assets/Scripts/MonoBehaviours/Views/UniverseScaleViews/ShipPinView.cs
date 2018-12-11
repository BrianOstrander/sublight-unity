using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class ShipPinView : UniverseScaleView, IShipPinView
	{
		[SerializeField]
		float unityScale;

		public float UnityScale { get { return unityScale; } }
	}

	public interface IShipPinView : IUniverseScaleView
	{
		float UnityScale { get; }
	}
}