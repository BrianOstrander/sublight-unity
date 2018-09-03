using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageListModel : Model
	{
		[JsonProperty] LanguageDatabaseEdge[] edges = new LanguageDatabaseEdge[0];

		public void Set(
			string key,
			string value,
			Action<RequestStatus, LanguageDatabaseEdge> done = null
		)
		{
			OnSet(key, null, value, done);
		}

		public void Set(
			string key,
			int order,
			string value,
			Action<RequestStatus, LanguageDatabaseEdge> done = null
		)
		{
			OnSet(key, order, value, done);
		}

		void OnSet(
			string key,
			int? order,
			string value,
			Action<RequestStatus, LanguageDatabaseEdge> done = null
		)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException("Cannot be null or empty", "key");

			if (string.IsNullOrEmpty(value))
			{
				edges = edges.Where(e => e.Key != key).ToArray();
				if (done != null) done(RequestStatus.Success, default(LanguageDatabaseEdge));
				return;
			}

			var edge = edges.FirstOrDefault(e => e.Key == key);
			if (edge.IsEmpty)
			{
				edges = edges.Append(edge = new LanguageDatabaseEdge(key, value, order.HasValue ? order.Value : 0)).ToArray();
			}
			else
			{
				if (order.HasValue) edge.Order = order.Value;
				edge.Value = value;
			}

			if (done != null) done(RequestStatus.Success, edge.Duplicate);
		}

		public void Get(string key, Action<RequestStatus, LanguageDatabaseEdge> done)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException("Cannot be null or empty", "key");
			if (done == null) throw new ArgumentNullException("done");

			var edge = edges.FirstOrDefault(e => e.Key == key);

			done(edge.IsEmpty ? RequestStatus.Failure : RequestStatus.Success, edge);
		}

		/// <summary>
		/// Adds or sets the specified key values in the provided list, assuming
		/// the order is greater.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="order">Order.</param>
		/// <param name="done">Done.</param>
		public void Apply(
			LanguageListModel list,
			int order,
			Action done = null
		)
		{
			if (list == null) throw new ArgumentNullException("list");

			var newEdges = new List<LanguageDatabaseEdge>();

			foreach (var otherEdge in list.edges)
			{
				var edge = edges.FirstOrDefault(e => e.Key == otherEdge.Key);
				if (edge.IsEmpty) newEdges.Add(otherEdge.Duplicate);
				else if (edge.Order < order)
				{
					edge.Value = otherEdge.Value;
					edge.Order = order;
				}
			}

			edges = edges.Concat(newEdges).ToArray();

			if (done != null) done();
		}

		[JsonIgnore]
		public LanguageDatabaseEdge[] Edges
		{
			get
			{
				var result = new List<LanguageDatabaseEdge>();
				foreach (var edge in edges) result.Add(edge.Duplicate);
				return result.ToArray();
			}
			set
			{
				edges = value ?? new LanguageDatabaseEdge[0];
			}
		}

		[JsonIgnore]
		public string[] Duplicates
		{
			get
			{
				var duplicates = new List<string>();
				var uniques = new List<string>();
				foreach (var edge in edges)
				{
					if (uniques.Any(e => e == edge.Value)) duplicates.Add(edge.Value);
					else uniques.Add(edge.Value);
				}
				return edges.Where(e => duplicates.Contains(e.Value)).Select(e => e.Key).ToArray();
			}
		}
	}
}