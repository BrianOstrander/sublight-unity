using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ProceduralNoiseModel : SaveModel
	{
		[JsonProperty] string proceduralNoiseId;
		[JsonIgnore] public readonly ListenerProperty<string> ProceduralNoiseId;

		public ProceduralNoiseModel()
		{
			SaveType = SaveTypes.ProceduralNoise;
			ProceduralNoiseId = new ListenerProperty<string>(value => proceduralNoiseId = value, () => proceduralNoiseId);
		}
	}
}