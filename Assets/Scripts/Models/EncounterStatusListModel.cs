using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterStatusListModel : Model
	{
		#region Serialized
		[JsonProperty] EncounterStatus[] statuses = new EncounterStatus[0];
		readonly ListenerProperty<EncounterStatus[]> statusesListener;
		[JsonIgnore] public readonly ReadonlyProperty<EncounterStatus[]> Statuses;
		#endregion

		public EncounterStatusListModel()
		{
			Statuses = new ReadonlyProperty<EncounterStatus[]>(value => statuses = value, () => statuses, out statusesListener);
		}

		public void SetEncounterStatus(EncounterStatus status)
		{
			if (status.Encounter == null)
			{
				Debug.LogError("Cannot update the status of an encounter with a null id, update ignored.");
				return;
			}
			statusesListener.Value = statusesListener.Value.Where(e => e.Encounter != status.Encounter).Append(status).ToArray();
		}

		public EncounterStatus GetEncounterStatus(string encounterId)
		{
			return Statuses.Value.FirstOrDefault(e => e.Encounter == encounterId);
		}
	}
}