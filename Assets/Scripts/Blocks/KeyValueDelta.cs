using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public interface IKeyValueDelta
	{
		string Key { get; }
		KeyValueExtendedTypes ExtendedType { get; }
		KeyValueTypes Type { get; }
		object ValueOtherRaw { get; }
		object ValueRaw { get; }
	}

	public struct KeyValueDelta<T> : IKeyValueDelta
		where T : IConvertible
	{
		public string Key { get; private set; }
		public KeyValueExtendedTypes ExtendedType { get; private set; }
		public KeyValueTypes Type { get; private set; }
		public T Value { get; private set; }
		public T ValueOther { get; private set; }

		public object ValueRaw { get { return Value; } }
		public object ValueOtherRaw { get { return ValueOther; } }

		public KeyValueDelta(
			string key,
			KeyValueExtendedTypes extendedType,
			T value,
			T valueOther
		)
		{
			Key = key;
			ExtendedType = extendedType;
			Value = value;
			ValueOther = valueOther;

			switch (extendedType)
			{
				case KeyValueExtendedTypes.Boolean:
					Type = KeyValueTypes.Boolean;
					break;
				case KeyValueExtendedTypes.Integer:
					Type = KeyValueTypes.Integer;
					break;
				case KeyValueExtendedTypes.String:
					Type = KeyValueTypes.String;
					break;
				case KeyValueExtendedTypes.Float:
					Type = KeyValueTypes.Float;
					break;
				case KeyValueExtendedTypes.Enum:
					Debug.LogWarning("Unpredictable behaviour may occur when testing against enum deltas");
					Type = KeyValueTypes.Unknown;
					break;
				default:
					Debug.LogError("Unrecognized ExtendedType: " + extendedType);
					Type = KeyValueTypes.Unknown;
					break;
			}
		}
	}
}