using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class BodyModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] int bodyId;
		[JsonProperty] int parentId;
		[JsonProperty] string name;
		[JsonProperty] string encounterId;
		[JsonProperty] float rations;
		[JsonProperty] float fuel;
		[JsonProperty] float rationsAcquired;
		[JsonProperty] float fuelAcquired;
		[JsonProperty] BodyStatus status;

		/// <summary>
		/// Gets or sets the type of the body.
		/// </summary>
		/// <value>The type of the body.</value>
		[JsonProperty]
		public BodyTypes BodyType { get; protected set; }

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<int> BodyId;
		[JsonIgnore]
		public readonly ListenerProperty<int> ParentId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		[JsonIgnore]
		public readonly ListenerProperty<float> Fuel;
		[JsonIgnore]
		public readonly ListenerProperty<float> RationsAcquired;
		[JsonIgnore]
		public readonly ListenerProperty<float> FuelAcquired;
		[JsonIgnore]
		public readonly ListenerProperty<BodyStatus> Status;

		public BodyModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			BodyId = new ListenerProperty<int>(value => bodyId = value, () => bodyId);
			ParentId = new ListenerProperty<int>(value => parentId = value, () => parentId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel);
			RationsAcquired = new ListenerProperty<float>(value => rationsAcquired = value, () => rationsAcquired);
			FuelAcquired = new ListenerProperty<float>(value => fuelAcquired = value, () => fuelAcquired);
			Status = new ListenerProperty<BodyStatus>(value => status = value, () => status);
		}
	}
}