using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class KeyValueEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] SetStringEntryEncounterLogModel[] setStrings = new SetStringEntryEncounterLogModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueEntryEncounterLogModel[]> KeyValues;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.KeyValue; } }

		public override bool EditableDuration { get { return false; } }

		public KeyValueEncounterLogModel()
		{
			KeyValues = new ListenerProperty<KeyValueEntryEncounterLogModel[]>(OnSetKeyValues, OnGetKeyValues);
		}

		#region Events
		void OnSetKeyValues(KeyValueEntryEncounterLogModel[] keyValues)
		{
			var setStringsList = new List<SetStringEntryEncounterLogModel>();

			foreach (var kv in keyValues)
			{
				switch (kv.KeyValueType)
				{
					case KeyValueEncounterLogTypes.SetString:
						setStringsList.Add(kv as SetStringEntryEncounterLogModel);
						break;
					default:
						Debug.LogError("Unrecognized KeyValueEncounterLogType: " + kv.KeyValueType);
						break;
				}
			}

			setStrings = setStringsList.ToArray();
		}

		KeyValueEntryEncounterLogModel[] OnGetKeyValues()
		{
			return setStrings.ToArray();
		}
		#endregion
	}
}