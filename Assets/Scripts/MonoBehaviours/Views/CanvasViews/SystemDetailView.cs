using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class SystemDetailView : CanvasView, ISystemDetailView
	{
		[SerializeField]
		TextMeshProUGUI nameLabel;
		[SerializeField]
		TextMeshProUGUI travelTimeLabel;

		public string Name { set { nameLabel.text = value ?? string.Empty; } }
		public int DayTravelTime { set { travelTimeLabel.text = value + " days"; } }

		public override void Reset()
		{
			base.Reset();

			Name = string.Empty;
			DayTravelTime = 0;
		}

		#region Events
		#endregion
	}

	public interface ISystemDetailView : ICanvasView
	{
		string Name { set; }
		int DayTravelTime { set; }
	}
}