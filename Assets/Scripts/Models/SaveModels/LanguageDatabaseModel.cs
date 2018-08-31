using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageDatabaseModel : SaveModel
	{
		[JsonProperty] string[] tags = new string[0];
		[JsonProperty] LanguageListModel language = new LanguageListModel();

		[JsonIgnore]
		public readonly ListenerProperty<string[]> Tags;
		[JsonIgnore]
		public LanguageListModel Language { get { return language; } }

		public LanguageDatabaseModel()
		{
			SaveType = SaveTypes.LanguageDatabase;

			Tags = new ListenerProperty<string[]>(value => tags = value, () => tags);
		}
	}
}