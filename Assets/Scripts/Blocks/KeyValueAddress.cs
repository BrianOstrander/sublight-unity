using System;

namespace LunraGames.SubLight
{
	public interface IKeyValueAddress
	{
		KeyValueSources Source { get; }
		KeyValueTargets ForeignTarget { get; }
		string ForeignKey { get; }
	}

	[Serializable]
	public struct KeyValueAddress<T> : IKeyValueAddress
	{
		public static KeyValueAddress<T> Default { get { return Local(); } }

		public static KeyValueAddress<T> Local(T value = default(T))
		{
			return new KeyValueAddress<T>(
				KeyValueSources.LocalValue,
				KeyValueTargets.Unknown,
				null,

				value
			);
		}

		public static KeyValueAddress<T> Foreign(KeyValueTargets target, string key)
		{
			return new KeyValueAddress<T>(
				KeyValueSources.KeyValue,
				target,
				key,

				default(T)
			);
		}

		public KeyValueSources Source { get; set; }
		public KeyValueTargets ForeignTarget { get; set; }
		public string ForeignKey { get; set; }

		public T LocalValue { get; set; }

		KeyValueAddress(
			KeyValueSources source,
			KeyValueTargets foreignTarget,
			string foreignKey,

			T localValue
		)
		{
			Source = source;
			ForeignTarget = foreignTarget;
			ForeignKey = foreignKey;

			LocalValue = localValue;
		}
	}
}