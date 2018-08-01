using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class InventoryReferenceModel<T> : SaveModel, IInventoryReferenceModel
		where T : InventoryModel
	{
		[JsonProperty] T model;

		[JsonIgnore]
		bool isInitialized;
		[JsonIgnore]
		public readonly ListenerProperty<T> Model;
		[JsonIgnore]
		public InventoryModel RawModel { get { return Model.Value; } }

		public InventoryReferenceModel(SaveTypes saveType)
		{
			SaveType = saveType;
			Model = new ListenerProperty<T>(value => model = value, () => model);
		}

		public void InitializeInstance()
		{
			if (isInitialized) return;
			isInitialized = true;

			Model.Value.InstanceId.Value = Guid.NewGuid().ToString();

			OnInitializeInstance();
		}

		protected virtual void OnInitializeInstance() {}
	}

	public interface IInventoryReferenceModel
	{
		InventoryModel RawModel { get; }

		void InitializeInstance();
	}
}