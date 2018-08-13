using UnityEngine;

namespace LunraGames.SubLight
{
	public struct LineSegment
	{
		public UniversePosition Start;
		public UniversePosition End;

		public Vector3 StartVector3 { get { return UniversePosition.ToUnity(Start); } }
		public Vector3 EndVector3 { get { return UniversePosition.ToUnity(End); } }
		public Vector3[] AllVector3 { get { return new Vector3[] { StartVector3, EndVector3 }; } }

		public LineSegment(UniversePosition start, UniversePosition end)
		{
			Start = start;
			End = end;
		}
	}
}