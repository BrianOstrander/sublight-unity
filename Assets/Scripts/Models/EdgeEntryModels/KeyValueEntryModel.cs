using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEntryModel : EdgeEntryModel
	{
		[Serializable]
		public struct BooleanBlock
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10
			}

			public static BooleanBlock Default { get { return new BooleanBlock(); } }

			public Operations Operation;
		}

		[Serializable]
		public struct IntegerBlock
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10
			}

			public static IntegerBlock Default { get { return new IntegerBlock(); } }

			public Operations Operation;
		}

		[Serializable]
		public struct StringBlock
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10
			}

			public static StringBlock Default { get { return new StringBlock(); } }

			public Operations Operation;
		}

		[Serializable]
		public struct FloatBlock
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10
			}

			public static FloatBlock Default { get { return new FloatBlock(); } }

			public Operations Operation;
		}

		[JsonProperty] KeyValueOperations operation;
		[JsonProperty] KeyValueTargets target;
		[JsonProperty] string key;

		[JsonProperty] bool booleanValue;
		[JsonProperty] int integerValue;
		[JsonProperty] string stringValue;
		[JsonProperty] float floatValue;

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueOperations> Operation;
		[JsonIgnore]
		public readonly ListenerProperty<KeyValueTargets> Target;
		[JsonIgnore]
		public readonly ListenerProperty<string> Key;

		[JsonIgnore]
		public readonly ListenerProperty<bool> BooleanValue;
		[JsonIgnore]
		public readonly ListenerProperty<int> IntegerValue;
		[JsonIgnore]
		public readonly ListenerProperty<string> StringValue;
		[JsonIgnore]
		public readonly ListenerProperty<float> FloatValue;

		public KeyValueEntryModel()
		{
			Operation = new ListenerProperty<KeyValueOperations>(value => operation = value, () => operation);
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);

			BooleanValue = new ListenerProperty<bool>(value => booleanValue = value, () => booleanValue);
			IntegerValue = new ListenerProperty<int>(value => integerValue = value, () => integerValue);
			StringValue = new ListenerProperty<string>(value => stringValue = value, () => stringValue);
			FloatValue = new ListenerProperty<float>(value => floatValue = value, () => floatValue);
		}
	}
}