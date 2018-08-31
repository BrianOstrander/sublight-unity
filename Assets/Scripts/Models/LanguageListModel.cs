using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageListModel : Model
	{
		[JsonProperty] Dictionary<string, LanguageDatabaseEdge> entries = new Dictionary<string, LanguageDatabaseEdge>();

		public void Set(
			string key,
			int order,
			string value,
			Action<RequestStatus, LanguageDatabaseEdge> done = null
		)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException("Cannot be null or empty", "key");

			if (string.IsNullOrEmpty(value))
			{
				entries.Remove(key);
				if (done != null) done(RequestStatus.Success, default(LanguageDatabaseEdge));
				return;
			}

			LanguageDatabaseEdge entry;
			if (entries.TryGetValue(key, out entry))
			{
				entry.Order = order;
				entry.Value = value;
			}
			else entries.Add(key, entry = new LanguageDatabaseEdge(order, value));

			if (done == null) return;

			done(RequestStatus.Success, entry.Duplicate(key));
		}

		public void Get(string key, Action<RequestStatus, LanguageDatabaseEdge> done)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException("Cannot be null or empty", "key");
			if (done == null) throw new ArgumentNullException("done");

			LanguageDatabaseEdge entry = default(LanguageDatabaseEdge);
			var result = RequestStatus.Failure;
			if (entries.TryGetValue(key, out entry))
			{
				entry = entry.Duplicate(key);
				result = RequestStatus.Success;
			}

			done(result, entry);
		}

		public void Apply(
			LanguageListModel list,
			int order,
			Action done = null
		)
		{
			if (list == null) throw new ArgumentNullException("list");

			foreach (var kv in list.entries)
			{
				LanguageDatabaseEdge entry;
				if (entries.TryGetValue(kv.Key, out entry))
				{
					if (entry.Order < order)
					{
						entry.Order = order;
						entry.Value = kv.Value.Value;
					}
				}
				else entries.Add(kv.Key, new LanguageDatabaseEdge(null, order, kv.Value.Value));
			}

			if (done != null) done();
		}

		[JsonIgnore]
		public Dictionary<string, LanguageDatabaseEdge> Entries { get { return entries; } }
		[JsonIgnore]
		public LanguageDatabaseEdge[] Edges
		{
			get
			{
				var list = new List<LanguageDatabaseEdge>();
				foreach (var kv in Entries) list.Add(kv.Value.Duplicate(kv.Key));
				return list.ToArray();
			}
		}
	}
}