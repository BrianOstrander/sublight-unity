using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct LanguageDatabaseEdge
	{
		public string Key;
		public string Value;

		[JsonIgnore]
		public int Order;

		[JsonIgnore]
		public bool IsEmpty { get { return Key == null && Value == null; } }

		public LanguageDatabaseEdge(string key, string value)
		{
			Key = key;
			Value = value;
			Order = int.MinValue;
		}

		public LanguageDatabaseEdge(string key, string value, int order)
		{
			Key = key;
			Value = value;
			Order = order;
		}

		[JsonIgnore]
		public LanguageDatabaseEdge Duplicate { get { return new LanguageDatabaseEdge(Key, Value, Order); } }
	}
}