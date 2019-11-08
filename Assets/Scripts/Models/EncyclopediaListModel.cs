using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaListModel : Model
	{
		[JsonProperty] EncyclopediaEdgeModel[] entries = new EncyclopediaEdgeModel[0];
		[JsonProperty] string[] titles = new string[0];

		[JsonIgnore]
		readonly ListenerProperty<EncyclopediaEdgeModel[]> entriesListener;

		[JsonIgnore]
		public readonly ReadonlyProperty<EncyclopediaEdgeModel[]> Entries;
		[JsonIgnore]
		public readonly DerivedProperty<string[], EncyclopediaEdgeModel[]> Titles;

		public EncyclopediaListModel()
		{
			Entries = new ReadonlyProperty<EncyclopediaEdgeModel[]>(
				value => entries = value,
				() => entries,
				out entriesListener
			);

			Titles = new DerivedProperty<string[], EncyclopediaEdgeModel[]>(
				value => titles = value,
				() => titles,
				DeriveTitles,
				entriesListener
			);
		}

		#region Utility
		public bool HasTitle(string title) { return Entries.Value.Any(e => e.Title.Value == title); }
		public bool HasHeader(string title, string header) { return Entries.Value.Any(e => e.Title.Value == title && e.Header.Value == header); }

		public int GetHighestOrderWeight(string title)
		{
			var highest = Entries.Value.Where(e => e.Title.Value == title).OrderBy(e => e.OrderWeight.Value).LastOrDefault();
			return highest == null ? 0 : highest.OrderWeight.Value;
		}

		public EncyclopediaArticleModel GetArticle(string title)
		{
			var articleEntries = Entries.Value.Where(e => e.Title.Value == title).OrderBy(e => e.OrderWeight.Value);
			if (articleEntries.None()) return null;

			var result = new EncyclopediaArticleModel();
			result.Title.Value = title;
			result.Entries.Value = articleEntries.ToArray();
			return result;
		}

		public EncyclopediaArticleModel[] GetArticles()
		{
			var result = new List<EncyclopediaArticleModel>();
			foreach (var title in Titles.Value) result.Add(GetArticle(title));
			return result.ToArray();
		}

		public EncyclopediaEdgeModel Get(string title, string header)
		{
			return Entries.Value.FirstOrDefault(e => e.Title.Value == title && e.Header.Value == header);
		}

		public void Add(params EncyclopediaEdgeModel[] newEntries)
		{
			var entriesToAdd = new List<EncyclopediaEdgeModel>();
			var entriesToRemove = new List<string>();

			foreach (var entry in newEntries)
			{
				var existing = Get(entry.Title, entry.Header);
				if (existing == null)
				{
					entriesToAdd.Add(entry);
					continue;
				}
				if (entry.Priority.Value < existing.Priority.Value) continue;

				entry.OrderWeight.Value = existing.OrderWeight.Value;

				entriesToAdd.Add(entry);
				entriesToRemove.Add(existing.EncyclopediaId.Value);
				continue;
			}

			foreach (var entry in entriesToAdd.Where(e => e.OrderWeight.Value == -1))
			{
				entry.OrderWeight.Value = GetHighestOrderWeight(entry.Title) + 10;
			}

			if (entriesToAdd.None()) return;

			foreach (var existing in Entries.Value.Where(e => !entriesToRemove.Contains(e.EncyclopediaId.Value))) entriesToAdd.Add(existing);

			entriesListener.Value = entriesToAdd.ToArray();
		}
		#endregion

		#region Events
		string[] DeriveTitles(EncyclopediaEdgeModel[] entries)
		{
			var uniqueTitles = entries.Select(e => e.Title.Value).Distinct();
			return titles.IntersectEqual(uniqueTitles) ? titles : uniqueTitles.ToArray();
		}
		#endregion
	}
}