using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InteractedInventoryReferenceModel : Model
	{
		[JsonProperty] string inventoryId;
		[JsonProperty] int timesSeen;
		[JsonProperty] int timesSlotted;
		[JsonProperty] DateTime lastSeen = DateTime.MinValue;

		[JsonIgnore]
		public readonly ListenerProperty<string> InventoryId;
		[JsonIgnore]
		public readonly ListenerProperty<int> TimesSeen;
		[JsonIgnore]
		public readonly ListenerProperty<int> TimesSlotted;
		[JsonIgnore]
		public readonly ListenerProperty<DateTime> LastSeen;

		public InteractedInventoryReferenceModel()
		{
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
			TimesSeen = new ListenerProperty<int>(value => timesSeen = value, () => timesSeen);
			TimesSlotted = new ListenerProperty<int>(value => timesSlotted = value, () => timesSlotted);
			LastSeen = new ListenerProperty<DateTime>(value => lastSeen = value, () => lastSeen);
		}
	}
}