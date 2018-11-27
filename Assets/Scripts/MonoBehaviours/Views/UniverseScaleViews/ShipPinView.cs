using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class ShipPinView : UniverseScaleView, IShipPinView
	{
		[SerializeField]
		float unityScale;

		public float UnityScale { get { return unityScale; } }

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;

			}
		}
	}

	public interface IShipPinView : IUniverseScaleView
	{
		float UnityScale { get; }
	}
}