namespace LunraGames.SpaceFarm.Models
{
	public class GameSaveModel : SaveModel
	{
		public override SaveTypes SaveType { get { return SaveTypes.Game; } }
	}
}