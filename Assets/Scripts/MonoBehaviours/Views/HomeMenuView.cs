using System;

namespace LunraGames.SpaceFarm.Views
{
	public class HomeMenuView : View, IHomeMenuView
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

	public interface IHomeMenuView : IView
	{
		Action StartClick { set;  }
	}
}