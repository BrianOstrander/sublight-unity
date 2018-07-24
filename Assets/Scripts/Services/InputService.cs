using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LunraGames.SpaceFarm
{
	public interface IInputService
	{
		bool IsEnabled { get; }
		void SetEnabled(bool isEnabled);
	}

	public class InputService : IInputService
	{
		Heartbeat heartbeat;
		CallbackService callbacks;

		public bool IsEnabled { get; private set; }
		DateTime clickDownTime;

		/// <summary>
		/// The maximum distance that a user's finger can traverse before a click is ignored.
		/// </summary>
		const float MaximumClickDistance = 1000.0f;

		protected List<GameObject> highlighted = new List<GameObject>();

		List<GameObject> dragging = new List<GameObject>();

		Vector2 beginGesture;
		Vector2 beginGestureNormal;

		Vector2 lastGesture;

		bool startedDragging;

		public InputService(Heartbeat heartbeat, CallbackService callbacks)
		{
			if (heartbeat == null) throw new ArgumentNullException("heartbeat");
			if (callbacks == null) throw new ArgumentNullException("callbacks");

			this.heartbeat = heartbeat;
			this.callbacks = callbacks;
		}

		public void SetEnabled(bool isEnabled)
		{
			if (isEnabled == IsEnabled) return;
			IsEnabled = isEnabled;
			if (IsEnabled) OnEnabled();
			else OnDisabled();
		}

		protected virtual void OnEnabled()
		{
			heartbeat.Update += OnUpdate;
		}

		protected virtual void OnDisabled()
		{
			heartbeat.Update -= OnUpdate;
		}

		#region Events
		protected virtual void OnUpdate(float delta)
		{
			if (IsEscapeUp()) callbacks.Escape();

			var clickDown = IsClickDown();
			var clickHeldDown = IsClickHeldDown();
			var clickUp = IsClickUp();
			var anyInteraction = clickDown || clickHeldDown || clickUp;

			if (clickDown) clickDownTime = DateTime.Now;
			var clickDownDuration = DateTime.Now - clickDownTime;
			var clickClick = clickUp && clickDownDuration.TotalSeconds < GetClickDuration() && ((beginGesture - lastGesture).sqrMagnitude < MaximumClickDistance);
			var gesturingBegan = GetGestureBegan();
			var gesturingEnded = GetGestureEnded();

			var currentGestureNormal = GetGesture();
			var currentGesture = currentGestureNormal * GetGestureSensitivity();

			// TODO: Delete this?
			//var gestureDelta = GetGestureDelta(gesturingBegan, gesturingEnded, currentGesture, lastGesture);

			if (gesturingBegan)
			{
				beginGesture = currentGesture;
				beginGestureNormal = currentGestureNormal;
				callbacks.BeginGesture(new Gesture(currentGestureNormal, IsSecondaryClickInteraction()));
			}

			// TODO: Delete this?
			//var gestureNormalDelta = currentGestureNormal - beginGestureNormal;

			if (gesturingEnded) callbacks.EndGesture(new Gesture(beginGestureNormal, currentGestureNormal, false, IsSecondaryClickInteraction()));
			callbacks.CurrentGesture(new Gesture(beginGestureNormal, currentGestureNormal, IsGesturing(), IsSecondaryClickInteraction()));

			var gestureDeltaFromBegin = GetGestureDelta(gesturingBegan, gesturingEnded, beginGesture, lastGesture);

			lastGesture = currentGesture;

			var screenPos = GetScreenPosition();

			var pointerData = new PointerEventData(EventSystem.current);
			pointerData.position = screenPos;

			var raycasts = new List<RaycastResult>();
			if (EventSystem.current != null) EventSystem.current.RaycastAll(pointerData, raycasts);
			// TODO: Does this need to be a list?
			var stillHighlighted = new List<GameObject>();
			var wasTriggered = false;
			var expiredDrags = new List<GameObject>();

			// TODO: This probably doesn't need to be a foreach loop.
			foreach (var raycast in raycasts)
			{
				if (stillHighlighted.Count == 0)
				{
					stillHighlighted.Add(raycast.gameObject);

					if (!highlighted.Contains(raycast.gameObject)) ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
				}

				if (!anyInteraction) break;
				if (clickDown)
				{
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerDownHandler) != null;
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.beginDragHandler) != null;
					dragging.Add(raycast.gameObject);
				}
				if (clickUp)
				{
					if (clickClick)
					{
						wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerClickHandler) != null;
					}
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerUpHandler) != null;

					expiredDrags.Add(raycast.gameObject);
				}
				if (clickHeldDown && (startedDragging || IsDragging(gestureDeltaFromBegin)))
				{
					startedDragging = true;
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.dragHandler) != null;
				}
				break;
			}

			if (clickUp)
			{
				foreach (var drag in dragging)
				{
					ExecuteEvents.ExecuteHierarchy(drag, pointerData, ExecuteEvents.endDragHandler);
				}
				startedDragging = false;
			}

			foreach (var drag in expiredDrags) dragging.Remove(drag);

			foreach (var highlight in highlighted)
			{
				if (highlight != null && !stillHighlighted.Contains(highlight))
				{
					ExecuteEvents.ExecuteHierarchy(highlight, pointerData, ExecuteEvents.pointerExitHandler);
				}
			}

			if (highlighted.Count != stillHighlighted.Count)
			{
				var highlightState = Highlight.States.Unknown;
				if (stillHighlighted.Count == 0) highlightState = Highlight.States.End;
				else if (highlighted.Count == 0) highlightState = Highlight.States.Begin;
				else highlightState = Highlight.States.Change;
				callbacks.Highlight(new Highlight(highlightState));
			}

			highlighted = stillHighlighted;

			if (clickClick) callbacks.Click(new Click(beginGestureNormal, currentGestureNormal, !wasTriggered));

			// Camera
			var cameraPosition = GetCameraPosition();
			var cameraRotation = GetCameraRotation();
			callbacks.CameraOrientation(new CameraOrientation(cameraPosition, cameraRotation));

			var pointerRotation = GetPointerRotation();
			callbacks.PointerOrientation(new PointerOrientation(cameraPosition, pointerRotation, screenPos));
		}
		#endregion
		protected virtual bool IsEscapeUp() { return false; }
		/// <summary>
		/// Gets the maximum duration of the click, holding down the input longer will not be a click.
		/// </summary>
		/// <returns>The click duration.</returns>
		protected virtual float GetClickDuration() { return 0.7f; }
		protected virtual bool IsClickDown() { return false; }
		protected virtual bool IsClickHeldDown() { return false; }
		protected virtual bool IsClickUp() { return false; }
		protected virtual bool IsSecondaryClickInteraction() { return false; }
		protected virtual float GetGestureSensitivity() { return 1f; }
		protected virtual bool GetGestureBegan() { return false; }
		protected virtual bool IsGesturing() { return false; }
		protected virtual bool GetGestureEnded() { return false; }
		protected virtual Vector2 GetGesture() { return Vector2.zero; }
		protected virtual Vector2 GetGestureDelta(bool gestureBegan, bool gestureEnded, Vector2 gesture, Vector2 lastGesture) { return gestureBegan || gestureEnded ? Vector2.zero : gesture - lastGesture; }
		protected virtual Vector2 GetScreenPosition() { return Vector2.zero; }
		protected virtual Quaternion GetPointerRotation() { return Quaternion.identity; }
		protected virtual Vector3 GetCameraPosition() { return new Vector3(0f, 0f, 0f); }
		protected virtual Quaternion GetCameraRotation() { return Quaternion.identity; }
		protected virtual bool IsDragging(Vector2 gesture) { return 0.0001f < gesture.sqrMagnitude; }
	}
}