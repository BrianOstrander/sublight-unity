using System;

namespace LunraGames.SpaceFarm.Views
{
	public class HomeMenuView : CanvasView, IHomeMenuView
	{
		public Action StartClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			StartClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnStartClick() { StartClick(); }
		#endregion
	}

	public interface IHomeMenuView : ICanvasView
	{
		Action StartClick { set;  }
	}
}