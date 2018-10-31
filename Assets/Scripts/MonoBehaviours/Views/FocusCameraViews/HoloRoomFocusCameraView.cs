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
		Transform lookPivot;
		[SerializeField]
		Transform gantry;
		[SerializeField]
		Transform gantryAnchor;

		[SerializeField]
		float radiusMinimum;
		[SerializeField]
		float radiusMaximum;
		[SerializeField]
		float pitchMinimum;
		[SerializeField]
		float pitchMaximum;

		[SerializeField]
		AnimationCurve pitchLimitingMinimum;
		[SerializeField]
		AnimationCurve pitchLimitingMaximum;

		[Header("Testing")]
		[SerializeField]
		Vector3 YawPitchRadiusTest;

		float yaw;
		float pitch;
		float radius;
		bool transformStale;

		Vector3 gantryPosition;
		Vector3 gantryForward;

		public void Mask(float scalar, bool revealing)
		{
			//var color = (revealing ? maskReveal : maskHide).Evaluate(scalar);
			maskMesh.material.SetColor(ShaderConstants.CameraMask.MaskColor, (revealing ? maskReveal : maskHide).Evaluate(scalar));

			if (revealing) maskMesh.gameObject.SetActive(!Mathf.Approximately(1f, scalar));
			else maskMesh.gameObject.SetActive(true);
		}

		public float Yaw
		{
			get { return yaw; }
			set
			{
				yaw = value;
				transformStale = true;
			}
		}

		public float Pitch
		{
			get { return pitch; }
			set
			{
				pitch = value;
				transformStale = true;
			}
		}

		public float Radius
		{
			get { return radius; }
			set
			{
				radius = value;
				transformStale = true;
			}
		}

		public Vector3 CameraPosition { get { return gantryPosition; } }
		public Vector3 CameraForward { get { return gantryForward; } }

		public Transform GantryAnchor { get { return gantryAnchor; } }

		Vector3 PitchNormal(float pitch)
		{
			var adjustedMin = 90 - pitchMinimum;
			var adjustedMax = 90 - pitchMaximum;
			var xValue = (adjustedMin + ((adjustedMax - adjustedMin) * pitch)) * Mathf.Deg2Rad;
			return new Vector3(0f, Mathf.Cos(xValue), Mathf.Sin(xValue));
		}

		Vector3 YawNormal(float yaw)
		{
			var radYaw = ((360f * yaw) + 90f) * Mathf.Deg2Rad;
			return new Vector3(Mathf.Cos(radYaw), 0f, Mathf.Sin(radYaw)); 
		}

		void GetOrientation(float yaw, float pitch, float radius, out Vector3 position, out Vector3 forward)
		{
			var pitchNormal = PitchNormal(pitch);
			var pitchZeroYawPosition = pitchNormal * (radiusMinimum + (radiusMaximum - radiusMinimum) * radius);
			var yawNormal = YawNormal(yaw);
			var originAtY = new Vector3(0f, pitchZeroYawPosition.y, 0f);
			position = lookPivot.position + (originAtY + (yawNormal * pitchZeroYawPosition.z));
			forward = (lookPivot.position - position).normalized;
		}

		float GetValue(float original, float? specified) { return specified.HasValue ? specified.Value : original; }

		public void Set(float? yaw = null, float? pitch = null, float? radius = null)
		{
			Yaw = GetValue(Yaw, yaw);
			Pitch = GetValue(Pitch, pitch);
			Radius = GetValue(Radius, radius);
		}

		public void Input(float? yaw = null, float? pitch = null, float? radius = null)
		{
			Debug.Log("todo lol");
		}

		/*
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
		*/

		public override void Reset()
		{
			base.Reset();

			Mask(1f, false);
			Yaw = 0f;
			Pitch = 0f;
			Radius = 0f;
		}

		#region Events
		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			if (!transformStale) return;

			transformStale = false;



			gantry.position = gantryPosition;
			gantry.forward = gantryForward;
		}
		#endregion

		void OnDrawGizmos()
		{
			OnDrawEditorGizmos();
		}

		//void OnDrawGizmosSelected()
		//{
		//	OnDrawEditorGizmos();
		//}

		void OnDrawEditorGizmos()
		{
#if UNITY_EDITOR
			if (pivot == null || lookPivot == null) return;

			Handles.color = Color.green;
			Handles.DrawWireDisc(pivot.position, Vector3.up, radiusMinimum);
			Handles.color = Color.green.NewA(0.5f);
			Handles.DrawWireDisc(pivot.position, Vector3.up, radiusMaximum);
			// 6.23
			Gizmos.color = Color.red;
			var pitchBegin = lookPivot.position + (PitchNormal(0f) * radiusMinimum);
			var pitchEnd = lookPivot.position + (PitchNormal(1f) * radiusMinimum);
			Gizmos.DrawWireSphere(pitchBegin, 0.1f);
			Gizmos.color = Color.red.NewA(0.5f);
			Gizmos.DrawWireSphere(pitchEnd, 0.1f);

			var normalizedTest = new Vector3(YawPitchRadiusTest.x % 1f, YawPitchRadiusTest.y % 1f, YawPitchRadiusTest.z % 1f);

			var yawTest = normalizedTest.x;
			var pitchTest = normalizedTest.y;
			var radiusTest = normalizedTest.z;

			Gizmos.color = Color.yellow;

			Vector3 positionTest;
			Vector3 forwardTest;
			GetOrientation(yawTest, pitchTest, radiusTest, out positionTest, out forwardTest);

			Gizmos.DrawLine(positionTest, positionTest + forwardTest);
			Handles.color = Color.yellow;
			Handles.DrawWireDisc(new Vector3(0f, positionTest.y, 0f), Vector3.up, positionTest.NewY(0f).magnitude);

			Gizmos.DrawWireSphere(positionTest, 0.2f);

#endif
		}
	}

	public interface IHoloRoomFocusCameraView : IFocusCameraView
	{
		void Mask(float scalar, bool revealing);

		float Yaw { get; set; }
		float Pitch { get; set; }
		float Radius { get; set; }

		void Set(float? yaw = null, float? pitch = null, float? radius = null);
		void Input(float? yaw = null, float? pitch = null, float? radius = null);

		Vector3 CameraPosition { get; }
		Vector3 CameraForward { get; }

		Transform GantryAnchor { get; }
	}
}