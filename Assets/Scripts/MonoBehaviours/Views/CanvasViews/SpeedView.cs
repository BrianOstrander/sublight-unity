using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class SpeedView : CanvasView, ISpeedView
	{
		[SerializeField]
		TextMeshProUGUI dayTimeLabel;

		public DayTime Current { set { dayTimeLabel.text = value.Day + ":" + value.Time.ToString("F0"); } }
		public Action<float> Click { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Current = DayTime.Zero;
			Click = ActionExtensions.GetEmpty<float>();
		}

		#region Events
		public void OnClick(float scalar) { Click(scalar); }
		#endregion
	}

	public interface ISpeedView : ICanvasView
	{
		DayTime Current { set; }
		Action<float> Click { set; }
	}
}