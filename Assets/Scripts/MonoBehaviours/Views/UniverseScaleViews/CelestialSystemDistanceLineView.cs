using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class CelestialSystemDistanceLineView : UniverseScaleView, ICelestialSystemDistanceLineView
	{
		enum Clamping
		{
			Unknown = 0,
			NoClamping = 10,
			BeginClamped = 20,
			EndClamped = 30,
			BothClamped = 40,
			NotVisible = 50,
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float yMinimumOffset;
		[SerializeField]
		LineRenderer bottomLine;
		[SerializeField]
		LineRenderer topLine;
		[SerializeField]
		float topLineMargin; // In unity units...
		[SerializeField]
		float bottomLineEndMargin; // In unity units...
		[SerializeField]
		float topLineSegmentLength; // In unity units...
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public void SetPoints(Vector3 begin, Vector3 end)
		{
			var minOffset = new Vector3(0f, yMinimumOffset, 0f);
			begin += minOffset;
			end += minOffset;

			Vector3? clampedBegin = null;
			Vector3? clampedEnd = null;
			var clamping = ClampPoints(begin, end, out clampedBegin, out clampedEnd);

			if (clampedBegin.HasValue)
			{
				Debug.DrawLine(clampedBegin.Value, clampedBegin.Value + (Vector3.up * 0.1f), Color.green, 0.1f);
			}

			if (clampedEnd.HasValue)
			{
				Debug.DrawLine(clampedEnd.Value, clampedEnd.Value + (Vector3.up * 0.1f), Color.red, 0.1f);
			}

			SetBottomPoints(begin, end);
			SetTopPoints(begin, end, clamping, clampedBegin, clampedEnd);
		}

		Clamping ClampPoints(Vector3 begin, Vector3 end, out Vector3? clampedBegin, out Vector3? clampedEnd)
		{
			// Incase you can't tell... I did not write this. Sourced frome here http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/

			clampedBegin = null;
			clampedEnd = null;

			var beginIsInRadius = PositionIsInRadius(begin);
			var endIsInRadius = PositionIsInRadius(end);

			if (beginIsInRadius && endIsInRadius) return Clamping.NoClamping;

			var pointInside = beginIsInRadius ? begin : end;
			var pointOutside = beginIsInRadius ? end : begin;

			var gridRadiusX = GridOrigin.x;
			var gridRadiesZ = GridOrigin.z;
			var radius = GridRadius;

			float xDelta, zDelta, A, B, C, det, t;

			xDelta = pointOutside.x - pointInside.x;
			zDelta = pointOutside.z - pointInside.z;

			A = xDelta * xDelta + zDelta * zDelta;
			B = 2f * (xDelta * (pointInside.x - GridOrigin.x) + zDelta * (pointInside.z - GridOrigin.z));
			C = (pointInside.x - GridOrigin.x) * (pointInside.x - GridOrigin.x) + (pointInside.z - GridOrigin.z) * (pointInside.z - GridOrigin.z) - radius * radius;

			det = B * B - 4f * A * C;
			if (Mathf.Approximately(A, 0f) || (det < 0f))
			{
				// No real solutions.
				return Clamping.NotVisible;
			}

			if (Mathf.Approximately(det, 0f))
			{
				// We don't care about a single tangent...
				return Clamping.NotVisible;
				/*
				// One solution.
				t = -B / (2f * A);
				var result = new Vector3(pointInside.x + t * xDelta, GridOrigin.y, pointInside.z + t * zDelta);
				var status = ClampedPointResults.NoClamping;

				if (beginIsInRadius)
				{
					clampedEnd = result;
					status = ClampedPointResults.EndClamped;
				}
				else 
				{
					clampedBegin = result;
					status = ClampedPointResults.BeginClamped;
				}
				return status;
				*/
			}

			// Two solutions.
			var doubleA = 2f * A;
			var sqrtDet = Mathf.Sqrt(det);

			t = (-B + sqrtDet) / doubleA;
			var result0 = new Vector3(pointInside.x + t * xDelta, GridOrigin.y, pointInside.z + t * zDelta);
			t = (-B - sqrtDet) / doubleA;
			var result1 = new Vector3(pointInside.x + t * xDelta, GridOrigin.y, pointInside.z + t * zDelta);

			var originalDistance = Vector3.Distance(begin, end);
			var beginOnGrid = begin.NewY(GridOrigin.y);
			var endOnGrid = end.NewY(GridOrigin.y);
			var distanceOnGrid = Vector3.Distance(beginOnGrid, endOnGrid);

			if (beginIsInRadius)
			{
				clampedEnd = result0;
				var scalar = Vector3.Distance(beginOnGrid, clampedEnd.Value) / distanceOnGrid;
				clampedEnd = begin + ((end - begin).normalized * (originalDistance * scalar));
			}
			else if (endIsInRadius)
			{
				clampedBegin = result0;
				var scalar = Vector3.Distance(endOnGrid, clampedBegin.Value) / distanceOnGrid;
				clampedBegin = end + ((begin - end).normalized * (originalDistance * scalar));
			}
			else
			{
				clampedBegin = result0;
				clampedEnd = result1;
			}

			return Clamping.BothClamped;
		}

		void SetBottomPoints(Vector3 begin, Vector3 end)
		{
			begin = begin.NewY(0f);
			end = end.NewY(0f);

			var delta = end - begin;

			var distance = Vector3.Distance(begin, end);
			var totalMargins = bottomLineEndMargin;

			if (distance <= totalMargins)
			{
				bottomLine.enabled = false;
				return;
			}
			bottomLine.enabled = true;

			distance -= totalMargins;

			var bottomDelta = delta.normalized * distance;

			bottomLine.SetPositions(new Vector3[] { Vector3.zero, bottomDelta });
		}

		void SetTopPoints(Vector3 begin, Vector3 end, Clamping clamping, Vector3? clampedBegin, Vector3? clampedEnd)
		{
			var delta = end - begin;

			var distance = Vector3.Distance(begin, end);
			var totalMargins = topLineMargin * 2f;

			if (distance <= totalMargins)
			{
				topLine.enabled = false;
				return;
			}
			topLine.enabled = true;

			distance -= totalMargins;

			var deltaNormal = delta.normalized;

			var topBegin = deltaNormal * topLineMargin;
			var topDelta = deltaNormal * distance;

			var topSegments = Mathf.Max(1, Mathf.RoundToInt(distance / topLineSegmentLength));
			var topSegmentDelta = topDelta * (1f / topSegments);

			var allTopSegments = new Vector3[topSegments + 1];

			//topBegin += new Vector3(0f, yMinimumOffset + (begin.y - transform.position.y), 0f); // this should be automatic...
			topBegin += new Vector3(0f, begin.y - transform.position.y, 0f); // this should be automatic...

			for (var i = 0; i < allTopSegments.Length; i++)
			{
				allTopSegments[i] = topBegin + (topSegmentDelta * i);
			}

			topLine.positionCount = allTopSegments.Length;
			topLine.SetPositions(allTopSegments);
		}

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;
				var currOpacity = Mathf.Approximately(value, 1f) ? 1f : 0f;
				topLine.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstant.Alpha, currOpacity);
				bottomLine.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstant.Alpha, currOpacity);
			}
		}
	}

	public interface ICelestialSystemDistanceLineView : IUniverseScaleView
	{
		void SetPoints(Vector3 begin, Vector3 end);
	}
}