using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridVelocityPresenter : FocusPresenter<IGridVelocityView, SystemFocusDetails>
	{
		GameModel model;

		public GridVelocityPresenter(
			GameModel model
		)
		{
			this.model = model;
		}

		protected override void OnUnBind()
		{

		}

		protected override void OnUpdateEnabled()
		{

		}

		#region Events

		#endregion
	}
}