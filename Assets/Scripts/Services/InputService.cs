using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LunraGames.SubLight
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

		Vector2 beginScrollGestureNormal;

		Vector2 lastScrollGesture;

		bool startedDragging;

		bool[] layerStates = new bool[32];

		public InputService(Heartbeat heartbeat, CallbackService callbacks)
		{
			this.heartbeat = heartbeat ?? throw new ArgumentNullException("heartbeat");
			this.callbacks = callbacks ?? throw new ArgumentNullException("callbacks");
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
			callbacks.InputLayerRequest += OnInputLayerRequest;
		}

		protected virtual void OnDisabled()
		{
			heartbeat.Update -= OnUpdate;
			callbacks.InputLayerRequest -= OnInputLayerRequest;
		}

		#region Events
		protected virtual void OnUpdate(float delta)
		{
			if (!IsFocused()) return;

			if (IsEscapeUp()) callbacks.Escape();

			var clickDown = IsClickDown();
			var clickHeldDown = IsClickHeldDown();
			var clickUp = IsClickUp();
			var anyInteraction = clickDown || clickHeldDown || clickUp;

			if (clickDown) clickDownTime = DateTime.Now;
			var clickDownDuration = DateTime.Now - clickDownTime;
			var clickClick = clickUp && clickDownDuration.TotalSeconds < GetClickDuration() && ((beginGesture - lastGesture).sqrMagnitude < MaximumClickDistance);

			// BEGIN GESTURE
			var gesturingBegan = GetGestureBegan();
			var gesturingEnded = GetGestureEnded();

			var currentGestureNormal = GetGesture();
			var currentGesture = currentGestureNormal * GetGestureSensitivity();

			var gestureState = Gesture.States.Active;

			if (gesturingBegan)
			{
				gestureState = Gesture.States.Begin;

				beginGesture = currentGesture;
				beginGestureNormal = currentGestureNormal;
				callbacks.BeginGesture(new Gesture(currentGestureNormal, IsSecondaryClickInteraction(), delta));
			}

			var gestureDeltaSinceLast = gestureState == Gesture.States.Begin ? Vector2.zero : (currentGesture - lastGesture);

			if (gesturingEnded)
			{
				gestureState = Gesture.States.End;

				callbacks.EndGesture(new Gesture(beginGestureNormal, currentGestureNormal, gestureDeltaSinceLast, gestureState, IsSecondaryClickInteraction(), delta));
			}

			if (IsGesturing() || gesturingEnded)
			{
				callbacks.CurrentGesture(new Gesture(beginGestureNormal, currentGestureNormal, gestureDeltaSinceLast, gestureState, IsSecondaryClickInteraction(), delta));
			}

			var gestureDeltaFromBegin = GetGestureDelta(gesturingBegan, gesturingEnded, beginGesture, lastGesture);

			lastGesture = currentGesture;
			// END GESTURE

			// BEGIN SCROLL GESTURE
			var scrollGesturingBegan = GetScrollGestureBegan();
			var scrollGesturingEnded = GetScrollGestureEnded();

			var currentScrollGestureNormal = GetScrollGesture();
			var currentScrollGesture = currentScrollGestureNormal * GetScrollGestureSensitivity();

			var scrollGestureState = ScrollGesture.States.Unknown;

			if (scrollGesturingBegan)
			{
				scrollGestureState = ScrollGesture.States.Begin;

				beginScrollGestureNormal = Vector2.zero;
				callbacks.BeginScrollGesture(new ScrollGesture(currentScrollGestureNormal, IsSecondaryClickInteraction(), delta));
			}

			var scrollGestureDeltaSinceLast = scrollGestureState == ScrollGesture.States.Begin ? currentScrollGesture : (currentScrollGesture - lastScrollGesture);

			if (scrollGesturingEnded)
			{
				scrollGestureState = ScrollGesture.States.End;
				callbacks.EndScrollGesture(
					new ScrollGesture(
						beginScrollGestureNormal,
						currentScrollGestureNormal,
						scrollGestureDeltaSinceLast,
						scrollGestureState,
						IsSecondaryClickInteraction(),
						delta
					)
				);
			}

			if (IsScrollGesturing() || scrollGesturingEnded)
			{
				if (scrollGestureState == ScrollGesture.States.Unknown) scrollGestureState = ScrollGesture.States.Active;
				callbacks.CurrentScrollGesture(
					new ScrollGesture(
						beginScrollGestureNormal,
						currentScrollGestureNormal,
						scrollGestureDeltaSinceLast,
						scrollGestureState,
						IsSecondaryClickInteraction(),
						delta
					)
				);
			}

			lastScrollGesture = currentScrollGesture;
			// END SCROLL GESTURE

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
				// Ignore non active layers.
				if (!layerStates[raycast.gameObject.layer]) continue;

				stillHighlighted.AddRange(OnExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerEnterHandler, r => !highlighted.Contains(r) && !stillHighlighted.Contains(r), r => !stillHighlighted.Contains(r)));

				if (!anyInteraction) break;
				if (clickDown)
				{
					wasTriggered |= OnExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerDownHandler).Any();
					wasTriggered |= OnExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.beginDragHandler).Any();
					dragging.Add(raycast.gameObject);
				}
				if (clickUp)
				{
					if (clickClick)
					{
						wasTriggered |= OnExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerClickHandler).Any();
					}
					wasTriggered |= OnExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.pointerUpHandler).Any();

					expiredDrags.Add(raycast.gameObject);
				}
				if (clickHeldDown && (startedDragging || IsDragging(gestureDeltaFromBegin)))
				{
					startedDragging = true;
					wasTriggered |= OnExecuteHierarchy(raycast.gameObject, pointerData, ExecuteEvents.dragHandler).Any();
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

			var highlightsExited = new List<GameObject>();
			Func<GameObject, bool> checkExecuteOrReturnHighlightExit = r => { return !stillHighlighted.Contains(r) && !highlightsExited.Contains(r); };
			foreach (var highlight in highlighted)
			{
				if (highlight != null && checkExecuteOrReturnHighlightExit(highlight))
				{
					highlightsExited.AddRange(OnExecuteHierarchy(highlight, pointerData, ExecuteEvents.pointerExitHandler, checkExecuteOrReturnHighlightExit, checkExecuteOrReturnHighlightExit));
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

			var pointerRotation = GetPointerRotation();
			callbacks.PointerOrientation(new PointerOrientation(cameraPosition, pointerRotation, screenPos));
		}

		void OnInputLayerRequest(InputLayerRequest request)
		{
			if (request.SetAllLayers.HasValue)
			{
				for (var i = 0; i < layerStates.Length; i++) layerStates[i] = request.SetAllLayers.Value;
				return;
			}

			foreach (var kv in request.LayerDeltas)
			{
				layerStates[LayerMask.NameToLayer(kv.Key)] = kv.Value;
			}
		}

		GameObject[] OnExecuteHierarchy<T>(
			GameObject root,
			BaseEventData eventData,
			ExecuteEvents.EventFunction<T> callbackFunction,
			Func<GameObject, bool> executeCondition = null,
			Func<GameObject, bool> returnCondition = null
		)
			where T : IEventSystemHandler
		{
			var results = new List<GameObject>();

			while (root != null)
			{
				root = ExecuteEvents.GetEventHandler<T>(root);

				if (root == null) break;

				if (executeCondition == null || executeCondition(root))
				{
					ExecuteEvents.Execute(root, eventData, callbackFunction);
				}

				if (returnCondition == null || returnCondition(root))
				{
					results.Add(root);
				}

				root = root.transform.parent.gameObject;
			}

			return results.ToArray();
		}
		#endregion
		protected virtual bool IsFocused() { return true; }
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

		protected virtual float GetScrollGestureSensitivity() { return 1f; }
		protected virtual bool GetScrollGestureBegan() { return false; }
		protected virtual bool IsScrollGesturing() { return false; }
		protected virtual bool GetScrollGestureEnded() { return false; }
		protected virtual Vector2 GetScrollGesture() { return Vector2.zero; }
	}
}