using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class EndDistanceView : CanvasView, IEndDistanceView
	{
		[SerializeField]
		TextMeshProUGUI distanceLabel;

		public float Distance { set { distanceLabel.text = value.ToString("F1") + " Light Years"; } }

		public override void Reset()
		{
			base.Reset();

			Distance = 0f;
		}
	}

	public interface IEndDistanceView : ICanvasView
	{
		float Distance { set; }
	}
}