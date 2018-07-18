using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class FinalReportModel : Model
	{
		[JsonProperty] string encounter;
		[JsonProperty] UniversePosition system;
		[JsonProperty] int body;
		[JsonProperty] string summary;

		[JsonIgnore]
		public readonly ListenerProperty<string> Encounter;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> System;
		[JsonIgnore]
		public readonly ListenerProperty<int> Body;
		[JsonIgnore]
		public readonly ListenerProperty<string> Summary;

		public FinalReportModel()
		{
			Encounter = new ListenerProperty<string>(value => encounter = value, () => encounter);
			System = new ListenerProperty<UniversePosition>(value => system = value, () => system);
			Body = new ListenerProperty<int>(value => body = value, () => body);
			Summary = new ListenerProperty<string>(value => summary = value, () => summary);
		}
	}
}