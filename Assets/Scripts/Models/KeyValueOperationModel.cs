using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class KeyValueOperationModel : Model
	{
		[JsonProperty] string operationId;
		[JsonProperty] KeyValueTargets target;
		[JsonProperty] string key;

		/// <summary>
		/// The operation identifier, used internally for keeping track of this
		/// entry. This is NOT related to the key itself.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> OperationId;
		[JsonIgnore]
		public readonly ListenerProperty<KeyValueTargets> Target;
		[JsonIgnore]
		public readonly ListenerProperty<string> Key;

		[JsonIgnore]
		public abstract KeyValueOperations Operation { get; }

		public KeyValueOperationModel()
		{
			OperationId = new ListenerProperty<string>(value => operationId = value, () => operationId);
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);
		}
	}
}