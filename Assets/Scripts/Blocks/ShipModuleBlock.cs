using System;

namespace LunraGames.SpaceFarm
{
	public struct ShipModuleBlock
	{
		public string Name;
		public string Description;
		public string ButtonText;
		public bool IsSlotted;
		public bool CurrentlySlotted;
		public Action Click;
	}
}