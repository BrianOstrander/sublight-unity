using LunraGames.SubLight.Views;

using UnityEngine;

namespace LunraGames.SubLight.Presenters
{
	public class HoloLipPresenter : Presenter<IHoloLipView>
	{
		Transform parent;

		public HoloLipPresenter(Transform parent, string layer)
		{
			this.parent = parent;
			View.SetLayer(layer);
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			ShowView(parent);
		}
		#region Events
		#endregion
	}
}