using System.Linq;

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
		float bottomLineMarginEnd; // In unity units...
		[SerializeField]
		float topLineSegmentLength; // In unity units...
		[SerializeField]
		float gridRadiusMargin;
		[SerializeField]
		GameObject terminatorBegin;
		[SerializeField]
		GameObject terminatorEnd;
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

			switch (clamping)
			{
				case Clamping.NotVisible:
					topLine.enabled = false;
					bottomLine.enabled = false;
					terminatorBegin.SetActive(false);
					terminatorEnd.SetActive(false);
					return;
			}

			SetBottomPoints(begin, end, clamping, clampedBegin, clampedEnd);
			SetTopPoints(begin, end, clamping, clampedBegin, clampedEnd);
		}

		Clamping ClampPoints(Vector3 begin, Vector3 end, out Vector3? clampedBegin, out Vector3? clampedEnd)
		{
			// Incase you can't tell... I did not write this. Sourced frome here http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/

			clampedBegin = null;
			clampedEnd = null;

			var beginIsInRadius = PositionIsInRadius(begin, gridRadiusMargin);
			var endIsInRadius = PositionIsInRadius(end, gridRadiusMargin);

			if (beginIsInRadius && endIsInRadius) return Clamping.NoClamping;

			var pointInside = beginIsInRadius ? begin : end;
			var pointOutside = beginIsInRadius ? end : begin;

			var gridRadiusX = GridOrigin.x;
			var gridRadiesZ = GridOrigin.z;
			var radius = GridRadius - gridRadiusMargin;

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

			if (beginIsInRadius)
			{
				clampedEnd = CalculateClamped(begin, end, result0);
				return Clamping.EndClamped;
			}

			if (endIsInRadius)
			{
				clampedBegin = CalculateClamped(end, begin, result0);
				return Clamping.BeginClamped;
			}

			var minClamped = new Vector3(Mathf.Min(result0.x, result1.x), 0f, Mathf.Min(result0.z, result1.z));
			var maxClamped = new Vector3(Mathf.Max(result0.x, result1.x), 0f, Mathf.Max(result0.z, result1.z));
			var minOriginal = new Vector3(Mathf.Min(begin.x, end.x), 0f, Mathf.Min(begin.z, end.z));
			var maxOriginal = new Vector3(Mathf.Max(begin.x, end.x), 0f, Mathf.Max(begin.z, end.z));

			if ((maxOriginal.x < minClamped.x && maxOriginal.z < minClamped.z) || (maxClamped.x < minOriginal.x && maxClamped.z < minOriginal.z))
			{
				return Clamping.NotVisible;
			}

			clampedBegin = CalculateClamped(begin, end, result0);
			clampedEnd = CalculateClamped(end, begin, result1);

			return Clamping.BothClamped;
		}

		Vector3? CalculateClamped(Vector3 clampOrigin, Vector3 clampTermination, Vector3 position)
		{
			var originalDistance = Vector3.Distance(clampOrigin, clampTermination);
			var orginOnGrid = clampOrigin.NewY(GridOrigin.y);
			var terminationOnGrid = clampTermination.NewY(GridOrigin.y);
			var distanceOnGrid = Vector3.Distance(orginOnGrid, terminationOnGrid);

			var scalar = Vector3.Distance(orginOnGrid, position) / distanceOnGrid;
			return clampOrigin + ((clampTermination - clampOrigin).normalized * (originalDistance * scalar));
		}

		void SetBottomPoints(Vector3 begin, Vector3 end, Clamping clamping, Vector3? clampedBegin, Vector3? clampedEnd)
		{
			begin = begin.NewY(0f);
			end = end.NewY(0f);
			if (clampedBegin.HasValue) clampedBegin = clampedBegin.Value.NewY(0f);
			if (clampedEnd.HasValue) clampedEnd = clampedEnd.Value.NewY(0f);

			var bottomLineMarginEndCurrent = bottomLineMarginEnd;
			var beginOffset = Vector3.zero;

			switch (clamping)
			{
				case Clamping.BothClamped:
					bottomLineMarginEndCurrent = Mathf.Max(0f, bottomLineMarginEndCurrent - Vector3.Distance(end, clampedEnd.Value));
					beginOffset = clampedBegin.Value - begin;
					begin = clampedBegin.Value;
					end = clampedEnd.Value;
					break;
				case Clamping.BeginClamped:
					beginOffset = clampedBegin.Value - begin;
					begin = clampedBegin.Value;
					break;
				case Clamping.EndClamped:
					bottomLineMarginEndCurrent = Mathf.Max(0f, bottomLineMarginEndCurrent - Vector3.Distance(end, clampedEnd.Value));
					end = clampedEnd.Value;
					break;
			}

			var delta = end - begin;

			var distance = Vector3.Distance(begin, end);
			var totalMargins = bottomLineMarginEndCurrent;

			if (distance <= totalMargins)
			{
				bottomLine.enabled = false;
				return;
			}
			bottomLine.enabled = true;

			distance -= totalMargins;

			var bottomDelta = delta.normalized * distance;

			bottomLine.SetPositions(new Vector3[] { beginOffset, beginOffset + bottomDelta });
		}

		void SetTopPoints(Vector3 begin, Vector3 end, Clamping clamping, Vector3? clampedBegin, Vector3? clampedEnd)
		{
			var topLineMarginBegin = topLineMargin;
			var topLineMarginEnd = topLineMargin;
			var beginOffset = Vector3.zero;

			switch (clamping)
			{
				case Clamping.BothClamped:
					topLineMarginBegin = Mathf.Max(0f, topLineMarginBegin - Vector3.Distance(begin, clampedBegin.Value));
					topLineMarginEnd = Mathf.Max(0f, topLineMarginEnd - Vector3.Distance(end, clampedEnd.Value));
					beginOffset = (clampedBegin.Value - begin).NewY(0f);
					begin = clampedBegin.Value;
					end = clampedEnd.Value;
					break;
				case Clamping.BeginClamped:
					topLineMarginBegin = Mathf.Max(0f, topLineMarginBegin - Vector3.Distance(begin, clampedBegin.Value));
					beginOffset = (clampedBegin.Value - begin).NewY(0f);
					begin = clampedBegin.Value;
					break;
				case Clamping.EndClamped:
					topLineMarginEnd = Mathf.Max(0f, topLineMarginEnd - Vector3.Distance(end, clampedEnd.Value));
					end = clampedEnd.Value;
					break;
			}

			var delta = end - begin;

			var distance = Vector3.Distance(begin, end);
			var totalMargins = topLineMarginBegin + topLineMarginEnd;

			if (distance <= totalMargins)
			{
				topLine.enabled = false;
				return;
			}
			topLine.enabled = true;

			distance -= totalMargins;

			var deltaNormal = delta.normalized;

			var topBegin = beginOffset + (deltaNormal * topLineMarginBegin);
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

			switch (clamping)
			{
				case Clamping.NoClamping:
					terminatorBegin.SetActive(false);
					terminatorEnd.SetActive(false);
					break;
				case Clamping.BothClamped:
					var endPos = allTopSegments.Last();
					SetTerminator(terminatorBegin, topBegin);
					SetTerminator(terminatorEnd, endPos);
					break;
				case Clamping.BeginClamped:
					SetTerminator(terminatorBegin, topBegin);
					break;
				case Clamping.EndClamped:
					SetTerminator(terminatorEnd, allTopSegments.Last());
					break;
			}
		}

		void SetTerminator(GameObject terminator, Vector3 origin)
		{
			terminator.transform.localPosition = origin;
			terminator.transform.LookAt(GridOrigin.NewY(GridOrigin.y + origin.y));
			terminator.SetActive(true);
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

		public override void Reset()
		{
			base.Reset();

			topLine.enabled = false;
			bottomLine.enabled = false;
			terminatorBegin.SetActive(false);
			terminatorEnd.SetActive(false);
		}
	}

	public interface ICelestialSystemDistanceLineView : IUniverseScaleView
	{
		void SetPoints(Vector3 begin, Vector3 end);
	}
}