using UnityEngine;
using LunraGames;

namespace LunraGames.SpaceFarm
{
	public class WebGlInputService : InputService
	{
		Vector3 lastMousePosition;
		Vector2 lastMouseRotation = Vector2.zero;
		Quaternion lastCameraRotation = Quaternion.identity;
		bool isMouseLooking;

		protected override void OnUpdate(float delta)
		{
			if (Input.GetKeyDown(KeyCode.LeftControl)) lastMousePosition = Input.mousePosition;
			isMouseLooking = Input.GetKey(KeyCode.LeftControl);

			base.OnUpdate(delta);

			lastMousePosition = Input.mousePosition;
		}

		protected override Quaternion GetCameraRotation()
		{
			if (isMouseLooking)
			{
				var mouseDelta = Input.mousePosition - lastMousePosition;
				var rotationX = Mathf.Clamp(mouseDelta.x + lastMouseRotation.x, -360f, 360f);
				var rotationY = Mathf.Clamp(mouseDelta.y + lastMouseRotation.y, -85f, 85f);
				lastMouseRotation = new Vector2(rotationX, rotationY);
				lastCameraRotation = Quaternion.Euler(-lastMouseRotation.y, lastMouseRotation.x, 0f);
			}
			return lastCameraRotation;
		}

		protected override Vector2 GetScreenPosition()
		{
			if (Camera.main == null)
			{
				Debug.LogError("Camera.main is null, unable to get screen position");
				return Vector2.one * -1f;
			}
			return Input.mousePosition;
		}

		protected override Quaternion GetPointerRawRotation()
		{
			if (Camera.main == null)
			{
				Debug.LogError("Camera.main is null, unable to get pointer rotation");
				return Quaternion.identity;
			}
			var cameraForward = (Camera.main.ScreenToWorldPoint(Input.mousePosition.NewZ(4f)) - Camera.main.transform.position).normalized;
			return Quaternion.LookRotation(cameraForward, Camera.main.transform.up);

		}

		protected override bool IsClickUp()
		{
			return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
		}

		protected override bool IsSecondaryClickInteraction()
		{
			return Input.GetMouseButton(1) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1);
		}

		protected override bool IsClickDown()
		{
			return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
		}

		protected override bool IsClickHeldDown()
		{
			return Input.GetMouseButton(0) || Input.GetMouseButton(1);
		}

		protected override bool GetGestureBegan()
		{
			return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
		}

		protected override bool IsGesturing()
		{
			return Input.GetMouseButton(0) || Input.GetMouseButton(1);
		}

		protected override bool GetGestureEnded()
		{
			return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
		}

		protected override Vector2 GetGesture()
		{
			var mousePosition = Input.mousePosition;
			var clampedMouse = new Vector2(Mathf.Min(Screen.width, Mathf.Max(0f, mousePosition.x)), Mathf.Min(Screen.height, Mathf.Max(0f, mousePosition.y)));
			return ((new Vector2(clampedMouse.x / Screen.width, clampedMouse.y / Screen.height) * 2f) - Vector2.one);
		}
	}
}