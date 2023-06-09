﻿using UnityEngine;
using LunraGames;

namespace LunraGames.SubLight
{
	public class CameraFollowAnimation : ViewAnimation
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		FloatRange depthRange = FloatRange.Normal;
		[SerializeField]
		AnimationCurve showDepthCurve = AnimationCurveExtensions.LinearNormal();
		[SerializeField]
		AnimationCurve closeDepthCurve = AnimationCurveExtensions.LinearInverseNormal();
		[SerializeField]
		FloatRange heightRange = FloatRange.Normal;
		[SerializeField]
		AnimationCurve showHeightCurve = AnimationCurveExtensions.LinearNormal();
		[SerializeField]
		AnimationCurve closeHeightCurve = AnimationCurveExtensions.LinearInverseNormal();
		[SerializeField]
		FloatRange pitchRange = FloatRange.Normal; // Should be angles not normal...
		[SerializeField]
		AnimationCurve showPitchCurve = AnimationCurveExtensions.LinearNormal();
		[SerializeField]
		AnimationCurve closePitchCurve = AnimationCurveExtensions.LinearInverseNormal();
		[SerializeField]
		bool idleLook;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		void UpdatePosition(
			IView view,
			float scalar,
			bool isShowing
		)
		{
			var depthCurve = isShowing ? showDepthCurve : closeDepthCurve;
			var heightCurve = isShowing ? showHeightCurve : closeHeightCurve;
			var pitchCurve = isShowing ? showPitchCurve : closePitchCurve;

			var position = App.V.CameraPosition + (App.V.CameraForward * depthRange.Evaluate(depthCurve.Evaluate(scalar)));
			position += App.V.CameraUp * heightRange.Evaluate(heightCurve.Evaluate(scalar));

			var pitch = pitchRange.Evaluate(pitchCurve.Evaluate(scalar)) * Mathf.Deg2Rad;
			var pitchForward = App.V.CameraRotation * new Vector3(0f, Mathf.Sin(pitch), Mathf.Cos(pitch));

			view.Root.position = position;

			// Didn't used to specify the camera up, if issues occur it could be related to that.
			view.Root.LookAt(position + pitchForward, App.V.CameraUp);
		}

		public override void OnShowing(IView view, float scalar)
		{
			UpdatePosition(view, scalar, true);
		}

		public override void OnLateIdle(IView view, float delta)
		{
			if (idleLook) UpdatePosition(view, 1f, true);
		}

		public override void OnClosing(IView view, float scalar)
		{
			UpdatePosition(view, scalar, false);
		}
	}
}