using System;

namespace LunraGames.SubLight
{
	public abstract class DefinedKeyTyped<T>
	{
		static ArgumentException GetValueTypeExecption(Type type, KeyValueTypes valueType)
		{
			return new ArgumentException("Generic type is " + type + " but KeyValueType is " + valueType + ", this is an invalid combination", "valueType");
		}

		public string Key { get; private set; }
		public KeyValueTargets Target { get; private set; }
		public KeyValueTypes ValueType { get; private set; }

		protected DefinedKeyTyped(
			string key,
			KeyValueTargets target,
			KeyValueTypes valueType
		)
		{
			var genericType = typeof(T);
			if (genericType == typeof(bool) && valueType != KeyValueTypes.Boolean) throw GetValueTypeExecption(genericType, valueType);
			if (genericType == typeof(int) && valueType != KeyValueTypes.Integer) throw GetValueTypeExecption(genericType, valueType);
			if (genericType == typeof(string) && valueType != KeyValueTypes.String) throw GetValueTypeExecption(genericType, valueType);
			if (genericType == typeof(float) && valueType != KeyValueTypes.Float) throw GetValueTypeExecption(genericType, valueType);

			Key = key;
			Target = target;
			ValueType = valueType;
		}
	}

	public static class DefinedKeys
	{
		public class DefinedKeyBoolean : DefinedKeyTyped<bool>
		{
			public DefinedKeyBoolean(string key, KeyValueTargets target) : base(key, target, KeyValueTypes.Boolean) { }
		}

		public class DefinedKeyInteger : DefinedKeyTyped<int>
		{
			public DefinedKeyInteger(string key, KeyValueTargets target) : base(key, target, KeyValueTypes.Integer) { }
		}

		public class DefinedKeyString : DefinedKeyTyped<string>
		{
			public DefinedKeyString(string key, KeyValueTargets target) : base(key, target, KeyValueTypes.String) { }
		}

		public class DefinedKeyFloat : DefinedKeyTyped<float>
		{
			public DefinedKeyFloat(string key, KeyValueTargets target) : base(key, target, KeyValueTypes.Float) { }
		}
	}
}