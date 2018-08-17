using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	/// <summary>
	/// A connection between a slot and an item.
	/// </summary>
	[Serializable]
	public struct SlotEdge
	{
		[JsonProperty] public readonly string ParentSlotId;
		[JsonProperty] public readonly string ItemInstanceId;

		[JsonIgnore]
		public bool IsEmpty { get { return string.IsNullOrEmpty(ParentSlotId) || string.IsNullOrEmpty(ItemInstanceId); } }

		public SlotEdge(string slotId, string itemInstanceId)
		{
			ParentSlotId = slotId;
			ItemInstanceId = itemInstanceId;
		}

		public override string ToString()
		{
			return "SlotEdge: ParentSlotId: " + ParentSlotId + " ItemInstanceId: " + ItemInstanceId;
		}
	}
}