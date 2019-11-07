/*
using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class EdgeEntryModel : Model
	{
		[JsonProperty] string entryId;

		[JsonIgnore]
		public readonly ListenerProperty<string> EntryId;

		public EdgeEntryModel()
		{
			EntryId = new ListenerProperty<string>(value => entryId = value, () => entryId);
		}
	}
}
*/