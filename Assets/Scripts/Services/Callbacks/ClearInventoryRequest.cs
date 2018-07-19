namespace LunraGames.SpaceFarm
{
	public struct ClearInventoryRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10
		}

		public static ClearInventoryRequest Request() { return new ClearInventoryRequest(States.Request); }

		public readonly States State;

		public ClearInventoryRequest(States state)
		{
			State = state;
		}

		public ClearInventoryRequest Duplicate(States state = States.Unknown)
		{
			return new ClearInventoryRequest(
				state == States.Unknown ? State : state
			);
		}
	}
}