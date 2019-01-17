//using Newtonsoft.Json;

//namespace LunraGames.SubLight.Models
//{
//	public class DialogEntryModel : Model
//	{
//		[JsonProperty] string dialogId;
//		[JsonProperty] EncounterEvents.Types encounterEvent;
//		[JsonProperty] bool isHalting;

//		[JsonIgnore]
//		public readonly ListenerProperty<string> EventId;
//		[JsonIgnore]
//		public readonly ListenerProperty<EncounterEvents.Types> EncounterEvent;
//		[JsonIgnore]
//		public readonly ListenerProperty<bool> IsHalting;

//		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
//		[JsonIgnore]
//		public KeyValueListModel KeyValues { get { return keyValues; } }

//		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default(true);
//		[JsonIgnore]
//		public ValueFilterModel Filtering { get { return filtering; } }

//		public EncounterEventEntryModel()
//		{
//			EventId = new ListenerProperty<string>(value => eventId = value, () => eventId);
//			EncounterEvent = new ListenerProperty<EncounterEvents.Types>(value => encounterEvent = value, () => encounterEvent);
//			IsHalting = new ListenerProperty<bool>(value => isHalting = value, () => isHalting);
//		}
//	}
//}