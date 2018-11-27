using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BodyModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] int bodyId;
		[JsonProperty] int parentId;
		[JsonProperty] string name;
		[JsonProperty] float encounterWeight;

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<int> BodyId;
		[JsonIgnore]
		public readonly ListenerProperty<int> ParentId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		/// <summary>
		/// A value from 0 to 1, that biases this one to have an encounter
		/// assigned to it, if the system has such an encounter.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> EncounterWeight;

		public BodyModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			BodyId = new ListenerProperty<int>(value => bodyId = value, () => bodyId);
			ParentId = new ListenerProperty<int>(value => parentId = value, () => parentId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			EncounterWeight = new ListenerProperty<float>(value => encounterWeight = value, () => encounterWeight);
		}
	}
}