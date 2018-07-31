using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class InventoryReferenceModel<T> : SaveModel, IInventoryReferenceModel
		where T : InventoryModel
	{
		[JsonProperty] T model;

		[JsonIgnore]
		public readonly ListenerProperty<T> Model;
		[JsonIgnore]
		public InventoryModel RawModel { get { return Model.Value; } }

		public InventoryReferenceModel(SaveTypes saveType)
		{
			SaveType = saveType;
			Model = new ListenerProperty<T>(value => model = value, () => model);
		}
	}

	public interface IInventoryReferenceModel
	{
		InventoryModel RawModel { get; }
	}
}