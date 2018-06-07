using System;
using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class CameraPresenter : Presenter<ICameraView>
	{
		CameraModel model;

		public CameraPresenter(CameraModel model)
		{
			SetView (App.V.Get<ICameraView> ());
			this.model = model;
			App.Callbacks.CameraOrientation += OnOrientation;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.CameraOrientation -= OnOrientation;
		}

		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.Shown += done;
			ShowView(model.Root, true);
		}

		#region Events
		void OnOrientation(CameraOrientation orientation)
		{
			// Unity rotates the camera for us on the actual device.
			View.Position = orientation.Position;
			if (Application.isEditor)
			{
				View.Rotation = orientation.Rotation;
			}
		}
  		#endregion

	}
}