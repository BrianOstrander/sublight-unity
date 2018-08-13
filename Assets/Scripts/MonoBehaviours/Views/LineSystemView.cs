using System;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class LineSystemView : View, ILineSystemView
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

		public Vector3 UnityPosition
		{
			set { Root.position = value; }
			get { return Root.position; }
		}
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

			UnityPosition = Vector3.zero;
			UniversePosition = UniversePosition.Zero;
			SetSegments();
		}
		#region Events

		#endregion
	}

	public interface ILineSystemView : IGridTransform
	{
		void SetSegments(LineSegment? safe = null, LineSegment? danger = null, LineSegment? max = null, LineSegment? remainder = null);
	}
}