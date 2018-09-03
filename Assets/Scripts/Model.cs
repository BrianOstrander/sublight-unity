using System;
using System.Linq;
using System.Runtime.Serialization;

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

		/// <summary>
		/// Don't know why I need this but I do, without this, something to do
		/// with serialization in newtonsoft.
		/// </summary>
		[OnDeserialized]
		void RegisterLanguageStrings(StreamingContext context)
		{
			OnRegisterLanguageStrings();
		}

		/// <summary>
		/// Updates all language strings registered with this model.
		/// </summary>
		/// <param name="edges">Edges.</param>
		public void UpdateLanguageStrings(params LanguageDatabaseEdge[] edges)
		{
			foreach (var entry in languageStringsListener.Value)
			{
				var edge = edges.FirstOrDefault(e => e.Key.Value == entry.Key.Value);
				entry.Value.Value = edge == null ? null : edge.Value.Value;
			}
			OnUpdateLanguageStrings(edges);
		}

		/// <summary>
		/// Adds the language strings so they'll be updated when
		/// UpdateLanguageStrings is run.
		/// </summary>
		/// <param name="entries">Entries.</param>
		protected void AddLanguageStrings(params LanguageStringModel[] entries)
		{
			languageStringsListener.Value = languageStringsListener.Value.Concat(entries).ToArray();
		}

		/// <summary>
		/// Updates all language strings registered with this model, call the
		/// UpdateLanguageStrings of child models to make sure they're updated.
		/// </summary>
		/// <param name="edges">Edges.</param>
		protected virtual void OnUpdateLanguageStrings(params LanguageDatabaseEdge[] edges) {}
		/// <summary>
		/// Where AddLanguageStrings should be called from.
		/// </summary>
		protected virtual void OnRegisterLanguageStrings() {}
	}
}
