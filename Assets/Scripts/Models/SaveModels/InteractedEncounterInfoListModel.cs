using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
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
	}
}