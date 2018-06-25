using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class CameraSystemPresenter : Presenter<ICameraSystemView>
	{
		SystemCameraRequest lastRequest;

		public CameraSystemPresenter()
		{
			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.ObscureCameraRequest += OnObscureCameraRequest;
			App.Callbacks.SystemCameraRequest += OnSystemCameraRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.ObscureCameraRequest -= OnObscureCameraRequest;
			App.Callbacks.SystemCameraRequest -= OnSystemCameraRequest;
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
			if (lastRequest.State != SystemCameraRequest.States.Active) return;

			var progress = lastRequest.GetProgress(DateTime.Now);
			var distance = UniversePosition.Distance(lastRequest.Destination, lastRequest.Origin);

			var normal = (lastRequest.Destination - lastRequest.Origin).Normalized;

			var doneFocusing = Mathf.Approximately(1f, progress);

			var newPos = doneFocusing ? lastRequest.Destination : lastRequest.Origin + new UniversePosition(progress * distance * normal);

			var request = new SystemCameraRequest(
				doneFocusing ? SystemCameraRequest.States.Complete : SystemCameraRequest.States.Active,
				newPos,
				lastRequest.Origin,
				lastRequest.Destination,
				lastRequest.StartTime,
				lastRequest.EndTime,
				progress,
				lastRequest.Instant
			);
			App.Callbacks.SystemCameraRequest(request);
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

		void OnSystemCameraRequest(SystemCameraRequest request)
		{
			switch(request.State)
			{
				case SystemCameraRequest.States.Request:
					if (lastRequest.State == SystemCameraRequest.States.Active)
					{
						Debug.LogWarning("Processing a new SystemCameraRequest before the current one is finished is not supported.");
						return;
					}
					if (request.Instant)
					{
						App.Callbacks.SystemCameraRequest(lastRequest = request.Duplicate(SystemCameraRequest.States.Complete));
						return;
					}
					App.Callbacks.SystemCameraRequest(lastRequest = request.Duplicate(SystemCameraRequest.States.Active));
					break;
				case SystemCameraRequest.States.Active:
					OnApplyRequest(lastRequest = request);
					break;
				case SystemCameraRequest.States.Complete:
					OnApplyRequest(lastRequest = request);
					break;
			}
		}

		void OnApplyRequest(SystemCameraRequest request)
		{
			View.LookingAt = UniversePosition.ToUnity(request.Position);
		}
  		#endregion
	}
}