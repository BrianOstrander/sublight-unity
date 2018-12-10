using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public abstract class UniverseArea
	{
		public enum Types
		{
			Unknown = 0,
			Circle = 10,
			Rectangle = 20
		}

		[JsonProperty] UniversePosition origin;
		[JsonProperty] UniversePosition minimumDelta;
		[JsonProperty] UniversePosition maximumDelta;
		[JsonProperty] UniversePosition centerDelta;

		[JsonIgnore]
		public abstract Types AreaType { get; }

		[JsonIgnore]
		public UniversePosition Origin
		{
			get { return origin; }
			set
			{
				origin = value;
				OnOrigin(value);
			}
		}

		[JsonIgnore]
		public UniversePosition Minimum { get { return origin + minimumDelta; } }
		[JsonIgnore]
		public UniversePosition Maximum { get { return origin + maximumDelta; } }
		[JsonIgnore]
		public UniversePosition Center { get { return origin + centerDelta; } }

		[JsonIgnore]
		public UniversePosition MinimumDelta
		{
			get { return minimumDelta; }
			protected set { minimumDelta = value; }
		}

		[JsonIgnore]
		public UniversePosition MaximumDelta
		{
			get { return maximumDelta; }
			protected set { maximumDelta = value; }
		}

		[JsonIgnore]
		public UniversePosition CenterDelta
		{
			get { return centerDelta; }
			protected set { centerDelta = value; }
		}

		public float Proximity(UniversePosition position)
		{
			var contains = false;
			return Proximity(position, out contains);
		}

		public float Proximity(UniversePosition position, out bool contains)
		{
			contains = Contains(position);
			return OnProximity(position);
		}

		public bool Contains(UniversePosition position)
		{
			var min = Minimum;
			if (position.Sector.x < min.Sector.x || position.Sector.z < min.Sector.z) return false;
			var max = Maximum;
			if (max.Sector.x < position.Sector.x || max.Sector.z < position.Sector.z) return false;
			return OnContains(position);
		}

		#region Events
		protected virtual void OnOrigin(UniversePosition position) {}
		protected virtual float OnProximity(UniversePosition position) { return UniversePosition.Distance(Origin, position); }
		protected virtual bool OnContains(UniversePosition position) { return true; }
		#endregion
	}
}