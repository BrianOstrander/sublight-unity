﻿using System;
using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class WaypointListModel : Model
	{
		#region Serialized
		[JsonProperty] WaypointModel[] waypoints = new WaypointModel[0];

		readonly ListenerProperty<WaypointModel[]> waypointsListener;
		[JsonIgnore]
		public readonly ReadonlyProperty<WaypointModel[]> Waypoints;
		#endregion

		public WaypointListModel()
		{
			Waypoints = new ReadonlyProperty<WaypointModel[]>(value => waypoints = value, () => waypoints, out waypointsListener);
		}

		public void AddWaypoint(WaypointModel waypoint)
		{
			if (waypointsListener.Value.Contains(waypoint)) return;
			waypointsListener.Value = waypointsListener.Value.Append(waypoint).ToArray();
		}

		public void RemoveWaypoint(WaypointModel waypoint)
		{
			waypointsListener.Value = waypointsListener.Value.Where(w => w != waypoint).ToArray();
		}

		public WaypointModel GetWaypointFirstOrDefault(string waypointId)
		{
			return GetWaypointFirstOrDefault(w => w.WaypointId.Value == waypointId);
		}

		public WaypointModel GetWaypointFirstOrDefault(Func<WaypointModel, bool> predicate)
		{
			if (predicate == null) throw new ArgumentNullException("predicate");

			return Waypoints.Value.FirstOrDefault(predicate);
		}
	}
}