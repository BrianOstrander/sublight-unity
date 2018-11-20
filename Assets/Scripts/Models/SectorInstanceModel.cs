using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SectorInstanceModel : Model
	{
		SectorModel sector;
		SystemInstanceModel[] systemModels;

		[JsonIgnore]
		public readonly ListenerProperty<SectorModel> Sector;
		[JsonIgnore]
		public readonly ListenerProperty<SystemInstanceModel[]> SystemModels;

		public SectorInstanceModel()
		{
			Sector = new ListenerProperty<SectorModel>(value => sector = value, () => sector);
			SystemModels = new ListenerProperty<SystemInstanceModel[]>(value => systemModels = value, () => systemModels);
		}
	}
}