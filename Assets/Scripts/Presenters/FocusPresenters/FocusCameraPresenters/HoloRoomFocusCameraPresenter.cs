using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class HoloRoomFocusCameraPresenter : FocusCameraPresenter<IHoloRoomFocusCameraView, RoomFocusDetails>
	{
		protected override bool IsGatherable { get { return false; } }

		public Transform GantryAnchor { get { return View.GantryAnchor; } }
		public float FieldOfView { get { return View.FieldOfView; } }

		CameraMaskRequest lastMask;

		GameModel gameModel;

		public HoloRoomFocusCameraPresenter() : base(null)
		{
			App.Callbacks.CameraMaskRequest += OnCameraMaskRequest;
			App.Callbacks.CameraTransformRequest += OnCameraTransformRequest;
			App.Callbacks.StateChange += OnStateChange;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.CameraMaskRequest -= OnCameraMaskRequest;
			App.Callbacks.CameraTransformRequest -= OnCameraTransformRequest;
			App.Callbacks.StateChange -= OnStateChange;
			App.Heartbeat.Update -= OnUpdate;
		}


		#region Events
		void OnStateChange(StateChange stateChange)
		{
			switch (stateChange.State)
			{
				case StateMachine.States.Game: break;
				default: return;
			}

			switch (stateChange.Event)
			{
				case StateMachine.Events.Idle: gameModel = stateChange.GetPayload<GamePayload>().Game; break;
				case StateMachine.Events.End: gameModel = null; break;
			}
		}

		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.Set(0f, 0f, 0f);
		}

		void OnCameraMaskRequest(CameraMaskRequest request)
		{
			switch (request.State)
			{
				case CameraMaskRequest.States.Request:
					if (lastMask.State != CameraMaskRequest.States.Complete && lastMask.State != CameraMaskRequest.States.Unknown)
					{
						Debug.LogError("Requesting a camera mask in the middle of an existing request, may cause unpredictable behaviour.");
					}
					lastMask = request.Duplicate(CameraMaskRequest.States.Active);
					OnUpdateMaskRequest(0f);
					break;
			}
		}

		void OnUpdate(float delta)
		{
			OnUpdateMaskRequest(delta);
		}

		void OnUpdateMaskRequest(float delta)
		{
			if (lastMask.State != CameraMaskRequest.States.Active) return;

			if (lastMask.IsInstant)
			{
				lastMask = lastMask.Duplicate(CameraMaskRequest.States.Active, lastMask.Duration);
				App.Callbacks.CameraMaskRequest(lastMask);
			}
			else
			{
				lastMask = lastMask.Duplicate(CameraMaskRequest.States.Active, Mathf.Min(lastMask.Duration, lastMask.Elapsed + delta));
				App.Callbacks.CameraMaskRequest(lastMask);
			}

			if (View.Visible) View.Mask(lastMask.Progress, lastMask.Revealing);

			if (Mathf.Approximately(lastMask.Duration, lastMask.Elapsed))
			{
				lastMask = lastMask.Duplicate(CameraMaskRequest.States.Complete);
				var lastDone = lastMask.Done;
				App.Callbacks.CameraMaskRequest(lastMask);
				lastDone();
			}
		}

		void OnCameraTransformRequest(CameraTransformRequest transform)
		{
			switch (transform.Transform)
			{
				case CameraTransformRequest.Transforms.Input: OnInputTransform(transform); break;
				default:
					Debug.LogError("Unrecognized transform: " + transform.Transform);
					break;
			}
		}

		void OnInputTransform(CameraTransformRequest transform)
		{
			View.Input(transform.Yaw, transform.Pitch, transform.Radius);
			if (gameModel != null) gameModel.CameraTransform.Value = new CameraTransformRequest(
				CameraTransformRequest.States.Complete,
				CameraTransformRequest.Transforms.Input,
				View.Yaw,
				View.Pitch,
				View.Radius
			);
		}
		#endregion
	}
}