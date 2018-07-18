using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class CameraSystemPresenter : Presenter<ICameraSystemView>
	{
		GameModel game;

		CameraSystemRequest lastMoveRequest;

		Gesture lastMoveGesture;
		Gesture lastRotateGesture;

		UniversePosition lastUniversePosition;

		public CameraSystemPresenter(GameModel game)
		{
			this.game = game;

			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.FocusRequest += OnFocus;
			App.Callbacks.UniversePositionRequest += OnUniversePositionRequest;
			App.Callbacks.ObscureCameraRequest += OnObscureCameraRequest;
			App.Callbacks.CameraSystemRequest += OnSystemCameraRequest;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.FocusRequest -= OnFocus;
			App.Callbacks.UniversePositionRequest -= OnUniversePositionRequest;
			App.Callbacks.ObscureCameraRequest -= OnObscureCameraRequest;
			App.Callbacks.CameraSystemRequest -= OnSystemCameraRequest;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();

			App.Callbacks.VoidRenderTexture(new VoidRenderTexture(View.VoidTexture));

			ShowView(instant: true);
		}

		#region Events
		void OnUpdate(float delta)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			if (lastMoveRequest.State == CameraSystemRequest.States.Active) OnMoveRequest();
			else if (!App.Callbacks.LastObscureCameraRequest.IsObscured) OnDrag();
		}

		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Systems:
					if (focus.State == FocusRequest.States.Active)
					{
						var systemFocus = focus as SystemsFocusRequest;
						Show();
						App.Callbacks.CameraSystemRequest(CameraSystemRequest.RequestInstant(systemFocus.CameraFocus));
					}
					break;
				default:
					if (focus.State == FocusRequest.States.Active)
					{
						if (View.TransitionState == TransitionStates.Shown) CloseView();
					}
					break;
			}
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

		void OnDrag()
		{
			//switch(App.Callbacks.LastHighlight.State)
			//{
			//	case Highlight.States.Begin:
			//	case Highlight.States.Change:
			//		return;
			//}
			OnDragMove(App.Callbacks.LastGesture);
			OnDragRotate(App.Callbacks.LastGesture);
		}

		void OnDragMove(Gesture gesture)
		{
			if ((gesture.IsSecondary) || (lastMoveGesture.IsGesturing == gesture.IsGesturing && !lastMoveGesture.IsGesturing))
			{
				lastMoveGesture = gesture;
				return;
			}

			if (lastMoveGesture.IsGesturing && !gesture.IsGesturing)
			{
				// We're ending.
				var newPos = View.DragRoot.position;
				View.Root.position = newPos;
				View.DragRoot.localPosition = Vector3.zero;
				lastMoveGesture = gesture;

				var endMovePosition = UniversePosition.ToUniverse(View.UnityPosition);

				if (!game.FocusedSector.Value.SectorEquals(endMovePosition))
				{
					// Camera is now focused in a new sector.
					lastUniversePosition = endMovePosition;
					game.FocusedSector.Value = endMovePosition.SystemZero; 
				}

				return;
			}

			var planeForward = View.DragForward.forward.FlattenY() * gesture.Delta.y * -1f;
			var planeRight = View.DragForward.right.FlattenY() * gesture.Delta.x * -1f;
			var planeCombined = planeForward + planeRight;

			View.DragRoot.localPosition = planeCombined * View.DragMoveScalar;

			lastMoveGesture = gesture;
		}

		void OnDragRotate(Gesture gesture)
		{
			var camera = App.Callbacks.LastCameraOrientation;

			if ((!gesture.IsSecondary) || (lastRotateGesture.IsGesturing == gesture.IsGesturing && !lastRotateGesture.IsGesturing))
			{
				lastRotateGesture = gesture;
				return;
			}

			if (!lastRotateGesture.IsGesturing && gesture.IsGesturing)
			{
				// We're starting.
				var plane = new Plane(Vector3.up, View.Root.position);
				var dist = 0f;
				plane.Raycast(new Ray(camera.Position, camera.Forward), out dist);
				View.DragAxis = camera.Position + (camera.Forward * dist);
				lastRotateGesture = gesture;
			}
			else if (lastRotateGesture.IsGesturing && !gesture.IsGesturing)
			{
				// We're ending.
				var newPos = View.DragRoot.position;
				View.Root.position = newPos;
				View.DragRoot.localPosition = Vector3.zero;

				lastRotateGesture = gesture;
				return;
			}

			View.DragRoot.RotateAround(View.DragAxis, Vector3.up, (gesture.Delta.x - lastRotateGesture.Delta.x) * View.DragRotateScalar);

			lastRotateGesture = gesture;
		}

		void OnUniversePositionRequest(UniversePositionRequest request)
		{
			switch(request.State)
			{
				case UniversePositionRequest.States.Complete:
					View.UnityPosition = UniversePosition.ToUnity(lastUniversePosition);
					break;
			}
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