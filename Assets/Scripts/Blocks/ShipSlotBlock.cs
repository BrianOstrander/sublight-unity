using System;

namespace LunraGames.SubLight
{
	public struct ShipSlotBlock
	{
		public string SlotName;
		public string TypeName;
		public string ItemName;
		public bool IsSelected;
		public Action Click;
	}
}