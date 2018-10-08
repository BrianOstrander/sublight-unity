using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CommunicationsFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, CommunicationsFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Communications; } }

		public CommunicationsFocusCameraPresenter(Transform viewParent) : base(viewParent, "GenericFocusCameraView - Communications") {}

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloCommunications);
		}
		#endregion
	}
}