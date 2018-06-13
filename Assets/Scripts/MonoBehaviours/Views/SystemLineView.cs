using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class SystemLineView : View, ISystemLineView
	{
		[SerializeField]
		Vector3 offset;
		[SerializeField]
		LineRenderer safeSegment;
		[SerializeField]
		LineRenderer dangerSegment;
		[SerializeField]
		LineRenderer maxSegment;
		[SerializeField]
		LineRenderer remainderSegment;

		public UniversePosition UniversePosition { set; get; }

		public void SetSegments(LineSegment? safe = null, LineSegment? danger = null, LineSegment? max = null, LineSegment? remainder = null)
		{
			safeSegment.enabled = safe.HasValue;
			dangerSegment.enabled = danger.HasValue;
			maxSegment.enabled = max.HasValue;
			remainderSegment.enabled = remainder.HasValue;

			if (safe.HasValue) safeSegment.SetPositions(safe.Value.AllVector3.Add(offset));
			if (danger.HasValue) dangerSegment.SetPositions(danger.Value.AllVector3.Add(offset));
			if (max.HasValue) maxSegment.SetPositions(max.Value.AllVector3.Add(offset));
			if (remainder.HasValue) remainderSegment.SetPositions(remainder.Value.AllVector3.Add(offset));
		}

		public override void Reset()
		{
			base.Reset();

			UniversePosition = UniversePosition.Zero;
			SetSegments();
		}
		#region Events

		#endregion
	}

	public interface ISystemLineView : IGridTransform
	{
		void SetSegments(LineSegment? safe = null, LineSegment? danger = null, LineSegment? max = null, LineSegment? remainder = null);
	}
}