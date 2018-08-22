using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueListModel : Model
	{
		[JsonProperty] Dictionary<string, bool> booleans = new Dictionary<string, bool>();
		[JsonProperty] Dictionary<string, int> integers = new Dictionary<string, int>();
		[JsonProperty] Dictionary<string, string> strings = new Dictionary<string, string>();

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
		#endregion
	}
}