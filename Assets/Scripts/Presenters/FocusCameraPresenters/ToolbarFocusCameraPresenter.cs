using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ToolbarFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, ToolbarFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Toolbar; } }

		public ToolbarFocusCameraPresenter(Transform viewParent) : base(viewParent, "GenericFocusCameraView - Toolbar") {}

		#region Events
		protected override void OnShowInstant()
		{
			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloToolbar);
		}
		#endregion
	}
}