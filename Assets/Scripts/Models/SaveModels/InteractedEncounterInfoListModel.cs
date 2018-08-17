using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InteractedEncounterInfoListModel : SaveModel
	{
		[JsonProperty] InteractedEncounterInfoModel[] encounters = new InteractedEncounterInfoModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<InteractedEncounterInfoModel[]> Encounters;

		public InteractedEncounterInfoListModel()
		{
			SaveType = SaveTypes.InteractedEncounterInfoList;
			Encounters = new ListenerProperty<InteractedEncounterInfoModel[]>(value => encounters = value, () => encounters);
		}

		public InteractedEncounterInfoModel GetEncounter(string encounter)
		{
			var result = Encounters.Value.FirstOrDefault(e => e.EncounterId.Value == encounter);
			if (result == null)
			{
				result = new InteractedEncounterInfoModel();
				result.EncounterId.Value = encounter;
				Encounters.Value = Encounters.Value.Append(result).ToArray();
			}
			return result;
		}
	}
}