using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public interface IKeyValueDelta
	{
		string Key { get; }
		KeyValueTypes Type { get; }
		object ValueOtherRaw { get; }
		object ValueRaw { get; }
	}

	public struct KeyValueDelta<T> : IKeyValueDelta
		where T : IConvertible
	{
		public string Key { get; private set; }
		public KeyValueTypes Type { get; private set; }
		public T Value { get; private set; }
		public T ValueOther { get; private set; }

		public object ValueRaw { get { return Value; } }
		public object ValueOtherRaw { get { return ValueOther; } }

		public KeyValueDelta(
			string key,
			KeyValueTypes type,
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