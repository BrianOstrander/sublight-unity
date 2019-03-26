using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight
{
	public static class KeyDefines
	{
		/// <summary>
		/// Names of resources are shared across severale classes.
		/// </summary>
		public static class Resources
		{
			public const string Rations = "rations";
			public const string Propellant = "propellant";
			public const string Metallics = "metallics";
		}

		#region Defined Keys
		public static readonly EncounterKeys Encounter = new EncounterKeys();
		public static readonly GameKeys Game = new GameKeys();
		public static readonly GlobalKeys Global = new GlobalKeys();
		public static readonly PreferencesKeys Preferences = new PreferencesKeys();
		public static readonly CelestialSystemKeys CelestialSystem = new CelestialSystemKeys();

		public static readonly KeyDefinitions[] AllTargets = {
			Encounter,
			Game,
			Global,
			Preferences,
			CelestialSystem
		};

		public static IKeyDefinition[] All { get { return AllTargets.SelectMany(t => t.All).ToArray(); } }
		#endregion
	}

	public interface IKeyDefinition
	{
		string Key { get; }
		KeyValueTargets Target { get; }
		KeyValueTypes ValueType { get; }
		string Notes { get; }
		bool CanWrite { get; }
		bool CanRead { get; }
	}

	public abstract class KeyDefinitionsTyped<T> : IKeyDefinition
	{
		static ArgumentException GetValueTypeExecption(Type type, KeyValueTypes valueType)
		{
			return new ArgumentException("Generic type is " + type + " but KeyValueType is " + valueType + ", this is an invalid combination", "valueType");
		}

		public string Key { get; private set; }
		public KeyValueTargets Target { get; private set; }
		public KeyValueTypes ValueType { get; private set; }
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

		protected KeyDefinitionsTyped(
			string key,
			KeyValueTargets target,
			KeyValueTypes valueType,
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
			Notes = notes;
			CanWrite = canWrite;
			CanRead = canRead;
		}
	}

	public abstract class KeyDefinitions
	{
		public class Boolean : KeyDefinitionsTyped<bool>
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
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class Integer : KeyDefinitionsTyped<int>
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
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class String : KeyDefinitionsTyped<string>
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
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class Float : KeyDefinitionsTyped<float>
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
				notes,
				canWrite,
				canRead
			)
			{ }
		}

		public class HsvaColor
		{
			public readonly Float Hue;
			public readonly Float Saturation;
			public readonly Float Value;
			public readonly Float Alpha;

			public HsvaColor(
				string key,
				KeyValueTargets target,
				string notes,
				bool canWrite,
				bool canRead
			)
			{
				Hue = new Float(
					key+"_hue",
					target,
					notes,
					canWrite,
					canRead
				);
				Saturation = new Float(
					key + "_saturation",
					target,
					notes,
					canWrite,
					canRead
				);
				Value = new Float(
					key + "_value",
					target,
					notes,
					canWrite,
					canRead
				);
				Alpha = new Float(
					key + "_alpha",
					target,
					notes,
					canWrite,
					canRead
				);
			}
		}

		public KeyValueTargets Target { get; private set; }

		public Boolean[] Booleans { get; protected set; }
		public Integer[] Integers { get; protected set; }
		public String[] Strings { get; protected set; }
		public Float[] Floats { get; protected set; }

		public IKeyDefinition[] All
		{
			get
			{
				return Booleans.Cast<IKeyDefinition>().Concat(Integers)
												   .Concat(Strings)
												   .Concat(Floats)
												   .ToArray();
			}
		}

		protected KeyDefinitions(KeyValueTargets target)
		{
			Target = target;
		}

		protected Boolean Create(
			ref Boolean instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true,
			Func<Boolean, Boolean> created = null
		)
		{
			created = created ?? (result => result);
			return instance = created(new Boolean(key, Target, notes, canWrite, canRead));
		}

		protected Integer Create(
			ref Integer instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true,
			Func<Integer, Integer> created = null
		)
		{
			created = created ?? (result => result);
			return instance = created(new Integer(key, Target, notes, canWrite, canRead));
		}

		protected String Create(
			ref String instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true,
			Func<String, String> created = null
		)
		{
			created = created ?? (result => result);
			return instance = created(new String(key, Target, notes, canWrite, canRead));
		}

		protected Float Create(
			ref Float instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true,
			Func<Float, Float> created = null
		)
		{
			created = created ?? (result => result);
			return instance = created(new Float(key, Target, notes, canWrite, canRead));
		}

		protected HsvaColor Create(
			ref HsvaColor instance,
			string key,
			string notes = null,
			bool canWrite = false,
			bool canRead = true,
			Func<HsvaColor, HsvaColor> created = null
		)
		{
			created = created ?? (result => result);
			instance = created(new HsvaColor(key, Target, notes, canWrite, canRead));

			Floats = Floats.Concat(
				new Float[] { instance.Hue, instance.Saturation, instance.Value, instance.Alpha }
			).ToArray();

			return instance;
		}
	}
}