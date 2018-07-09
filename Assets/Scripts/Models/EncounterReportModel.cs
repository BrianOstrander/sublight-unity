using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class EncounterReportModel : Model
	{
		[JsonProperty] string reportId;

		[JsonIgnore]
		public readonly ListenerProperty<string> ReportId;

		public EncounterReportModel()
		{
			ReportId = new ListenerProperty<string>(value => reportId = value, () => reportId);
		}
	}
}