using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class CrewInventoryModel : InventoryModel, IExplorableInventoryModel
	{
		[JsonProperty] BodyTypes[] supportedBodies = new BodyTypes[0];

		[JsonIgnore]
		public readonly ListenerProperty<BodyTypes[]> SupportedBodies;

		protected CrewInventoryModel()
		{
			SupportedBodies = new ListenerProperty<BodyTypes[]>(value => supportedBodies = value, () => supportedBodies);
		}

		public bool IsExplorable(BodyModel body)
		{
			return SupportedBodies.Value.Contains(body.BodyType);
		}
	}
}