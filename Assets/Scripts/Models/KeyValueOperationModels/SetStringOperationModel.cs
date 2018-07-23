using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class SetStringOperationModel : KeyValueOperationModel
	{
		[JsonProperty] string value;

		[JsonIgnore]
		public readonly ListenerProperty<string> Value;

		public override KeyValueOperations Operation { get { return KeyValueOperations.SetString; } }

		public SetStringOperationModel()
		{
			Value = new ListenerProperty<string>(value => this.value = value, () => value);
		}
	}
}