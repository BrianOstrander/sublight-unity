using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class CelestialSystemDistanceLineView : UniverseScaleView, ICelestialSystemDistanceLineView
	{
		[SerializeField]
		LineRenderer bottomLine;
		[SerializeField]
		LineRenderer topLine;
		[SerializeField]
		float topLineMargin; // In unity units...
		[SerializeField]
		float topLineSegmentLength; // In unity units...

		public void SetPoints(Vector3 begin, Vector3 end)
		{
			var delta = end - begin;
			bottomLine.SetPositions(new Vector3[] { Vector3.zero, delta });

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

			topBegin += new Vector3(0f, 0.5f, 0f); // this should be automatic...

			for (var i = 0; i < allTopSegments.Length; i++)
			{
				allTopSegments[i] = topBegin + (topSegmentDelta * i);
			}

			topLine.positionCount = allTopSegments.Length;
			topLine.SetPositions(allTopSegments);
		}
	}

	public interface ICelestialSystemDistanceLineView : IUniverseScaleView
	{
		void SetPoints(Vector3 begin, Vector3 end);
	}
}