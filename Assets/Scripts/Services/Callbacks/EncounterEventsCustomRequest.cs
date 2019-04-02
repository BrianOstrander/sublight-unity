using System;
using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct EncounterEventsCustomRequest
	{
		public readonly string EventName;
		public readonly KeyValueListModel KeyValues;
		public readonly Action Done;

		public EncounterEventsCustomRequest(
			KeyValueListModel keyValues,
			Action done = null
		)
		{
			if (keyValues == null) throw new ArgumentNullException("keyValues");

			KeyValues = keyValues;
			EventName = KeyValues.GetString(EncounterEvents.Custom.StringKeys.CustomEventName);

			if (EventName == null) Debug.LogError("Unable to get custom event name");

			Done = done;
		}
	}
}