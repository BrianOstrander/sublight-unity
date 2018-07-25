using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class InventoryReferenceModel<T> : SaveModel where T : InventoryModel
	{
		[JsonProperty] T model;

		[JsonIgnore]
		public readonly ListenerProperty<T> Model;

		public InventoryReferenceModel(SaveTypes saveType)
		{
			SaveType = saveType;
			Model = new ListenerProperty<T>(value => model = value, () => model);
		}
	}
}