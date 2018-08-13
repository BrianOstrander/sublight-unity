using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueListModel : Model
	{
		[JsonProperty] Dictionary<string, bool> booleans = new Dictionary<string, bool>();
		[JsonProperty] Dictionary<string, int> integers = new Dictionary<string, int>();
		[JsonProperty] Dictionary<string, string> strings = new Dictionary<string, string>();

		#region Utility
		public bool GetBoolean(string key, bool fallback = false)
		{
			booleans.TryGetValue(key, out fallback);
			return fallback;
		}

		public int GetInteger(string key, int fallback = 0)
		{
			integers.TryGetValue(key, out fallback);
			return fallback;
		}

		public string GetString(string key, string fallback = null)
		{
			strings.TryGetValue(key, out fallback);
			return fallback;
		}

		public bool SetBoolean(string key, bool value)
		{
			booleans[key] = value;
			return value;
		}

		public int SetInteger(string key, int value)
		{
			integers[key] = value;
			return value;
		}

		public string SetString(string key, string value)
		{
			strings[key] = value;
			return value;
		}
		#endregion
	}
}