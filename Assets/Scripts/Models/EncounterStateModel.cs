﻿using System;
using System.Linq;

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

		public struct Details
		{
			public static Details Default { get { return new Details(States.Complete, null); } }

			public States State;
			public string EncounterId;

			public Details(States state, string encounterId)
			{
				State = state;
				EncounterId = encounterId;
			}

			public Details NewState(States state) { return new Details { State = state, EncounterId = EncounterId }; }
		}

		Details current = Details.Default;
		public readonly ListenerProperty<Details> Current;

		EncounterStatus[] encounterStatuses = new EncounterStatus[0];
		/// <summary>
		/// The encounters seen, completed or otherwise.
		/// </summary>
		public readonly ListenerProperty<EncounterStatus[]> EncounterStatuses;

		#region KeyValues
		public KeyValueListener KeyValueListener { get; private set; }
		public KeyValueListModel KeyValues { get; private set; }

		public KeyValueListener RegisterKeyValueListener(KeyValueService keyValueService)
		{
			if (KeyValueListener != null) throw new Exception("Registering a new encounter keyvalue listener before unregestistering the previous one");

			KeyValues = KeyValues ?? new KeyValueListModel();

			return (KeyValueListener = new KeyValueListener(KeyValueTargets.Encounter, KeyValues, keyValueService).Register());
		}

		public void UnRegisterKeyValueListener()
		{
			if (KeyValueListener == null) throw new NullReferenceException("Unable to register a null encounter keyvalue listener");
			var oldKeyvalueListener = KeyValueListener;
			KeyValueListener = null;
			KeyValues = null;
			oldKeyvalueListener.UnRegister();
		}
		#endregion

		public EncounterStateModel()
		{
			Current = new ListenerProperty<Details>(value => current = value, () => current);
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