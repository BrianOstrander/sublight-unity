using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridSystemPresenter : FocusPresenter<IGridSystemView, SystemFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.System; } }

		#region Events
		#endregion
	}
}