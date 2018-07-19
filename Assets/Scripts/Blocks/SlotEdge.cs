using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	/// <summary>
	/// A connection between a slot and an item.
	/// </summary>
	[Serializable]
	public struct SlotEdge
	{
		[JsonProperty] public readonly string SlotId;
		[JsonProperty] public readonly string ItemInstanceId;

		[JsonIgnore]
		public bool IsEmpty { get { return string.IsNullOrEmpty(SlotId) || string.IsNullOrEmpty(ItemInstanceId); } }

		public SlotEdge(string slotId, string itemInstanceId)
		{
			SlotId = slotId;
			ItemInstanceId = itemInstanceId;
		}

		public override string ToString()
		{
			return "SlotEdge: SlotId: " + SlotId + " ItemInstanceId: " + ItemInstanceId;
		}
	}
}