using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class KeyValueListModel : Model
	{
		[JsonProperty] Dictionary<string, bool> booleans = new Dictionary<string, bool>();
		[JsonProperty] Dictionary<string, int> integers = new Dictionary<string, int>();
		[JsonProperty] Dictionary<string, string> strings = new Dictionary<string, string>();
		[JsonProperty] Dictionary<string, float> floats = new Dictionary<string, float>();

		/// <summary>
		/// Keys are not case sensitive, so we normalize them here, along with
		/// any other operations.
		/// </summary>
		/// <returns>The key.</returns>
		/// <param name="key">Key.</param>
		string NormalizeKey(string key) { return string.IsNullOrEmpty(key) ? key : key.ToLower(); }

		#region Base Get
		public bool GetBoolean(string key, bool fallback = false)
		{
			booleans.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
		}

		public int GetInteger(string key, int fallback = 0)
		{
			integers.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
		}

		public string GetString(string key, string fallback = null)
		{
			strings.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
		}

		public T GetEnumeration<T>(string key, T fallback = default(T)) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new Exception(typeof(T).FullName + " is not an enum.");

			var intValue = Convert.ToInt32(fallback);
			integers.TryGetValue(NormalizeKey(key), out intValue);
			return Enum.GetValues(typeof(T)).Cast<T>().FirstOrDefault(e => Convert.ToInt32(e) == intValue);
		}

		public float GetFloat(string key, float fallback = 0f)
		{
			floats.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
		}

		public Color GetColor(KeyDefinitions.HsvaColor key, Color fallback = default(Color))
		{
			return Color.HSVToRGB(
				GetFloat(key.Hue.Key, fallback.GetH()),
				GetFloat(key.Saturation.Key, fallback.GetS()),
				GetFloat(key.Value.Key, fallback.GetV())
			).NewA(GetFloat(key.Alpha.Key, fallback.a));
		}
		#endregion

		#region Base Set
		public bool SetBoolean(string key, bool value)
		{
			booleans[NormalizeKey(key)] = value;
			return value;
		}

		public int SetInteger(string key, int value)
		{
			integers[NormalizeKey(key)] = value;
			return value;
		}

		public string SetString(string key, string value)
		{
			strings[NormalizeKey(key)] = value;
			return value;
		}

		public float SetFloat(string key, float value)
		{
			floats[NormalizeKey(key)] = value;
			return value;
		}

		public T SetEnumeration<T>(string key, T value) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new Exception(typeof(T).FullName + " is not an enum.");

			integers[NormalizeKey(key)] = Convert.ToInt32(value);
			return value;
		}

		public Color SetColor(KeyDefinitions.HsvaColor key, Color value)
		{
			SetFloat(key.Hue.Key, value.GetH());
			SetFloat(key.Saturation.Key, value.GetS());
			SetFloat(key.Value.Key, value.GetV());
			SetFloat(key.Alpha.Key, value.a);
			return value;
		}
		#endregion

		#region Defined Get & Set
		public bool Get(KeyDefinitions.Boolean key, bool fallback = false) { return GetBoolean(key.Key, fallback); }
		public int Get(KeyDefinitions.Integer key, int fallback = 0) { return GetInteger(key.Key, fallback); }
		public string Get(KeyDefinitions.String key, string fallback = null) { return GetString(key.Key, fallback); }
		public float Get(KeyDefinitions.Float key, float fallback = 0f) { return GetFloat(key.Key, fallback); }
		public T Get<T>(KeyDefinitions.Enumeration<T> key, T fallback = default(T)) where T : struct, IConvertible
		{
			return GetEnumeration(key.Key, fallback);
		}
		public Color Get(KeyDefinitions.HsvaColor key, Color fallback = default(Color)) { return GetColor(key, fallback); }

		public bool Set(KeyDefinitions.Boolean key, bool value) { return SetBoolean(key.Key, value); }
		public int Set(KeyDefinitions.Integer key, int value) { return SetInteger(key.Key, value); }
		public string Set(KeyDefinitions.String key, string value) { return SetString(key.Key, value); }
		public float Set(KeyDefinitions.Float key, float value) { return SetFloat(key.Key, value); }
		public T Set<T>(KeyDefinitions.Enumeration<T> key, T value) where T : struct, IConvertible
		{
			return SetEnumeration(key.Key, value);
		}
		public Color Set(KeyDefinitions.HsvaColor key, Color value) { return SetColor(key, value); }
		#endregion

		#region Utility
		public void Clear()
		{
			booleans.Clear();
			integers.Clear();
			strings.Clear();
			floats.Clear();
		}

		public string Dump(string prefix = null)
		{
			prefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".";

			var result = string.Empty;

			result += prefix + "Booleans:\n";

			if (booleans.Any())
			{
				foreach (var kv in booleans) result += "\t" + kv.Key + " , " + kv.Value+"\n";
			}
			else result += "\tNone\n";

			result += prefix + "Integers:\n";

			if (integers.Any())
			{
				foreach (var kv in integers) result += "\t" + kv.Key + " , " + kv.Value + "\n";
			}
			else result += "\tNone\n";

			result += prefix + "Strings:\n";

			if (strings.Any())
			{
				foreach (var kv in strings) result += "\t" + kv.Key + " , " + kv.Value + "\n";
			}
			else result += "\tNone\n";

			result += prefix + "Floats:\n";

			if (floats.Any())
			{
				foreach (var kv in floats) result += "\t" + kv.Key + " , " + kv.Value.ToString("N4") + "\n";
			}
			else result += "\tNone\n";

			return result;
		}

		[JsonIgnore]
		public KeyValueListModel Duplicate
		{
			get
			{
				var result = new KeyValueListModel
				{
					booleans = new Dictionary<string, bool>(booleans),
					integers = new Dictionary<string, int>(integers),
					strings = new Dictionary<string, string>(strings),
					floats = new Dictionary<string, float>(floats)
				};
				return result;
			}
		}

		public IKeyValueDelta[] GetDeltas(KeyValueListModel other)
		{
			var result = new List<IKeyValueDelta>();

			result.AddRange(
				GetDeltasTyped(
					KeyValueTypes.Boolean,
					booleans,
					other.booleans,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueTypes.Integer,
					integers,
					other.integers,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueTypes.String,
					strings,
					other.strings,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueTypes.Float,
					floats,
					other.floats,
					Mathf.Approximately
				)
			);

			return result.ToArray();
		}

		List<IKeyValueDelta> GetDeltasTyped<T>(
			KeyValueTypes type,
			Dictionary<string, T> values,
			Dictionary<string, T> valuesOther,
			Func<T, T, bool> comparison
		)
			where T : IConvertible
		{
			var result = new List<IKeyValueDelta>();

			var allKeys = values.Keys.ToList();
			allKeys.AddRange(valuesOther.Keys.ToList());
			allKeys = allKeys.Select(NormalizeKey).Distinct().ToList();

			foreach (var key in allKeys)
			{
				T value = default(T);
				T valueOther = default(T);

				values.TryGetValue(key, out value);
				valuesOther.TryGetValue(key, out valueOther);

				if (!comparison(value, valueOther))
				{
					result.Add(
						new KeyValueDelta<T>(
							key,
							type,
							value,
							valueOther
						)
					);
				}
			}

			return result;
		}

		public void Apply(KeyValueListModel other)
		{
			foreach (var kv in other.booleans) SetBoolean(kv.Key, kv.Value);
			foreach (var kv in other.integers) SetInteger(kv.Key, kv.Value);
			foreach (var kv in other.strings) SetString(kv.Key, kv.Value);
			foreach (var kv in other.floats) SetFloat(kv.Key, kv.Value);
		}
		#endregion
	}
}