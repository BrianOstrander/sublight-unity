using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridTimePresenter : FocusPresenter<IGridTimeView, SystemFocusDetails>
	{
		GameModel model;
		bool isDelta;

		public GridTimePresenter(GameModel model, bool isDelta)
		{
			this.model = model;
			model.DayTime.Changed += OnDayTime;

		}

		protected override void OnUnBind()
		{

		}

		protected override void OnUpdateEnabled()
		{

		}

		#region Events
		void OnDayTime(DayTime dayTime)
		{

		}
		#endregion
	}
}