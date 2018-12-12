using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridTimePresenter : FocusPresenter<IGridTimeView, SystemFocusDetails>
	{
		GameModel model;

		public GridTimePresenter(GameModel model)
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