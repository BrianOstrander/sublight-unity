using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaEntryModel : EdgeModel
	{
		[JsonProperty] string encyclopediaId;
		[JsonProperty] string title;
		[JsonProperty] string header;
		[JsonProperty] string body;
		[JsonProperty] int priority;
		[JsonProperty] int orderWeight;

		/// <summary>
		/// The encyclopedia identifier, used mostly internally for production
		/// purposes. Two entries may be the exact same despite having different
		/// EncyclopediaIds. (uhhh... why? Should this just use EntryId?)
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> EncyclopediaId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Title;
		[JsonIgnore]
		public readonly ListenerProperty<string> Header;
		[JsonIgnore]
		public readonly ListenerProperty<string> Body;
		[JsonIgnore]
		public readonly ListenerProperty<int> Priority;
		[JsonIgnore]
		public readonly ListenerProperty<int> OrderWeight;

		public override string EdgeName => string.IsNullOrEmpty(Header.Value) ? "Introduction" : "Section";
		
		public EncyclopediaEntryModel()
		{
			EncyclopediaId = new ListenerProperty<string>(value => encyclopediaId = value, () => encyclopediaId);
			Title = new ListenerProperty<string>(value => title = value, () => title);
			Header = new ListenerProperty<string>(value => header = value, () => header);
			Body = new ListenerProperty<string>(value => body = value, () => body);
			Priority = new ListenerProperty<int>(value => priority = value, () => priority);
			OrderWeight = new ListenerProperty<int>(value => orderWeight = value, () => orderWeight);
		}

		/// <summary>
		/// Gets a duplicate of this entry with a unique EncyclopediaId.
		/// </summary>
		/// <value>The duplicate.</value>
		[JsonIgnore]
		public EncyclopediaEntryModel Duplicate
		{
			get
			{
				var result = new EncyclopediaEntryModel();
				result.EncyclopediaId.Value = Guid.NewGuid().ToString();
				result.Title.Value = Title.Value;
				result.Header.Value = Header.Value;
				result.Body.Value = Body.Value;
				result.Priority.Value = Priority.Value;
				result.OrderWeight.Value = OrderWeight.Value;
				return result;
			}
		}
	}
}