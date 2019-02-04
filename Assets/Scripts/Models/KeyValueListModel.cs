using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

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

		#region Utility
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

		public float GetFloat(string key, float fallback = 0f)
		{
			floats.TryGetValue(NormalizeKey(key), out fallback);
			return fallback;
		}

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

		public float SetFloat(string key, float value)
		{
			floats[NormalizeKey(key)] = value;
			return value;
		}

		public void Clear()
		{
			booleans.Clear();
			integers.Clear();
			strings.Clear();
			enums.Clear();
			floats.Clear();
		}
		#endregion
	}
}