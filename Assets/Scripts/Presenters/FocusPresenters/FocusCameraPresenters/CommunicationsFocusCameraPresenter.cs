using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CommunicationsFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, CommunicationsFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Communications; } }

		public CommunicationsFocusCameraPresenter(Transform viewParent, float fieldOfView) : base(viewParent, "GenericFocusCameraView - Communications", fieldOfView) {}

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloCommunications);
		}
		#endregion
	}
}