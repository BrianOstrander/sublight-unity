using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEntryModel : EdgeEntryModel
	{
		[JsonProperty] KeyValueOperations operation;
		[JsonProperty] KeyValueTargets target;
		[JsonProperty] string key;

		[JsonProperty] string stringValue;
		[JsonProperty] bool boolValue;

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueOperations> Operation;
		[JsonIgnore]
		public readonly ListenerProperty<KeyValueTargets> Target;
		[JsonIgnore]
		public readonly ListenerProperty<string> Key;

		[JsonIgnore]
		public readonly ListenerProperty<string> StringValue;
		[JsonIgnore]
		public readonly ListenerProperty<bool> BoolValue;

		public KeyValueEntryModel()
		{
			Operation = new ListenerProperty<KeyValueOperations>(value => operation = value, () => operation);
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);

			StringValue = new ListenerProperty<string>(value => stringValue = value, () => stringValue);
			BoolValue = new ListenerProperty<bool>(value => boolValue = value, () => boolValue);
		}
	}
}