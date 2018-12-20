using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridTransitLockoutPresenter : FocusPresenter<IGridTransitLockoutView, SystemFocusDetails>
	{
		GameModel model;

		public GridTransitLockoutPresenter(
			GameModel model
		)
		{
			this.model = model;

			model.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			model.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{

		}

		#region Events
		void OnTransitState(TransitState transitState)
		{
			switch (transitState.State)
			{
				case TransitState.States.Request:
					Debug.Log("request recieved");
					break;
			}
		}
		#endregion
	}
}