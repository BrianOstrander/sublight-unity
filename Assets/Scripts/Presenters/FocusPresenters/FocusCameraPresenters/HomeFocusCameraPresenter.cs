using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class HomeFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, HomeFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Home; } }

		public HomeFocusCameraPresenter(Transform viewParent, float fieldOfView) : base(viewParent, "GenericFocusCameraView - Home", fieldOfView) { }

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloHome);
		}
		#endregion
	}
}