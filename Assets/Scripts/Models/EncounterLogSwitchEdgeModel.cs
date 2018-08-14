using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterLogSwitchEdgeModel : Model
	{
		[JsonProperty] string switchId;
		[JsonProperty] int index;
		[JsonProperty] bool ignore;
		[JsonProperty] string nextLogId;
		[JsonProperty] ValueFilterModel filtering = new ValueFilterModel();

		[JsonIgnore]
		public readonly ListenerProperty<string> SwitchId;
		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;
		[JsonIgnore]
		public readonly ListenerProperty<string> NextLogId;
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public EncounterLogSwitchEdgeModel()
		{
			SwitchId = new ListenerProperty<string>(value => switchId = value, () => switchId);
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
		}
	}
}