using LunraGames.SpaceFarm.Views;
using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class StarModel : SystemModel
	{
		public override SystemTypes SystemType { get { return SystemTypes.Star; } }
	}
}