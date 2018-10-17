using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class HoloRoomFocusCameraView : FocusCameraView, IHoloRoomFocusCameraView
	{
		[SerializeField]
		MeshRenderer maskMesh;
		[SerializeField]
		Gradient maskReveal;
		[SerializeField]
		Gradient maskHide;

		[SerializeField]
		Transform pivot;
		[SerializeField]
		Transform gantry;
		[SerializeField]
		Transform gantryAnchor;

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
		float orientationPreview;
		[SerializeField]
		bool liveMode;

		bool gantryStale;

		float orbit;
		float zoom;

		Vector3? orbitForward;
		Vector3 gantryPosition;
		Vector3 gantryForward;

		public void Mask(float scalar, bool revealing)
		{
			//var color = (revealing ? maskReveal : maskHide).Evaluate(scalar);
			maskMesh.material.SetColor(ShaderConstants.CameraMask.MaskColor, (revealing ? maskReveal : maskHide).Evaluate(scalar));

			if (revealing) maskMesh.gameObject.SetActive(!Mathf.Approximately(1f, scalar));
			else maskMesh.gameObject.SetActive(true);
		}

		public float Orbit
		{
			get { return orbit; }
			set
			{
				orbit = value;
				orbitForward = Vector3.forward;
				//Debug.LogWarning("Todo: orbital logic"); // This was annoying, but still needs to be done!
				gantryStale = true;
			}
		}

		public float Zoom
		{
			get { return zoom; }
			set
			{
				zoom = Mathf.Clamp01(value);
				GetOrientation(
					zoom,
					pivot.position,
					orbitForward.HasValue ? orbitForward.Value : Vector3.forward,
					out gantryPosition,
					out gantryForward
				);
				gantryStale = true;
			}
		}

		public Vector3 CameraPosition { get { return gantryPosition; } }
		public Vector3 CameraForward { get { return gantryForward; } }

		public Transform GantryAnchor { get { return gantryAnchor; } }

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

		public override void Reset()
		{
			base.Reset();

			Mask(1f, false);
			Orbit = 0f;
			Zoom = 1f;
		}

		#region Events
		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			if (Application.isEditor && liveMode)
			{
				Orbit = Orbit;
				Zoom = Zoom;
			}
			else if (!gantryStale) return;

			gantryStale = false;
			gantry.position = gantryPosition;
			gantry.forward = gantryForward;
		}
		#endregion

		void OnDrawGizmosSelected()
		{
			OnDrawEditorGizmos();
		}

		void OnDrawEditorGizmos()
		{
#if UNITY_EDITOR
			if (pivot == null) return;

			var pivotPosition = pivot.position;

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(pivotPosition, Vector3.one);

			const int focalSamples = 16;

			Vector3? last = null;
			for (var i = 0; i < focalSamples; i++)
			{
				var isFirst = i == 0;
				var isLast = i == focalSamples - 1;
				var progress = i / (float)(focalSamples - 1);

				Vector3 point;
				GetFocalOrientation(progress, pivotPosition, out point);

				if (isFirst || isLast) Gizmos.DrawWireCube(point, Vector3.one * 0.6f);
				else Gizmos.DrawWireSphere(point, 0.1f);

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

				if (isFirst || isLast) Gizmos.DrawWireCube(point, Vector3.one * 0.6f);
				else Gizmos.DrawWireSphere(point, 0.1f);

				Gizmos.DrawLine(point, point + (forward * 2f));


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
		void Mask(float scalar, bool revealing);

		float Orbit { get; set; }
		/// <summary>
		/// How close the camera is zoomed in. Lower values mean the camera is
		/// closer.
		/// </summary>
		/// <value>The zoom.</value>
		float Zoom { get; set; }

		Vector3 CameraPosition { get; }
		Vector3 CameraForward { get; }

		Transform GantryAnchor { get; }
	}
}