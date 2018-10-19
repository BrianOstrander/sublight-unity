using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GenericFocusCameraPresenter<D> : FocusCameraPresenter<IGenericFocusCameraView, D>
		where D : SetFocusDetails<D>, new()
	{
		public GenericFocusCameraPresenter(Transform viewParent, float fieldOfView) : base(viewParent, fieldOfView: fieldOfView)
		{
			View.InstanceName = "GenericFocusCameraView - "+FocusLayer;
		}

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.Get(FocusLayer));
		}
		#endregion
	}
}