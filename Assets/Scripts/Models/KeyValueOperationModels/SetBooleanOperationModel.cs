using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SetBooleanOperationModel : KeyValueOperationModel
	{
		[JsonProperty] bool value;

		[JsonIgnore]
		public readonly ListenerProperty<bool> Value;

		public override KeyValueOperations Operation { get { return KeyValueOperations.SetBoolean; } }

		public SetBooleanOperationModel()
		{
			Value = new ListenerProperty<bool>(value => this.value = value, () => value);
		}
	}
}