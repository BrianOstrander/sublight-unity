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
		[JsonProperty] Dictionary<string, int> enums = new Dictionary<string, int>();
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

		public T GetEnum<T>(string key, T fallback = default(T)) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new Exception(typeof(T).FullName + " is not an enum.");

			var intValue = Convert.ToInt32(fallback);
			enums.TryGetValue(NormalizeKey(key), out intValue);
			return Enum.GetValues(typeof(T)).Cast<T>().FirstOrDefault(e => Convert.ToInt32(e) == intValue);
		}

		public int GetEnumInteger(string key, int fallback = 0)
		{
			enums.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
		}

		public float GetFloat(string key, float fallback = 0f)
		{
			floats.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
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

		public T SetEnum<T>(string key, T value) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new Exception(typeof(T).FullName + " is not an enum.");

			enums[NormalizeKey(key)] = Convert.ToInt32(value);
			return value;
		}

		public int SetEnumInteger(string key, int value)
		{
			enums[NormalizeKey(key)] = value;
			return value;
		}

		public float SetFloat(string key, float value)
		{
			floats[NormalizeKey(key)] = value;
			return value;
		}
		#endregion

		#region Defined Get & Set
		public bool Get(KeyDefinitions.Boolean key, bool fallback = false) { return GetBoolean(key.Key, fallback); }
		public int Get(KeyDefinitions.Integer key, int fallback = 0) { return GetInteger(key.Key, fallback); }
		public string Get(KeyDefinitions.String key, string fallback = null) { return GetString(key.Key, fallback); }
		public float Get(KeyDefinitions.Float key, float fallback = 0f) { return GetFloat(key.Key, fallback); }

		public bool Set(KeyDefinitions.Boolean key, bool value) { return SetBoolean(key.Key, value); }
		public int Set(KeyDefinitions.Integer key, int value) { return SetInteger(key.Key, value); }
		public string Set(KeyDefinitions.String key, string value) { return SetString(key.Key, value); }
		public float Set(KeyDefinitions.Float key, float value) { return SetFloat(key.Key, value); }
		#endregion

		#region Utility
		public void Clear()
		{
			booleans.Clear();
			integers.Clear();
			strings.Clear();
			enums.Clear();
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

			result += prefix + "Enums:\n";

			if (enums.Any())
			{
				foreach (var kv in enums) result += "\t" + kv.Key + " , " + kv.Value + "\n";
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
					enums = new Dictionary<string, int>(enums),
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
					KeyValueExtendedTypes.Boolean,
					booleans,
					other.booleans,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueExtendedTypes.Integer,
					integers,
					other.integers,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueExtendedTypes.String,
					strings,
					other.strings,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueExtendedTypes.Enum,
					enums,
					other.enums,
					(value, valueOther) => value == valueOther
				)
			);
			result.AddRange(
				GetDeltasTyped(
					KeyValueExtendedTypes.Float,
					floats,
					other.floats,
					Mathf.Approximately
				)
			);

			return result.ToArray();
		}

		List<IKeyValueDelta> GetDeltasTyped<T>(
			KeyValueExtendedTypes type,
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
			foreach (var kv in other.enums) SetEnumInteger(kv.Key, kv.Value);
			foreach (var kv in other.floats) SetFloat(kv.Key, kv.Value);
		}
		#endregion
	}
}