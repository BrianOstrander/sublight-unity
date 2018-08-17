using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class DetailSystemView : CanvasView, IDetailSystemView
	{
		[SerializeField]
		TextMeshProUGUI nameLabel;
		[SerializeField]
		TextMeshProUGUI travelTimeLabel;
		[SerializeField]
		TextMeshProUGUI rationsLabel;
		[SerializeField]
		TextMeshProUGUI fuelLabel;

		public string Name { set { nameLabel.text = value ?? string.Empty; } }
		public int DayTravelTime { set { travelTimeLabel.text = value + " days"; } }
		public string Rations { set { rationsLabel.text = value ?? string.Empty; } }
		public string Fuel { set { fuelLabel.text = value ?? string.Empty; } }
		public Color RationsColor { set { rationsLabel.color = value; } }
		public Color FuelColor { set { fuelLabel.color = value; } }

		public override void Reset()
		{
			base.Reset();

			Name = string.Empty;
			DayTravelTime = 0;
			Rations = string.Empty;
			Fuel = string.Empty;
			RationsColor = Color.white;
			FuelColor = Color.white;
		}

		#region Events
		#endregion
	}

	public interface IDetailSystemView : ICanvasView
	{
		string Name { set; }
		int DayTravelTime { set; }
		string Rations { set; }
		Color RationsColor { set; }
		string Fuel { set; }
		Color FuelColor { set; }
	}
}