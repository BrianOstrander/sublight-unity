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
		public bool IsEnabled { get; private set; }
		Quaternion offset = Quaternion.identity;
		DateTime primaryDownTime;

		/// <summary>
		/// The maximum distance that a user's finger can traverse before a click is ignored.
		/// </summary>
		const float MaximumClickDistance = 1000.0f;

		protected List<GameObject> highlighted = new List<GameObject>();

		List<GameObject> dragging = new List<GameObject>();

		Vector2 beginGesture;
		Vector2 beginGestureNormal;

		Vector2 lastGesture;

		public void SetEnabled(bool isEnabled)
		{
			if (isEnabled == IsEnabled) return;
			IsEnabled = isEnabled;
			if (IsEnabled) OnEnabled();
			else OnDisabled();
		}

		protected virtual void OnEnabled()
		{
			App.Heartbeat.Update += OnUpdate;
		}

		protected virtual void OnDisabled()
		{
			App.Heartbeat.Update -= OnUpdate;
		}
		#region Events
		protected virtual void OnUpdate(float delta)
		{
			var primaryDown = IsPrimaryDown();
			var primaryHeldDown = IsPrimaryHeldDown();
			var primaryUp = IsPrimaryUp();
			var anyInteraction = primaryDown || primaryHeldDown || primaryUp;

			if (primaryDown) primaryDownTime = DateTime.Now;
			var primaryDownDuration = DateTime.Now - primaryDownTime;
			var primaryClick = primaryUp && primaryDownDuration.TotalSeconds < GetPrimaryClickDuration() && ((beginGesture - lastGesture).sqrMagnitude < MaximumClickDistance);
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
				App.Callbacks.BeginGesture(new Gesture(currentGestureNormal));
			}

			// TODO: Delete this?
			//var gestureNormalDelta = currentGestureNormal - beginGestureNormal;

			if (gesturingEnded) App.Callbacks.EndGesture(new Gesture(beginGestureNormal, currentGestureNormal, false));
			App.Callbacks.CurrentGesture(new Gesture(beginGestureNormal, currentGestureNormal, IsGesturing()));

			lastGesture = currentGesture;

			var screenPos = GetScreenPosition();
			var rawRotation = GetPointerRawRotation();

			var rotation = GetPointerRotation(rawRotation, offset);

			var pointerData = new PointerEventData(EventSystem.current);
			pointerData.position = screenPos;

			var raycasts = new List<RaycastResult>();
			if (EventSystem.current != null) EventSystem.current.RaycastAll(pointerData, raycasts);
			var stillHighlighted = new List<GameObject>();
			var wasTriggered = false;
			var wasClicked = false;
			var expiredDrags = new List<GameObject>();

			foreach (var raycast in raycasts)
			{
				stillHighlighted.Add(raycast.gameObject);

				if (!highlighted.Contains(raycast.gameObject)) ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);

				if (!anyInteraction) continue;
				if (primaryDown)
				{
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerDownHandler) != null;
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.beginDragHandler) != null;
					dragging.Add(raycast.gameObject);
				}
				if (primaryUp)
				{
					if (SendClickEvents() && primaryClick && !wasClicked)
					{
						wasClicked |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerClickHandler) != null;
						wasTriggered |= wasClicked;
					}
					wasTriggered |= ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerUpHandler) != null;

					expiredDrags.Add(raycast.gameObject);
				}
			}

			if (primaryUp)
			{
				foreach (var drag in dragging)
				{
					ExecuteEvents.ExecuteHierarchy(drag, pointerData, ExecuteEvents.endDragHandler);
				}
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
				App.Callbacks.Highlight(new Highlight(highlightState));
			}

			highlighted = stillHighlighted;

			if (primaryClick) App.Callbacks.Click(new Click(beginGestureNormal, currentGestureNormal, !wasTriggered));


			// Camera
			var cameraPosition = GetCameraPosition();
			var cameraRotation = GetCameraRotation();
			App.Callbacks.CameraOrientation(new CameraOrientation(cameraPosition, cameraRotation));

			App.Callbacks.PointerOrientation(new PointerOrientation(cameraPosition, rawRotation, rotation, screenPos));
		}
		#endregion

		/// <summary>
		/// Gets the maximum duration of the primary click, holding down the primary input longer will not be a click.
		/// </summary>
		/// <returns>The primary click duration.</returns>
		protected virtual float GetPrimaryClickDuration() { return 0.7f; }
		protected virtual bool IsPrimaryDown() { return false; }
		protected virtual bool IsPrimaryHeldDown() { return false; }
		protected virtual bool IsPrimaryUp() { return false; }
		/// <summary>
		/// Determines if click events should be executed by this InputService.
		/// </summary>
		/// <remarks>
		/// For some platforms, like desktop, we can let the base input module do its job and set this to false.
		/// </remarks>
		/// <returns><c>true</c>, if click events was sent, <c>false</c> otherwise.</returns>
		protected virtual bool SendClickEvents() { return true; }
		protected virtual float GetGestureSensitivity() { return 1f; }
		protected virtual bool GetGestureBegan() { return false; }
		protected virtual bool IsGesturing() { return false; }
		protected virtual bool GetGestureEnded() { return false; }
		protected virtual Vector2 GetGesture() { return Vector2.zero; }
		protected virtual Vector2 GetGestureDelta(bool gestureBegan, bool gestureEnded, Vector2 gesture, Vector2 lastGesture) { return gestureBegan || gestureEnded ? Vector2.zero : gesture - lastGesture; }
		protected virtual Vector2 GetScreenPosition() { return Vector2.zero; }
		protected virtual Quaternion GetPointerRawRotation() { return Quaternion.identity; }
		protected virtual Quaternion GetPointerRotation(Quaternion rotation, Quaternion offset) { return Quaternion.Inverse(offset) * rotation; }
		// TODO: Remove hack.
		protected virtual Vector3 GetCameraPosition() { return new Vector3(0f, 0f, 0f); }
		protected virtual Quaternion GetCameraRotation() { return Quaternion.identity; }
	}
}