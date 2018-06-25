using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class SpeedView : CanvasView, ISpeedView
	{
		[SerializeField]
		TextMeshProUGUI dayTimeLabel;
		// TODO: Make these button styles...
		[SerializeField]
		Color UnselectedColor;
		[SerializeField]
		Color SelectedColor;
		[SerializeField]
		SpeedButtonLeaf[] speedButtons;

		public DayTime Current { set { dayTimeLabel.text = value.ToDayTimeString(); } }
		public Action<int> Click { set; private get; }
		public int SelectedSpeed
		{
			set
			{
				foreach (var button in speedButtons)
				{
					button.Background.color = value == button.Index ? SelectedColor : UnselectedColor;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			Current = DayTime.Zero;
			SelectedSpeed = -1;
			foreach (var button in speedButtons)
			{
				button.Button.OnClick.RemoveAllListeners();
				button.Button.OnClick.AddListener(new UnityAction(() => OnClick(button.Index)));
			}
			Click = ActionExtensions.GetEmpty<int>();
		}

		#region Events
		public void OnClick(int index) { Click(index); }
		#endregion
	}

	public interface ISpeedView : ICanvasView
	{
		DayTime Current { set; }
		Action<int> Click { set; }
		int SelectedSpeed { set; }
	}
}