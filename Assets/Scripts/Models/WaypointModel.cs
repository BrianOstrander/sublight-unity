using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class WaypointModel : Model
	{
		[Serializable]
		public struct LocationDetails
		{
			[JsonProperty] public readonly UniversePosition Position;
			[JsonProperty] public readonly int SystemIndex; // -1 if none

			[JsonIgnore] public bool IsSystem { get { return SystemIndex != -1; } }

			public LocationDetails(
				UniversePosition position,
				int systemIndex
			)
			{
				Position = position;
				SystemIndex = systemIndex;
			}
		}

		public enum VisibilityStates
		{
			Unknown = 0,
			Visible = 10,
			Hidden = 20
		}

		public enum VisitStates
		{
			Unknown = 0,
			NotVisited = 10,
			Current = 20,
			Visited = 30
		}

		public enum RangeStates
		{
			Unknown = 0,
			InRange = 10,
			OutOfRange = 20
		}

		public static WaypointModel Zero
		{
			get
			{
				var result = new WaypointModel();
				result.SetLocation(UniversePosition.Zero);
				return result;
			}
		}

		#region Serialized
		[JsonProperty] string waypointId;
		[JsonIgnore] public readonly ListenerProperty<string> WaypointId;

		[JsonProperty] string name;
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] int priority;
		[JsonIgnore] public readonly ListenerProperty<int> Priority;

		[JsonProperty] float distance;
		/// <summary>
		/// The distance in universe units.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<float> Distance;

		[JsonProperty] VisibilityStates visibilityState;
		[JsonIgnore] public readonly ListenerProperty<VisibilityStates> VisibilityState;

		[JsonProperty] VisitStates visitState;
		[JsonIgnore] public readonly ListenerProperty<VisitStates> VisitState;

		[JsonProperty] RangeStates rangeState;
		[JsonIgnore] public readonly ListenerProperty<RangeStates> RangeState;

		[JsonProperty] LocationDetails location;
		readonly ListenerProperty<LocationDetails> locationListener;
		[JsonIgnore] public readonly ReadonlyProperty<LocationDetails> Location;
		#endregion

		public WaypointModel()
		{
			WaypointId = new ListenerProperty<string>(value => waypointId = value, () => waypointId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Priority = new ListenerProperty<int>(value => priority = value, () => priority);
			Distance = new ListenerProperty<float>(value => distance = value, () => distance);
			VisibilityState = new ListenerProperty<VisibilityStates>(value => visibilityState = value, () => visibilityState);
			VisitState = new ListenerProperty<VisitStates>(value => visitState = value, () => visitState);
			RangeState = new ListenerProperty<RangeStates>(value => rangeState = value, () => rangeState);

			Location = new ReadonlyProperty<LocationDetails>(value => location = value, () => location, out locationListener);
		}

		#region Utilities
		public void SetLocation(UniversePosition position)
		{
			locationListener.Value = new LocationDetails(position, -1);
		}

		public void SetLocation(SystemModel system)
		{
			if (system == null) throw new ArgumentNullException("system");
			locationListener.Value = new LocationDetails(system.Position.Value, system.Index.Value);
		}
		#endregion
	}
}