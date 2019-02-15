using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct KeyValueAddress<T>
	{
		public static KeyValueAddress<T> Local(T value = default(T))
		{
			return new KeyValueAddress<T>(
				KeyValueSources.LocalValue,
				value,
				KeyValueTargets.Unknown,
				null
			);
		}

		public static KeyValueAddress<T> Foreign(KeyValueTargets target, string key)
		{
			return new KeyValueAddress<T>(
				KeyValueSources.KeyValue,
				default(T),
				target,
				key
			);
		}

		public KeyValueSources Source;
		public T LocalValue;
		public KeyValueTargets ForeignTarget;
		public string ForeignKey;

		KeyValueAddress(
			KeyValueSources source,
			T localValue,
			KeyValueTargets foreignTarget,
			string foreignKey
		)
		{
			Source = source;
			LocalValue = localValue;
			ForeignTarget = foreignTarget;
			ForeignKey = foreignKey;
		}
	}
}