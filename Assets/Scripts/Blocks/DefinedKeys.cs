using System;
//using System.Collections.Generic;

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
		public string Notes { get; private set; }

		protected DefinedKeyTyped(
			string key,
			KeyValueTargets target,
			KeyValueTypes valueType,
			string notes
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
			Notes = notes;
		}
	}

	public abstract class DefinedKeys
	{
		public class Boolean : DefinedKeyTyped<bool>
		{
			public Boolean(string key, KeyValueTargets target, string notes) : base(key, target, KeyValueTypes.Boolean, notes) { }
		}

		public class Integer : DefinedKeyTyped<int>
		{
			public Integer(string key, KeyValueTargets target, string notes) : base(key, target, KeyValueTypes.Integer, notes) { }
		}

		public class String : DefinedKeyTyped<string>
		{
			public String(string key, KeyValueTargets target, string notes) : base(key, target, KeyValueTypes.String, notes) { }
		}

		public class Float : DefinedKeyTyped<float>
		{
			public Float(string key, KeyValueTargets target, string notes) : base(key, target, KeyValueTypes.Float, notes) { }
		}

		public KeyValueTargets Target { get; private set; }

		public Boolean[] Booleans { get; protected set; }
		public Integer[] Integers { get; protected set; }
		public String[] Strings { get; protected set; }
		public Float[] Floats { get; protected set; }

		protected DefinedKeys(KeyValueTargets target)
		{
			Target = target;
		}

		protected Boolean Create(
			ref Boolean instance,
			string key,
			string notes = null
		)
		{
			return instance = new Boolean(key, Target, notes);
		}

		protected Integer Create(
			ref Integer instance,
			string key,
			string notes = null
		)
		{
			return instance = new Integer(key, Target, notes);
		}

		protected String Create(
			ref String instance,
			string key,
			string notes = null
		)
		{
			return instance = new String(key, Target, notes);
		}

		protected Float Create(
			ref Float instance,
			string key,
			string notes = null
		)
		{
			return instance = new Float(key, Target, notes);
		}
	}
}