using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public enum LineClamping
	{
		Unknown = 0,
		NoClamping = 10,
		BeginClamped = 20,
		EndClamped = 30,
		BothClamped = 40,
		NotVisible = 50,
	}

	public class CelestialSystemDistanceLineView : UniverseScaleView, ICelestialSystemDistanceLineView
	{

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
		float bottomLineMarginBegin; // In unity units...
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
		[SerializeField]
		GameObject directionRing;
		[SerializeField]
		MeshRenderer directionRingGraphic;
		[SerializeField]
		float rangeMargin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float GridRadiusMargin { get { return gridRadiusMargin; } }
		public float YMinimumOffset { get { return yMinimumOffset; } }

		public void SetPoints(
			Vector3 begin,
			Vector3 end,
			LineClamping clamping,
			Vector3? clampedBegin,
			Vector3? clampedEnd,
			float range
		)
		{
			switch (clamping)
			{
				case LineClamping.NotVisible:
					topLine.enabled = false;
					bottomLine.enabled = false;
					terminatorBegin.SetActive(false);
					terminatorEnd.SetActive(false);
					directionRing.SetActive(false);
					return;
			}

			var rangeFalloff = 1f;
			var totalDistance = Vector3.Distance(begin, end);
			if (range < totalDistance)
			{
				rangeFalloff = totalDistance - range;
				//rangeFalloff = Mathf.Max(-totalDistance, totalDistance - 1f);
			}

			SetBottomPoints(begin, end, clamping, clampedBegin, clampedEnd, range, rangeFalloff);
			SetTopPoints(begin, end, clamping, clampedBegin, clampedEnd, range, rangeFalloff);
		}

		void SetBottomPoints(
			Vector3 begin,
			Vector3 end,
			LineClamping clamping,
			Vector3? clampedBegin,
			Vector3? clampedEnd,
			float range,
			float falloff
		)
		{
			foreach (var material in bottomLine.materials)
			{
				material.SetFloat(ShaderConstants.TextureColorAlphaScrollingRange.RangeFadeFalloff, falloff);
				material.SetFloat(ShaderConstants.TextureColorAlphaScrollingRange.Range, range - rangeMargin);
				material.SetVector(ShaderConstants.TextureColorAlphaScrollingRange.RangeBegin, begin);
			}

			begin = begin.NewY(0f);
			end = end.NewY(0f);
			if (clampedBegin.HasValue) clampedBegin = clampedBegin.Value.NewY(0f);
			if (clampedEnd.HasValue) clampedEnd = clampedEnd.Value.NewY(0f);

			var bottomLineMarginBeginCurrent = bottomLineMarginBegin;
			var bottomLineMarginEndCurrent = bottomLineMarginEnd;
			var beginOffset = Vector3.zero;

			switch (clamping)
			{
				case LineClamping.BothClamped:
					bottomLineMarginBeginCurrent = Mathf.Max(0f, bottomLineMarginBeginCurrent - Vector3.Distance(begin, clampedBegin.Value));
					bottomLineMarginEndCurrent = Mathf.Max(0f, bottomLineMarginEndCurrent - Vector3.Distance(end, clampedEnd.Value));
					beginOffset = clampedBegin.Value - begin;
					begin = clampedBegin.Value;
					end = clampedEnd.Value;
					break;
				case LineClamping.BeginClamped:
					bottomLineMarginBeginCurrent = Mathf.Max(0f, bottomLineMarginBeginCurrent - Vector3.Distance(begin, clampedBegin.Value));
					beginOffset = clampedBegin.Value - begin;
					begin = clampedBegin.Value;
					break;
				case LineClamping.EndClamped:
					bottomLineMarginEndCurrent = Mathf.Max(0f, bottomLineMarginEndCurrent - Vector3.Distance(end, clampedEnd.Value));
					end = clampedEnd.Value;
					break;
			}

			var delta = end - begin;

			var distance = Vector3.Distance(begin, end);
			var totalMargins = bottomLineMarginEndCurrent + bottomLineMarginBeginCurrent;

			if (distance <= totalMargins)
			{
				bottomLine.enabled = false;
				directionRing.SetActive(false);
				return;
			}
			bottomLine.enabled = true;
			directionRing.SetActive(true);

			distance -= totalMargins;

			var bottomDelta = delta.normalized * distance;

			var bottomBegin = beginOffset + (bottomDelta.normalized * bottomLineMarginBeginCurrent);

			bottomLine.SetPositions(new Vector3[] { bottomBegin, bottomBegin + bottomDelta });

			directionRing.transform.forward = delta;
		}

		void SetTopPoints(
			Vector3 begin,
			Vector3 end,
			LineClamping clamping,
			Vector3? clampedBegin,
			Vector3? clampedEnd,
			float range,
			float falloff
		)
		{
			foreach (var material in topLine.materials)
			{
				material.SetFloat(ShaderConstants.TextureColorAlphaScrollingRange.RangeFadeFalloff, falloff);
				material.SetFloat(ShaderConstants.TextureColorAlphaScrollingRange.Range, range - rangeMargin);
				material.SetVector(ShaderConstants.TextureColorAlphaScrollingRange.RangeBegin, begin);
			}

			var topLineMarginBegin = topLineMargin;
			var topLineMarginEnd = topLineMargin;
			var beginOffset = Vector3.zero;

			switch (clamping)
			{
				case LineClamping.BothClamped:
					topLineMarginBegin = Mathf.Max(0f, topLineMarginBegin - Vector3.Distance(begin, clampedBegin.Value));
					topLineMarginEnd = Mathf.Max(0f, topLineMarginEnd - Vector3.Distance(end, clampedEnd.Value));
					beginOffset = (clampedBegin.Value - begin).NewY(0f);
					begin = clampedBegin.Value;
					end = clampedEnd.Value;
					break;
				case LineClamping.BeginClamped:
					topLineMarginBegin = Mathf.Max(0f, topLineMarginBegin - Vector3.Distance(begin, clampedBegin.Value));
					beginOffset = (clampedBegin.Value - begin).NewY(0f);
					begin = clampedBegin.Value;
					break;
				case LineClamping.EndClamped:
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
				terminatorBegin.SetActive(false);
				terminatorEnd.SetActive(false);
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
				case LineClamping.NoClamping:
					terminatorBegin.SetActive(false);
					terminatorEnd.SetActive(false);
					break;
				case LineClamping.BothClamped:
					var endPos = allTopSegments.Last();
					SetTerminator(terminatorBegin, topBegin);
					SetTerminator(terminatorEnd, endPos);
					break;
				case LineClamping.BeginClamped:
					terminatorEnd.SetActive(false);
					SetTerminator(terminatorBegin, topBegin);
					break;
				case LineClamping.EndClamped:
					terminatorBegin.SetActive(false);
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
				directionRingGraphic.material.SetFloat(ShaderConstants.HoloTextureColorAlphaMasked.Alpha, currOpacity);
			}
		}

		public override void Reset()
		{
			base.Reset();

			topLine.enabled = false;
			bottomLine.enabled = false;
			terminatorBegin.SetActive(false);
			terminatorEnd.SetActive(false);
			directionRing.SetActive(false);
		}
	}

	public interface ICelestialSystemDistanceLineView : IUniverseScaleView
	{
		float GridRadiusMargin { get; }
		float YMinimumOffset { get; }

		void SetPoints(
			Vector3 begin,
			Vector3 end,
			LineClamping clamping,
			Vector3? clampedBegin,
			Vector3? clampedEnd,
			float range
		);
	}
}