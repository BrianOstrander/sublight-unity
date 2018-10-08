﻿using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class SystemFocusCameraPresenter : FocusCameraPresenter<IGenericFocusCameraView, SystemFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.System; } }

		public SystemFocusCameraPresenter(Transform viewParent) : base(viewParent, "GenericFocusCameraView - System") {}

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.CullingMask = LayerMask.GetMask(LayerConstants.HoloSystem);
		}
		#endregion
	}
}