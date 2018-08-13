using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] SetStringOperationModel[] setStrings = new SetStringOperationModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueOperationModel[]> Operations;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.KeyValue; } }

		public override bool EditableDuration { get { return false; } }

		public KeyValueEncounterLogModel()
		{
			Operations = new ListenerProperty<KeyValueOperationModel[]>(OnSetOperations, OnGetOperations);
		}

		#region Events
		void OnSetOperations(KeyValueOperationModel[] entries)
		{
			var setStringsList = new List<SetStringOperationModel>();

			foreach (var entry in entries)
			{
				switch (entry.Operation)
				{
					case KeyValueOperations.SetString:
						setStringsList.Add(entry as SetStringOperationModel);
						break;
					default:
						Debug.LogError("Unrecognized KeyValueOperation: " + entry.Operation);
						break;
				}
			}

			setStrings = setStringsList.ToArray();
		}

		KeyValueOperationModel[] OnGetOperations()
		{
			return setStrings.ToArray();
		}
		#endregion
	}
}