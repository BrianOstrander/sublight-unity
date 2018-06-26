namespace LunraGames.SpaceFarm
{
	public struct UniversePositionRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static UniversePositionRequest Request(UniversePosition position)
		{
			return new UniversePositionRequest(States.Request, position);
		}

		public readonly States State;
		public readonly UniversePosition Position;

		public UniversePositionRequest(States state, UniversePosition position)
		{
			State = state;
			Position = position;
		}

		public UniversePositionRequest Duplicate(States state = States.Unknown)
		{
			return new UniversePositionRequest(
				state == States.Unknown ? State : state,
				Position
			);
		}

		public override string ToString()
		{
			return "State: " + State + ", Position: " + Position;
		}
	}
}