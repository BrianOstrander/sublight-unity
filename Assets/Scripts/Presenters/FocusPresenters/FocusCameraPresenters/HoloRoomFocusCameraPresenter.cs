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

		bool lastTransformedByInput;
		float elapsedSinceInput;

		CameraTransformRequest transformOnAnimationBegin;
		CameraTransformRequest transformAnimation;
		float? animationRemaining;


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

			if (gameModel == null) return;

			if (animationRemaining.HasValue)
			{
				OnAnimation(delta);
				return;
			}

			var cameraTransform = gameModel.CameraTransform.Value;

			if (lastTransformedByInput)
			{
				if (Mathf.Approximately(0f, cameraTransform.PitchValue()) || View.PitchSettleThreshold < cameraTransform.PitchValue()) lastTransformedByInput = false;
				else
				{
					elapsedSinceInput = Mathf.Max(0f, elapsedSinceInput - delta);
					if (Mathf.Approximately(0f, elapsedSinceInput))
					{
						App.Callbacks.CameraTransformRequest(CameraTransformRequest.Settle(pitch: (-View.PitchSettleSpeed * delta)));

					}
				}
			}

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
				case CameraTransformRequest.Transforms.Settle: OnSettleTransform(transform); break;
				case CameraTransformRequest.Transforms.Animation: OnAnimationTransform(transform); break;
				default:
					Debug.LogError("Unrecognized transform: " + transform.Transform);
					break;
			}
		}

		void OnAnimationTransform(CameraTransformRequest transform)
		{
			switch (transform.State)
			{
				case CameraTransformRequest.States.Request:
					if (gameModel == null)
					{
						Debug.LogError("Cannot request a camera animation when there is no active game");
						return;
					}
					if (animationRemaining.HasValue)
					{
						Debug.LogError("Cannot request an animation while another is active");
						return;
					}
					lastTransformedByInput = false;
					transformOnAnimationBegin = gameModel.CameraTransform.Value;
					transformAnimation = transform;
					animationRemaining = transform.Duration;
					OnAnimation(0f);
					break;
			}
		}

		float? GetValue(float? endValue, float beginValue, float progress, AnimationCurve curve)
		{
			if (endValue.HasValue) return beginValue + ((endValue.Value - beginValue) * curve.Evaluate(progress));
			return null;
		}

		void OnAnimation(float delta)
		{
			animationRemaining = Mathf.Max(0f, animationRemaining.Value - delta);
			var progress = transformAnimation.IsInstant ? 1f : 1f - (animationRemaining.Value / transformAnimation.Duration);

			View.Set(
				GetValue(transformAnimation.Yaw, transformOnAnimationBegin.YawValue(), progress, View.YawAnimationCurve),
				GetValue(transformAnimation.Pitch, transformOnAnimationBegin.PitchValue(), progress, View.PitchAnimationCurve),
				GetValue(transformAnimation.Radius, transformOnAnimationBegin.RadiusValue(), progress, View.RadiusAnimationCurve)
			);
				
			var nextTransform = new CameraTransformRequest(
				Mathf.Approximately(0f, animationRemaining.Value) ? CameraTransformRequest.States.Complete : CameraTransformRequest.States.Active,
				CameraTransformRequest.Transforms.Animation,
				View.Yaw,
				View.Pitch,
				View.Radius
			);

			if (nextTransform.State == CameraTransformRequest.States.Complete) animationRemaining = null;
			var onDone = transformAnimation.Done;

			gameModel.CameraTransform.Value = nextTransform;
			App.Callbacks.CameraTransformRequest(nextTransform);

			if (nextTransform.State == CameraTransformRequest.States.Complete && onDone != null) onDone();
		}

		void OnInputTransform(CameraTransformRequest transform)
		{
			switch (transform.State)
			{
				case CameraTransformRequest.States.Request:
					lastTransformedByInput = true;
					elapsedSinceInput = View.DelayBeforePitchSettle;

					View.Input(transform.Yaw, transform.Pitch, transform.Radius);
					if (gameModel != null) gameModel.CameraTransform.Value = new CameraTransformRequest(
						CameraTransformRequest.States.Active,
						CameraTransformRequest.Transforms.Input,
						View.Yaw,
						View.Pitch,
						View.Radius
					);
					break;
				case CameraTransformRequest.States.Complete:
					if (gameModel != null) gameModel.CameraTransform.Value = new CameraTransformRequest(
						CameraTransformRequest.States.Complete,
						CameraTransformRequest.Transforms.Input,
						View.Yaw,
						View.Pitch,
						View.Radius
					);
					break;
			}
		}

		void OnSettleTransform(CameraTransformRequest transform)
		{
			View.Input(transform.Yaw, transform.Pitch, transform.Radius);
			if (gameModel != null) gameModel.CameraTransform.Value = new CameraTransformRequest(
				CameraTransformRequest.States.Complete,
				CameraTransformRequest.Transforms.Settle,
				View.Yaw,
				View.Pitch,
				View.Radius
			);
		}
		#endregion
	}
}