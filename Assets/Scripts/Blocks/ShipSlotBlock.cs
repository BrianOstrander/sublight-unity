using System;

namespace LunraGames.SpaceFarm
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