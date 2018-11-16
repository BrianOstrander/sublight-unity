using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class CrewInventoryModel : InventoryModel, IExplorableInventoryModel
	{
		protected CrewInventoryModel()
		{

		}

		public bool IsExplorable(BodyModel body)
		{
			return true;
		}
	}
}