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
		float yawSensitivity;
		[SerializeField]
		float pitchSensitivity;
		[SerializeField]
		float radiusSensitivity;

		[SerializeField]
		AnimationCurve pitchLimiting;
		[SerializeField]
		float pitchLimitingMinimum;
		[SerializeField]
		AnimationCurve pitchRadiusOffset;
		[SerializeField]
		float pitchRadiusOffsetMaximum;
		[SerializeField]
		AnimationCurve pitchLookYOffset;
		[SerializeField]
		float pitchLookYOffsetMaximum;

		[SerializeField]
		AnimationCurve yawPitchLimiting;
		[SerializeField]
		float yawPitchLimitingMaximum;

		[SerializeField]
		float pitchSettleThreshold;
		[SerializeField]
		float pitchSettleSpeed;
		[SerializeField]
		float delayBeforePitchSettle;

		[Header("Testing")]
		[SerializeField]
		bool liveMode;
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

		public float PitchSettleThreshold { get { return pitchSettleThreshold; } }
		public float PitchSettleSpeed { get { return pitchSettleSpeed; } }
		public float DelayBeforePitchSettle { get { return delayBeforePitchSettle; } }

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
			radius = radius + (pitchRadiusOffset.Evaluate(pitch) * pitchRadiusOffsetMaximum);

			var lookOffset = new Vector3(0f, pitchLookYOffset.Evaluate(pitch) * pitchLookYOffsetMaximum, 0f);

			var pitchNormal = PitchNormal(pitch);
			var pitchZeroYawPosition = pitchNormal * (radiusMinimum + (radiusMaximum - radiusMinimum) * radius);
			var yawNormal = YawNormal(yaw);
			var originAtY = new Vector3(0f, pitchZeroYawPosition.y, 0f);
			position = lookPivot.position + (originAtY + (yawNormal * pitchZeroYawPosition.z));
			forward = ((lookPivot.position + lookOffset) - position).normalized;
		}

		float AddNormalized(float original, float delta)
		{
			original = (original + delta) % 1f;
			return original < 0f ? (1f + original) : original;
		}

		float AddLimited(float original, float delta, AnimationCurve limiting, float limitingMinimum, float sensitivity)
		{
			if ((delta < 0f && (original < 0.5f)) || (0f < delta && 0.5f < original))
			{
				sensitivity *= limitingMinimum + (limiting.Evaluate(original) * (1f - limitingMinimum));
			}
			return Mathf.Clamp01(original + (sensitivity * delta));
		}

		public void Set(float? yaw = null, float? pitch = null, float? radius = null)
		{
			if (yaw.HasValue) Yaw = yaw.Value;
			if (pitch.HasValue) Pitch = pitch.Value;
			if (radius.HasValue) Radius = radius.Value;
		}

		public void Input(float? yaw = null, float? pitch = null, float? radius = null)
		{
			if (yaw.HasValue && pitch.HasValue && !(Mathf.Approximately(yaw.Value, 0f) || Mathf.Approximately(pitch.Value, 0f)))
			{
				var yawAbs = Mathf.Abs(yaw.Value);
				var pitchAbs = Mathf.Abs(pitch.Value);
				var limiting = yawPitchLimiting.Evaluate(Mathf.Min(yawAbs, pitchAbs) / Mathf.Max(yawAbs, pitchAbs)) * yawPitchLimitingMaximum;
				if (yawAbs < pitchAbs) yaw = yaw.Value * limiting;
				else pitch = pitch.Value * limiting;
			}
			
			if (yaw.HasValue) Yaw = AddNormalized(Yaw, yaw.Value * yawSensitivity);
			if (pitch.HasValue) Pitch = AddLimited(Pitch, pitch.Value, pitchLimiting, pitchLimitingMinimum, pitchSensitivity);
			if (radius.HasValue) Radius = AddNormalized(Radius, radius.Value * radiusSensitivity);
		}

		public override void Reset()
		{
			base.Reset();

			Mask(1f, false);
			Yaw = 0f;
			Pitch = 0f;
			Radius = 0f;

			CalculateTransform();
		}

		void CalculateTransform()
		{
			GetOrientation(Yaw, Pitch, Radius, out gantryPosition, out gantryForward);

			gantry.position = gantryPosition;
			gantry.forward = gantryForward;
		}

		#region Events
		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			if (!transformStale)
			{
				if (Application.isEditor)
				{
					if (!liveMode) return;
				}
				else return;
			}

			transformStale = false;

			CalculateTransform();
		}
		#endregion

		//void OnDrawGizmos()
		//{
		//	OnDrawEditorGizmos();
		//}

		void OnDrawGizmosSelected()
		{
			OnDrawEditorGizmos();
		}

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
			Gizmos.DrawWireSphere(pitchBegin, 0.05f);
			Gizmos.color = Color.red.NewA(0.5f);
			Gizmos.DrawWireSphere(pitchEnd, 0.05f);

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

		float PitchSettleThreshold { get; }
		float PitchSettleSpeed { get; }
		float DelayBeforePitchSettle { get; }
	}
}