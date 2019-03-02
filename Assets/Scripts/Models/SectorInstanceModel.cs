using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SectorInstanceModel : Model
	{
		SectorModel sector;
		public readonly ListenerProperty<SectorModel> Sector;

		SystemInstanceModel[] systemModels;
		public readonly ListenerProperty<SystemInstanceModel[]> SystemModels;

		public SectorInstanceModel()
		{
			Sector = new ListenerProperty<SectorModel>(value => sector = value, () => sector);
			SystemModels = new ListenerProperty<SystemInstanceModel[]>(value => systemModels = value, () => systemModels);
		}
	}
}