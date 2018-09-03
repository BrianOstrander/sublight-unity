using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageDatabaseEdge
	{
		[JsonProperty] string key;
		[JsonProperty] string value;
		[JsonProperty] string notes;
		[JsonProperty] bool showNotes;
		[JsonProperty] string duplicateKey;

		int order;

		[JsonIgnore]
		public readonly ListenerProperty<string> Key;
		[JsonIgnore]
		public readonly ListenerProperty<string> Value;
		[JsonIgnore]
		public readonly ListenerProperty<string> Notes;
		[JsonIgnore]
		public readonly ListenerProperty<bool> ShowNotes;
		[JsonIgnore]
		public readonly ListenerProperty<string> DuplicateKey;

		[JsonIgnore]
		public bool IsDuplicate { get { return !string.IsNullOrEmpty(DuplicateKey); } }
		[JsonIgnore]
		public int Order { get { return order; } }

		public LanguageDatabaseEdge()
		{
			Key = new ListenerProperty<string>(value => key = value, () => key);
			Value = new ListenerProperty<string>(v => value = v, () => value);
			Notes = new ListenerProperty<string>(value => notes = value, () => notes);
			ShowNotes = new ListenerProperty<bool>(value => showNotes = value, () => showNotes);
			DuplicateKey = new ListenerProperty<string>(value => duplicateKey = value, () => duplicateKey);
		}

		public LanguageDatabaseEdge Duplicate(int? order = null)
		{
			var result = new LanguageDatabaseEdge();
			
			result.Key.Value = Key.Value;
			result.Value.Value = Value.Value;
			result.Notes.Value = Notes.Value;
			result.ShowNotes.Value = ShowNotes.Value;
			result.DuplicateKey.Value = DuplicateKey.Value;

			result.order = order.HasValue ? order.Value : Order;

			return result;
		}
	}
}