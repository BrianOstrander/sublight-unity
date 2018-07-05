using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class CrewInventoryModel : InventoryModel
	{
		[JsonProperty] BodyTypes[] supportedBodies = new BodyTypes[0];

		[JsonIgnore]
		public readonly ListenerProperty<BodyTypes[]> SupportedBodies;

		protected CrewInventoryModel()
		{
			SupportedBodies = new ListenerProperty<BodyTypes[]>(value => supportedBodies = value, () => supportedBodies);
		}
	}
}