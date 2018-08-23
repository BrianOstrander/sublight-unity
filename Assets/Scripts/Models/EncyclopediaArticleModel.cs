using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaArticleModel : Model
	{
		[JsonProperty] string title;
		[JsonProperty] EncyclopediaEntryModel[] entries;

		[JsonIgnore]
		public readonly ListenerProperty<string> Title;
		[JsonIgnore]
		public readonly ListenerProperty<EncyclopediaEntryModel[]> Entries;

		public EncyclopediaArticleModel()
		{
			Title = new ListenerProperty<string>(value => title = value, () => title);
			Entries = new ListenerProperty<EncyclopediaEntryModel[]>(value => entries = value, () => entries);
		}
	}
}