using System;

namespace LunraGames.SubLight
{
	public interface IKeyValueDelta
	{
		string Key { get; }
		KeyValueExtendedTypes Type { get; }
		object ValueOtherRaw { get; }
		object ValueRaw { get; }
	}

	public struct KeyValueDelta<T> : IKeyValueDelta
		where T : IConvertible
	{
		public string Key { get; private set; }
		public KeyValueExtendedTypes Type { get; private set; }
		public T Value { get; private set; }
		public T ValueOther { get; private set; }

		public object ValueRaw { get { return Value; } }
		public object ValueOtherRaw { get { return ValueOther; } }

		public KeyValueDelta(
			string key,
			KeyValueExtendedTypes type,
			T value,
			T valueOther
		)
		{
			Key = key;
			Type = type;
			Value = value;
			ValueOther = valueOther;
		}
	}
}