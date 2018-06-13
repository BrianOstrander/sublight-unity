using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public struct SpeedChange
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static SpeedChange PauseRequest { get { return new SpeedChange(States.Request, 0f); } }
		public static SpeedChange PlayRequest { get { return new SpeedChange(States.Request, 1f); } }
		public static SpeedChange FastRequest { get { return new SpeedChange(States.Request, 2f); } }
		public static SpeedChange FastFastRequest { get { return new SpeedChange(States.Request, 4f); } }

		public States State;
		public float Speed;

		public SpeedChange(States state, float speed)
		{
			State = state;
			Speed = speed;
		}

		public SpeedChange Duplicate(States state = States.Unknown)
		{
			return new SpeedChange(
				state == States.Unknown ? State : state,
				Speed
			);
		}

		public override string ToString()
		{
			return "State: " + State + ", Speed: " + Speed;
		}
	}
}