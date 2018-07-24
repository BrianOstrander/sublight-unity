using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class InventoryReferenceModel<T> : SaveModel where T : InventoryModel
	{
		[JsonProperty] T reference;

		[JsonIgnore]
		public readonly ListenerProperty<T> Reference;

		public InventoryReferenceModel(SaveTypes saveType)
		{
			SaveType = saveType;
			Reference = new ListenerProperty<T>(value => reference = value, () => reference);
		}
	}
}