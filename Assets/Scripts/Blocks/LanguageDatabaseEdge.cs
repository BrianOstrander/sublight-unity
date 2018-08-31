using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct LanguageDatabaseEdge
	{
		[JsonIgnore]
		public string Key;
		[JsonIgnore]
		public int Order;
		public string Value;

		public LanguageDatabaseEdge(string value)
		{
			Key = null;
			Order = int.MinValue;
			Value = value;
		}

		public LanguageDatabaseEdge(string key, string value)
		{
			Key = key;
			Order = int.MinValue;
			Value = value;
		}

		public LanguageDatabaseEdge(int order, string value)
		{
			Key = null;
			Order = order;
			Value = value;
		}

		public LanguageDatabaseEdge(string key, int order, string value)
		{
			Key = key;
			Order = order;
			Value = value;
		}

		public LanguageDatabaseEdge Duplicate(string key) { return new LanguageDatabaseEdge(key, Order, Value); }
	}
}