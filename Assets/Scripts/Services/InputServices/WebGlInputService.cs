using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public class WebGlInputService : InputService
	{
		protected override Vector2 GetScreenPosition()
		{
			if (Camera.main == null)
			{
				Debug.LogError("Camera.main is null, unable to get screen position");
				return Vector2.one * -1f;
			}
			return Input.mousePosition;
		}

		protected override Quaternion GetPointerRotation()
		{
			if (Camera.main == null)
			{
				Debug.LogError("Camera.main is null, unable to get pointer rotation");
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(Camera.main.ScreenPointToRay(Input.mousePosition).direction, Camera.main.transform.up);
		}

		protected override Quaternion GetCameraRotation()
		{
			if (Camera.main == null)
			{
				Debug.LogError("Camera.main is null, unable to get pointer rotation");
				return Quaternion.identity;
			}
			return Camera.main.transform.rotation;
		}

		protected override Vector3 GetCameraPosition()
		{
			if (Camera.main == null)
			{
				Debug.LogError("Camera.main is null, unable to get pointer rotation");
				return Vector3.zero;
			}
			return Camera.main.transform.position;
		}

		protected override bool IsEscapeUp()
		{
			return Input.GetKeyUp(KeyCode.Escape);
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