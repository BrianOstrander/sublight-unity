using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEntryModel : EdgeEntryModel
	{
		[JsonProperty] float randomWeight;
		[JsonIgnore] public readonly ListenerProperty<float> RandomWeight;

		[JsonProperty] string nextLogId;
		[JsonIgnore] public readonly ListenerProperty<string> NextLogId;

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public SwitchEntryModel()
		{
			RandomWeight = new ListenerProperty<float>(value => randomWeight = value, () => randomWeight);
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
		}
	}
}