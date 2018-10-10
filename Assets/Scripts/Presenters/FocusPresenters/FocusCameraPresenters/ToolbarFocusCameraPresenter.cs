using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ToolbarFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, ToolbarFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Toolbar; } }

		public ToolbarFocusCameraPresenter(Transform viewParent, float fieldOfView) : base(viewParent, "GenericFocusCameraView - Toolbar", fieldOfView) {}

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloToolbar);
		}
		#endregion
	}
}