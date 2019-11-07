using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEdgeModel : EdgeModel
	{
		[JsonProperty] float randomWeight;
		[JsonIgnore] public readonly ListenerProperty<float> RandomWeight;

		[JsonProperty] string nextLogId;
		[JsonIgnore] public readonly ListenerProperty<string> NextLogId;

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public override string EdgeName => "Switch";
		
		public SwitchEdgeModel()
		{
			RandomWeight = new ListenerProperty<float>(value => randomWeight = value, () => randomWeight);
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
		}
	}
}