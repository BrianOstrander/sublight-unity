using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class CameraSystemPresenter : Presenter<ICameraSystemView>
	{
		CameraSystemRequest lastMoveRequest;
		Gesture lastGesture;

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
			if (lastMoveRequest.State == CameraSystemRequest.States.Active) OnMoveRequest();
			else if (!App.Callbacks.LastObscureCameraRequest.IsObscured) OnDragMove();
		}

		void OnMoveRequest()
		{
			var progress = lastMoveRequest.GetProgress(DateTime.Now);
			var distance = UniversePosition.Distance(lastMoveRequest.Destination, lastMoveRequest.Origin);

			var normal = (lastMoveRequest.Destination - lastMoveRequest.Origin).Normalized;

			var doneFocusing = Mathf.Approximately(1f, progress);

			var newPos = doneFocusing ? lastMoveRequest.Destination : lastMoveRequest.Origin + new UniversePosition(progress * distance * normal);

			var request = new CameraSystemRequest(
				doneFocusing ? CameraSystemRequest.States.Complete : CameraSystemRequest.States.Active,
				newPos,
				lastMoveRequest.Origin,
				lastMoveRequest.Destination,
				lastMoveRequest.StartTime,
				lastMoveRequest.EndTime,
				progress,
				lastMoveRequest.Instant
			);
			App.Callbacks.CameraSystemRequest(request);
		}

		void OnDragMove()
		{
			// Drag logic

			if (App.Callbacks.LastObscureCameraRequest.IsObscured) return;

			var gesture = App.Callbacks.LastGesture;

			if ((gesture.IsSecondary) || (lastGesture.IsGesturing == gesture.IsGesturing && !lastGesture.IsGesturing))
			{
				lastGesture = gesture;
				return;
			}

			if (lastGesture.IsGesturing && !gesture.IsGesturing)
			{
				var newPos = View.DragRoot.position;
				View.Root.position = newPos;
				View.DragRoot.localPosition = Vector3.zero;

				lastGesture = gesture;
				return;
			}

			var planeForward = View.DragForward.forward.FlattenY() * gesture.Delta.y * -1f;
			var planeRight = View.DragForward.right.FlattenY() * gesture.Delta.x * -1f;
			var planeCombined = planeForward + planeRight;

			View.DragRoot.localPosition = planeCombined * View.DragScalar;

			lastGesture = gesture;
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
					if (lastMoveRequest.State == CameraSystemRequest.States.Active)
					{
						Debug.LogWarning("Processing a new SystemCameraRequest before the current one is finished is not supported.");
						return;
					}
					if (request.Instant)
					{
						App.Callbacks.CameraSystemRequest(lastMoveRequest = request.Duplicate(CameraSystemRequest.States.Complete));
						return;
					}
					App.Callbacks.CameraSystemRequest(lastMoveRequest = request.Duplicate(CameraSystemRequest.States.Active));
					break;
				case CameraSystemRequest.States.Active:
					OnApplyRequest(lastMoveRequest = request);
					break;
				case CameraSystemRequest.States.Complete:
					OnApplyRequest(lastMoveRequest = request);
					break;
			}
		}

		void OnApplyRequest(CameraSystemRequest request)
		{
			View.UnityPosition = UniversePosition.ToUnity(request.Position);
		}
  		#endregion
	}
}