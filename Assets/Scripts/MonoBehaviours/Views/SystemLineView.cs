using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class SystemLineView : View, ISystemLineView
	{
		[SerializeField]
		LineRenderer safeSegment;
		[SerializeField]
		LineRenderer dangerSegment;
		[SerializeField]
		LineRenderer maxSegment;

		public UniversePosition UniversePosition { set; get; }

		public void SetSegments(LineSegment? safe = null, LineSegment? danger = null, LineSegment? max = null)
		{
			safeSegment.enabled = safe.HasValue;
			dangerSegment.enabled = danger.HasValue;
			maxSegment.enabled = max.HasValue;

			if (safe.HasValue) safeSegment.SetPositions(safe.Value.AllVector3);
			if (danger.HasValue) safeSegment.SetPositions(danger.Value.AllVector3);
			if (max.HasValue) safeSegment.SetPositions(max.Value.AllVector3);
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
		void SetSegments(LineSegment? safe = null, LineSegment? danger = null, LineSegment? max = null);
	}
}