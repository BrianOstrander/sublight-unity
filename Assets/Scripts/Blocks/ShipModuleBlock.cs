using System;

namespace LunraGames.SubLight
{
	public struct ShipModuleBlock
	{
		public string Name;
		public string Description;
		public string ButtonText;
		public bool IsSlotted;
		public bool CurrentlySlotted;
		public Action AssignClick;
		public bool IsRemovable;
		public Action RemoveClick;
	}
}