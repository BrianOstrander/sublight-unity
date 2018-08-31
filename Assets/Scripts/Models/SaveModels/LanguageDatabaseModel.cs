using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageDatabaseModel : SaveModel
	{
		[JsonProperty] bool ignore;
		[JsonProperty] string languageId;
		[JsonProperty] string languageTag;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string[] tags = new string[0];
		[JsonProperty] LanguageListModel language = new LanguageListModel();

		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;
		[JsonIgnore]
		public readonly ListenerProperty<string> LanguageId;
		/// <summary>
		/// The standardized language tag, see https://en.wikipedia.org/wiki/Language_localisation#Language_tags_and_codes
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> LanguageTag;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;
		[JsonIgnore]
		public readonly ListenerProperty<string[]> Tags;
		[JsonIgnore]
		public LanguageListModel Language { get { return language; } }

		public LanguageDatabaseModel()
		{
			SaveType = SaveTypes.LanguageDatabase;

			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			LanguageId = new ListenerProperty<string>(value => languageId = value, () => languageId);
			LanguageTag = new ListenerProperty<string>(value => languageTag = value, () => languageTag);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Tags = new ListenerProperty<string[]>(value => tags = value, () => tags);
		}
	}
}