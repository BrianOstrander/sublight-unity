using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageListModel : Model
	{
		[JsonProperty] LanguageDatabaseEdge[] edges = new LanguageDatabaseEdge[0];

		[JsonIgnore]
		public readonly ListenerProperty<LanguageDatabaseEdge[]> Edges;

		public LanguageListModel()
		{
			Edges = new ListenerProperty<LanguageDatabaseEdge[]>(OnSetEdges, OnGetEdges);
		}

		#region Events
		void OnSetEdges(LanguageDatabaseEdge[] newEdges)
		{
			var uniques = new Dictionary<string, string>();
			var duplicates = new List<string>();

			foreach (var edge in newEdges) edge.DuplicateKey.Value = null;

			foreach (var edge in newEdges)
			{
				if (string.IsNullOrEmpty(edge.Value.Value)) continue;

				string existingKey;
				if (uniques.TryGetValue(edge.Value.Value, out existingKey))
				{
					if (!duplicates.Contains(existingKey))
					{
						duplicates.Add(existingKey);
						newEdges.First(e => e.Key.Value == existingKey).DuplicateKey.Value = existingKey;
					}
					edge.DuplicateKey.Value = existingKey;
				}
				else uniques.Add(edge.Value.Value, edge.Key.Value);
			}

			edges = newEdges;
		}

		LanguageDatabaseEdge[] OnGetEdges() { return edges; }
		#endregion

		/// <summary>
		/// Adds or sets the specified key values in the provided list, assuming
		/// the order is greater.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="order">Order.</param>
		public void Apply(
			LanguageListModel list,
			int order
		)
		{
			if (list == null) throw new ArgumentNullException("list");

			var newEdges = new List<LanguageDatabaseEdge>();
			var currentEdges = Edges.Value;

			foreach (var otherEdge in list.Edges.Value)
			{
				var edge = currentEdges.FirstOrDefault(e => e.Key.Value == otherEdge.Key.Value);
				if (edge == null) newEdges.Add(otherEdge.Duplicate(order));
				else if (edge.Order < order) newEdges.Add(otherEdge.Duplicate(order));
				else newEdges.Add(edge.Duplicate());
			}

			Edges.Value = newEdges.ToArray();
		}

		public LanguageDatabaseEdge[] GetDuplicates(LanguageDatabaseEdge edge)
		{
			//return Edges.Value;
			return Edges.Value.Where(e => e.IsDuplicate && e.DuplicateKey == edge.DuplicateKey.Value).ToArray();
		}

		public int DuplicateCount()
		{
			var count = 0;
			foreach (var edge in Edges.Value)
			{
				if (edge.IsDuplicate) count++;
			}
			return count;
		}
	}
}