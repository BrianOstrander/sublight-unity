using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class CameraSystemPresenter : Presenter<ICameraSystemView>
	{
		CameraSystemRequest lastRequest;

		public CameraSystemPresenter()
		{
			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.ObscureCameraRequest += OnObscureCameraRequest;
			App.Callbacks.CameraSystemRequest += OnSystemCameraRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.ObscureCameraRequest -= OnObscureCameraRequest;
			App.Callbacks.CameraSystemRequest -= OnSystemCameraRequest;
		}

		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();

			App.Callbacks.VoidRenderTexture(new VoidRenderTexture(View.VoidTexture));

			View.Shown += done;

			ShowView(instant: true);
		}

		#region Events
		void OnUpdate(float delta)
		{
			if (lastRequest.State != CameraSystemRequest.States.Active) return;

			var progress = lastRequest.GetProgress(DateTime.Now);
			var distance = UniversePosition.Distance(lastRequest.Destination, lastRequest.Origin);

			var normal = (lastRequest.Destination - lastRequest.Origin).Normalized;

			var doneFocusing = Mathf.Approximately(1f, progress);

			var newPos = doneFocusing ? lastRequest.Destination : lastRequest.Origin + new UniversePosition(progress * distance * normal);

			var request = new CameraSystemRequest(
				doneFocusing ? CameraSystemRequest.States.Complete : CameraSystemRequest.States.Active,
				newPos,
				lastRequest.Origin,
				lastRequest.Destination,
				lastRequest.StartTime,
				lastRequest.EndTime,
				progress,
				lastRequest.Instant
			);
			App.Callbacks.CameraSystemRequest(request);
		}

		void OnObscureCameraRequest(ObscureCameraRequest request)
		{
			switch(request.State)
			{
				case ObscureCameraRequest.States.Request:
					View.Raycasting = !request.IsObscured;
					App.Callbacks.ObscureCameraRequest(request.Duplicate(ObscureCameraRequest.States.Complete));
					break;
			}
		}

		void OnSystemCameraRequest(CameraSystemRequest request)
		{
			switch(request.State)
			{
				case CameraSystemRequest.States.Request:
					if (lastRequest.State == CameraSystemRequest.States.Active)
					{
						Debug.LogWarning("Processing a new SystemCameraRequest before the current one is finished is not supported.");
						return;
					}
					if (request.Instant)
					{
						App.Callbacks.CameraSystemRequest(lastRequest = request.Duplicate(CameraSystemRequest.States.Complete));
						return;
					}
					App.Callbacks.CameraSystemRequest(lastRequest = request.Duplicate(CameraSystemRequest.States.Active));
					break;
				case CameraSystemRequest.States.Active:
					OnApplyRequest(lastRequest = request);
					break;
				case CameraSystemRequest.States.Complete:
					OnApplyRequest(lastRequest = request);
					break;
			}
		}

		void OnApplyRequest(CameraSystemRequest request)
		{
			View.LookingAt = UniversePosition.ToUnity(request.Position);
		}
  		#endregion
	}
}