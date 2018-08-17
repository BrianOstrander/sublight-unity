using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class LinearEncounterLogModel : EncounterLogModel
	{
		[JsonProperty] string nextLogId;

		[JsonIgnore]
		public readonly ListenerProperty<string> NextLogId;

		public override string NextLog { get { return NextLogId; } }

		public LinearEncounterLogModel()
		{
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
		}
	}
}