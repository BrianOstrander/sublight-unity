using System;
using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class EncounterStateModel : Model
	{
		public enum States
		{
			Unknown = 0,
			Processing = 10,
			Ending = 20,
			Complete = 30
		}

		#region Serialized Listeners
		[JsonProperty] States state = States.Complete;
		[JsonProperty] EncounterStatus[] encounterStatuses = new EncounterStatus[0];

		[JsonIgnore]
		public readonly ListenerProperty<States> State;
		[JsonIgnore]
		public readonly ListenerProperty<string> CurrentEncounter;
		/// <summary>
		/// The encounters seen, completed or otherwise.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<EncounterStatus[]> EncounterStatuses;
		#endregion

		#region KeyValues
		public KeyValueListener KeyValueListener { get; private set; }
		[JsonProperty] KeyValueListModel keyValues;

		public KeyValueListener RegisterKeyValueListener(KeyValueService keyValueService)
		{
			if (KeyValueListener != null) throw new Exception("Registering a new encounter keyvalue listener before unregestistering the previous one");

			keyValues = keyValues ?? new KeyValueListModel();

			return (KeyValueListener = new KeyValueListener(KeyValueTargets.Encounter, keyValues, keyValueService));
		}

		public void UnRegisterKeyValueListener()
		{
			if (KeyValueListener == null) throw new NullReferenceException("Unable to register a null encounter keyvalue listener");
			var oldKeyvalueListener = KeyValueListener;
			KeyValueListener = null;
			keyValues = null;
			oldKeyvalueListener.UnRegister();
		}
		#endregion

		public EncounterStateModel()
		{
			State = new ListenerProperty<States>(value => state = value, () => state);
			EncounterStatuses = new ListenerProperty<EncounterStatus[]>(value => encounterStatuses = value, () => encounterStatuses);
		}

		#region Utility
		public void SetEncounterStatus(EncounterStatus status)
		{
			if (status.Encounter == null)
			{
				Debug.LogError("Cannot update the status of an encounter with a null id, update ignored.");
				return;
			}
			EncounterStatuses.Value = EncounterStatuses.Value.Where(e => e.Encounter != status.Encounter).Append(status).ToArray();
		}

		public EncounterStatus GetEncounterStatus(string encounterId)
		{
			return EncounterStatuses.Value.FirstOrDefault(e => e.Encounter == encounterId);
		}
		#endregion
	}
}