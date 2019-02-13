using System;
using System.Linq;

namespace LunraGames.SubLight
{
	public static class DefinedKeyInstances
	{
//#pragma warning disable CS0414 // The private field is assigned but its value is never used.
//#pragma warning restore CS0414 // The private field is assigned but its value is never used.

		public static readonly ValueFilterTypes[] SupportedFilterTypes = {
			ValueFilterTypes.KeyValueBoolean,
			ValueFilterTypes.KeyValueInteger,
			ValueFilterTypes.KeyValueString,
			ValueFilterTypes.KeyValueFloat
		};

		public static readonly EncounterKeys Encounter = new EncounterKeys();
		public static readonly GameKeys Game = new GameKeys();
		public static readonly GlobalKeys Global = new GlobalKeys();
		public static readonly PreferencesKeys Preferences = new PreferencesKeys();

		public static readonly DefinedKeys[] AllTargets = {
			Encounter,
			Game,
			Global,
			Preferences
		};

		public static IDefinedKey[] All { get { return AllTargets.SelectMany(t => t.All).ToArray(); } }
	}

	public interface IDefinedKey
	{
		string Key { get; }
		KeyValueTargets Target { get; }
		KeyValueTypes ValueType { get; }
		ValueFilterTypes FilterType { get; }
		string Notes { get; }
		bool CanWrite { get; }
		bool CanRead { get; }
	}

	public abstract class DefinedKeyTyped<T> : IDefinedKey
	{
		static ArgumentException GetValueTypeExecption(Type type, KeyValueTypes valueType)
		{
			return new ArgumentException("Generic type is " + type + " but KeyValueType is " + valueType + ", this is an invalid combination", "valueType");
		}

		public string Key { get; private set; }
		public KeyValueTargets Target { get; private set; }
		public KeyValueTypes ValueType { get; private set; }
		public ValueFilterTypes FilterType { get; private set; }
		public string Notes { get; private set; }
		/// <summary>
		/// If <see langword="true"/>, encounters can write to this key value.
		/// </summary>
		/// <value><c>true</c> if can write; otherwise, <c>false</c>.</value>
		public bool CanWrite { get; private set; }
		/// <summary>
		/// If <see langword="true"/>, encounters can read this key value.
		/// </summary>
		/// <value><c>true</c> if can read; otherwise, <c>false</c>.</value>
		public bool CanRead { get; private set; }

		protected DefinedKeyTyped(
			string key,
			KeyValueTargets target,
			KeyValueTypes valueType,
			ValueFilterTypes filterType,
			string notes,
			bool canWrite,
			bool canRead
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
			FilterType = filterType;
			Notes = notes;
			CanWrite = canWrite;
			CanRead = canRead;
		}
	}

	public abstract class DefinedKeys
	{
		public class Boolean : DefinedKeyTyped<bool>
		{
			public Boolean(
				string key,
				KeyValueTargets target,
				string notes,
				bool canWrite,
				bool canRead
			) : base(
				key,
				target,
				KeyValueTypes.Boolean,
				ValueFilterTypes.KeyValueBoolean,
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class Integer : DefinedKeyTyped<int>
		{
			public Integer(
				string key,
				KeyValueTargets target,
				string notes,
				bool canWrite,
				bool canRead
			) : base(
				key,
				target,
				KeyValueTypes.Integer,
				ValueFilterTypes.KeyValueInteger,
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class String : DefinedKeyTyped<string>
		{
			public String(
				string key,
				KeyValueTargets target,
				string notes,
				bool canWrite,
				bool canRead
			) : base(
				key,
				target,
				KeyValueTypes.String,
				ValueFilterTypes.KeyValueString,
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class Float : DefinedKeyTyped<float>
		{
			public Float(
				string key,
				KeyValueTargets target,
				string notes,
				bool canWrite,
				bool canRead
			) : base(
				key,
				target,
				KeyValueTypes.Float,
				ValueFilterTypes.KeyValueFloat,
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public KeyValueTargets Target { get; private set; }

		public Boolean[] Booleans { get; protected set; }
		public Integer[] Integers { get; protected set; }
		public String[] Strings { get; protected set; }
		public Float[] Floats { get; protected set; }

		public IDefinedKey[] All
		{
			get
			{
				return Booleans.Cast<IDefinedKey>().Concat(Integers)
												   .Concat(Strings)
												   .Concat(Floats)
												   .ToArray();
			}
		}

		protected DefinedKeys(KeyValueTargets target)
		{
			Target = target;
		}

		protected Boolean Create(
			ref Boolean instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true
		)
		{
			return instance = new Boolean(key, Target, notes, canWrite, canRead);
		}

		protected Integer Create(
			ref Integer instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true
		)
		{
			return instance = new Integer(key, Target, notes, canWrite, canRead);
		}

		protected String Create(
			ref String instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true
		)
		{
			return instance = new String(key, Target, notes, canWrite, canRead);
		}

		protected Float Create(
			ref Float instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true
		)
		{
			return instance = new Float(key, Target, notes, canWrite, canRead);
		}
	}
}