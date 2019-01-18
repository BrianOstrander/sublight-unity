using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEntryModel : EdgeEntryModel
	{
		[JsonProperty] string nextLogId;

		[JsonIgnore]
		public readonly ListenerProperty<string> NextLogId;

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public SwitchEntryModel()
		{
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
		}
	}
}