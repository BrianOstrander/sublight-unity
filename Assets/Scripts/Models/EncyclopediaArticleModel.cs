using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaArticleModel : Model
	{
		[JsonProperty] string title;
		[JsonProperty] EncyclopediaEdgeModel[] entries;

		[JsonIgnore]
		public readonly ListenerProperty<string> Title;
		[JsonIgnore]
		public readonly ListenerProperty<EncyclopediaEdgeModel[]> Entries;

		public EncyclopediaArticleModel()
		{
			Title = new ListenerProperty<string>(value => title = value, () => title);
			Entries = new ListenerProperty<EncyclopediaEdgeModel[]>(value => entries = value, () => entries);
		}
	}
}