using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public struct SpeedRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static SpeedRequest PauseRequest { get { return new SpeedRequest(States.Request, 0f); } }
		public static SpeedRequest PlayRequest { get { return new SpeedRequest(States.Request, 1f); } }
		public static SpeedRequest FastRequest { get { return new SpeedRequest(States.Request, 2f); } }
		public static SpeedRequest FastFastRequest { get { return new SpeedRequest(States.Request, 4f); } }

		public States State;
		public float Speed;

		public SpeedRequest(States state, float speed)
		{
			State = state;
			Speed = speed;
		}

		public SpeedRequest Duplicate(States state = States.Unknown)
		{
			return new SpeedRequest(
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