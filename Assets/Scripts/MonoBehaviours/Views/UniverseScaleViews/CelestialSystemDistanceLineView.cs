using UnityEngine;

namespace LunraGames.SubLight.Views
{
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
		float bottomLineEndMargin; // In unity units...
		[SerializeField]
		float topLineSegmentLength; // In unity units...
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public void SetPoints(Vector3 begin, Vector3 end)
		{
			SetBottomPoints(begin, end);
			SetTopPoints(begin, end);
		}

		public void SetBottomPoints(Vector3 begin, Vector3 end)
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

		public void SetTopPoints(Vector3 begin, Vector3 end)
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

			topBegin += new Vector3(0f, yMinimumOffset + (begin.y - transform.position.y), 0f); // this should be automatic...

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