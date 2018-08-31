using System;
using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	// TODO: Unclear if we need this?
	public interface IModel 
	{
		ListenerProperty<string> Id { get; }
	}

	[Serializable]
	public abstract class Model : IModel
	{
		// TODO: Figure out what this is supposed to mean and if it's actually needed...
		[JsonProperty] string id;

		LanguageStringModel[] languageStrings = new LanguageStringModel[0];
		[JsonIgnore]
		readonly ListenerProperty<LanguageStringModel[]> languageStringsListener;

		[JsonIgnore]
		readonly ListenerProperty<string> idListener;
		[JsonIgnore]
		public ListenerProperty<string> Id { get { return idListener; } }
		[JsonIgnore]
		public readonly ReadonlyProperty<LanguageStringModel[]> LanguageStrings;

		public Model()
		{
			idListener = new ListenerProperty<string>(value => id = value, () => id);
			LanguageStrings = new ReadonlyProperty<LanguageStringModel[]>(
				value => languageStrings = value,
				() => languageStrings,
				out languageStringsListener
			);
		}

		public void UpdateLanguageStrings(params LanguageDatabaseEdge[] edges)
		{
			foreach (var entry in languageStringsListener.Value)
			{
				try
				{
					var edge = edges.First(e => e.Key == entry.Key.Value);
					entry.Value.Value = edge.Value;
				}
				catch (InvalidOperationException) { continue; }
			}
			OnUpdateLanguageStrings(edges);
		}

		protected void AddLanguageStrings(params LanguageStringModel[] entries)
		{
			languageStringsListener.Value = languageStringsListener.Value.Concat(entries).ToArray();
		}

		protected virtual void OnUpdateLanguageStrings(params LanguageDatabaseEdge[] edges) {}
	}
}
