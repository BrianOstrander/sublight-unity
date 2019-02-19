using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridDeveloperPresenter : DeveloperFocusPresenter<ISystemScratchView, SystemFocusDetails>
	{
		protected override DeveloperViews DeveloperView { get { return DeveloperViews.Grid; } }

		public GridDeveloperPresenter(GameModel model) : base(model)
		{

		}

		protected override void OnUpdateEnabled()
		{
			View.Message = "Lol test";
		}

		#region Events

		#endregion
	}
}