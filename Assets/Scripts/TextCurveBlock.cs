using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TextCurveBlock
	{
		const float NormalSampleDelta = 0.05f;

		public static TextCurveBlock Default
		{
			get
			{
				return new TextCurveBlock
				{
					LabelStyle = GalaxyLabelStyles.Bold,
					FontSize = 16f,
					Curve = AnimationCurveExtensions.Constant(),
					CurveMaximum = 1f
				};
			}
		}

		public GalaxyLabelStyles LabelStyle;
		public float FontSize;
		public AnimationCurve Curve;
		public float CurveMaximum;
		public bool FlipAnchors;
		public bool FlipCurve;

		public Vector3 Begin(Vector3 beginAnchor, Vector3 endAnchor) { return FlipAnchors ? endAnchor : beginAnchor; }
		public Vector3 End(Vector3 beginAnchor, Vector3 endAnchor) { return FlipAnchors ? beginAnchor : endAnchor; }

		public Vector3 Delta(Vector3 beginAnchor, Vector3 endAnchor)
		{
			return End(beginAnchor, endAnchor) - Begin(beginAnchor, endAnchor);
		}

		public float Distance(Vector3 beginAnchor, Vector3 endAnchor)
		{
			return Vector3.Distance(End(beginAnchor, endAnchor), Begin(beginAnchor, endAnchor));
		}

		public float NormalizedCurveMaximum(Vector3 beginAnchor, Vector3 endAnchor)
		{
			return Distance(beginAnchor, endAnchor) * CurveMaximum;
		}

		public Vector3 Normal(Vector3 beginAnchor, Vector3 endAnchor)
		{
			return Delta(beginAnchor, endAnchor).normalized;
		}

		public Vector3 CurveUp(Vector3 beginAnchor, Vector3 endAnchor)
		{
			return Quaternion.AngleAxis(FlipCurve ? 90f : -90f, Vector3.up) * Normal(beginAnchor, endAnchor);
		}

		public Vector3 EvaluateLine(Vector3 beginAnchor, Vector3 endAnchor, float progress)
		{
			return Begin(beginAnchor, endAnchor) + (Delta(beginAnchor, endAnchor) * progress);
		}

		public Vector3 Evaluate(Vector3 beginAnchor, Vector3 endAnchor, float progress)
		{
			return EvaluateLine(beginAnchor, endAnchor, progress) + (CurveUp(beginAnchor, endAnchor) * ((Curve ?? AnimationCurveExtensions.Constant()).Evaluate(progress) * NormalizedCurveMaximum(beginAnchor, endAnchor)));
		}

		public Vector3 Evaluate(Vector3 beginAnchor, Vector3 endAnchor, float progress, bool flipNormals, out Vector3 normal)
		{
			var beginSample = Evaluate(beginAnchor, endAnchor, Mathf.Max(0f, progress - NormalSampleDelta));
			var endSample = Evaluate(beginAnchor, endAnchor, Mathf.Min(1f, progress + NormalSampleDelta));
			normal = Quaternion.AngleAxis(flipNormals ? 90f : -90f, Vector3.up) * (endSample - beginSample).normalized;
			return Evaluate(beginAnchor, endAnchor, progress);
		}

		public float EvaluateLength(Vector3 beginAnchor, Vector3 endAnchor, int samplingMinimum, int count)
		{
			count = Mathf.Max(samplingMinimum, count);
			var totalLength = 0f;
			var lastPoint = Evaluate(beginAnchor, endAnchor, 0f);
			for (var i = 0f; i < count; i++)
			{
				var currPoint = Evaluate(beginAnchor, endAnchor, (i + 1f) / count);
				totalLength += Vector3.Distance(lastPoint, currPoint);
				lastPoint = currPoint;
			}

			return totalLength;
		}
	}
}