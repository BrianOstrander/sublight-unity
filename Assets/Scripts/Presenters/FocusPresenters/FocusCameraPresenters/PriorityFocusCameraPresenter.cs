using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class PriorityFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, PriorityFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Priority; } }

		public PriorityFocusCameraPresenter(Transform viewParent, float fieldOfView) : base(viewParent, "GenericFocusCameraView - Priority", fieldOfView) { }

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloPriority);
		}
		#endregion
	}
}