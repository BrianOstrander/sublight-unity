using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridPresenter : FocusPresenter<IGridView, SystemFocusDetails>
	{
		GameModel model;

		public GridPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			//App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			//App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.HoloColor = App.Callbacks.LastHoloColorRequest.Color;

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}

		#region
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		//void OnTransitionFocusRequest(TransitionFocusRequest request)
		//{
		//	switch (request.)
		//}
		#endregion
	}
}