using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class GameSaveModel : SaveModel
	{
		[JsonProperty] GameModel game;

		[JsonIgnore]
		public readonly ListenerProperty<GameModel> Game;

		public GameSaveModel()
		{
			SaveType = SaveTypes.Game;
			Game = new ListenerProperty<GameModel>(value => game = value, () => game);
		}
	}
}