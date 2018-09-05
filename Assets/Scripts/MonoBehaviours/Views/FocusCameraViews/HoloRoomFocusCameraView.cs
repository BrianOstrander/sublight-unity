using System;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace LunraGames.SubLight.Views
{
	public class HoloRoomFocusCameraView : FocusCameraView, IHoloRoomFocusCameraView
	{
		[SerializeField]
		Transform pivot;
		[SerializeField]
		Transform gantry;

		[SerializeField]
		Vector2 gantryHeightRange;
		[SerializeField]
		Vector2 gantryRadiusRange;
		[SerializeField]
		Vector2 focalHeightRange;
		[SerializeField]
		AnimationCurve gantryHeightCurve;
		[SerializeField]
		AnimationCurve gantryRadiusCurve;
		[SerializeField]
		AnimationCurve focalHeightCurve;

		[SerializeField, Header("Test"), Range(0f, 1f)]
		public float orientationPreview;

		void GetFocalOrientation(
			float progress,
			Vector3 pivotPosition,
			out Vector3 worldPosition
		)
		{
			progress = Mathf.Clamp01(progress);
			var focalHeightDelta = focalHeightRange.y - focalHeightRange.x;
			var focalHeight = focalHeightRange.x + (focalHeightDelta * focalHeightCurve.Evaluate(progress));
			worldPosition = pivotPosition + (Vector3.up * focalHeight);
		}

		void GetOrientation(
			float progress,
			Vector3 pivotPosition,
			Vector3 pivotForward,
			out Vector3 worldPosition,
			out Vector3 forward
		)
		{
			progress = Mathf.Clamp01(progress);
			var heightDelta = gantryHeightRange.y - gantryHeightRange.x;
			var radiusDelta = gantryRadiusRange.y - gantryRadiusRange.x;

			var height = gantryHeightRange.x + (heightDelta * gantryHeightCurve.Evaluate(progress));
			var radius = gantryRadiusRange.x + (radiusDelta * gantryRadiusCurve.Evaluate(progress));

			worldPosition = pivotPosition + ((pivotForward * radius) + (Vector3.up * height));

			Vector3 focalPoint;
			GetFocalOrientation(progress, pivotPosition, out focalPoint);

			forward = (focalPoint - worldPosition).normalized;
		}

		void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				OnDrawEditorGizmos();
				return;
			}
			/*
			Gizmos.color = Color.magenta;
			Gizmos.DrawRay(UnityPosition, Vector3.up);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(UnityPosition, DragForward.position);
			Gizmos.DrawWireCube(DragForward.position, Vector3.one);
			*/
		}

		void OnDrawEditorGizmos()
		{
#if UNITY_EDITOR
			if (pivot == null) return;

			var pivotPosition = pivot.position;

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(pivotPosition, Vector3.one);

			const int focalSamples = 8;

			Vector3? last = null;
			for (var i = 0; i < focalSamples; i++)
			{
				var isFirst = i == 0;
				var isLast = i == focalSamples - 1;
				var progress = i / (float)(focalSamples - 1);

				Vector3 point;
				GetFocalOrientation(progress, pivotPosition, out point);

				if (isFirst || isLast) Gizmos.DrawWireCube(point, Vector3.one * 0.8f);
				else Gizmos.DrawWireSphere(point, 0.2f);

				if (last.HasValue) Gizmos.DrawLine(last.Value, point);
				last = point;
			}

			last = null;

			Gizmos.color = Color.green;

			for (var i = 0; i < focalSamples; i++)
			{
				var isFirst = i == 0;
				var isLast = i == focalSamples - 1;
				var progress = i / (float)(focalSamples - 1);

				Vector3 point;
				Vector3 forward;
				GetOrientation(progress, pivotPosition, Vector3.forward, out point, out forward);

				if (isFirst || isLast) Gizmos.DrawWireCube(point, Vector3.one * 0.8f);
				else Gizmos.DrawWireSphere(point, 0.2f);

				Gizmos.DrawLine(point, point + (forward * 3f));


				if (last.HasValue) Gizmos.DrawLine(last.Value, point);
				last = point;

				if (isFirst)
				{
					Handles.color = Color.green;
					Handles.DrawWireDisc(pivotPosition, Vector3.up, Vector3.Distance(point.NewY(pivotPosition.y), pivotPosition));
				}
			}

			var pivotForward = Vector3.back;

			Vector3 testPoint;
			Vector3 testForward;
			GetOrientation(orientationPreview, pivotPosition, Vector3.forward, out testPoint, out testForward);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(testPoint, testPoint + (testForward * 4f));
#endif
		}
	}

	public interface IHoloRoomFocusCameraView : IFocusCameraView
	{
		
	}
}