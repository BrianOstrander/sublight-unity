using UnityEngine;

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

		public const float PauseSpeed = 0f;
		public const float PlaySpeed = 1f;
		public const float FastSpeed = 2f;
		public const float FastFastSpeed = 4f;

		public static SpeedRequest PauseRequest { get { return new SpeedRequest(States.Request, PauseSpeed); } }
		public static SpeedRequest PlayRequest { get { return new SpeedRequest(States.Request, PlaySpeed); } }
		public static SpeedRequest FastRequest { get { return new SpeedRequest(States.Request, FastSpeed); } }
		public static SpeedRequest FastFastRequest { get { return new SpeedRequest(States.Request, FastFastSpeed); } }

		public readonly States State;
		public readonly float Speed;

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

		public int Index
		{
			get
			{
				if (Mathf.Approximately(Speed, PauseSpeed)) return 0;
				if (Mathf.Approximately(Speed, PlaySpeed)) return 1;
				if (Mathf.Approximately(Speed, FastSpeed)) return 2;
				if (Mathf.Approximately(Speed, FastFastSpeed)) return 3;
				Debug.LogWarning("Unknown speed: " + Speed);
				return 0;
			}
		}

		public override string ToString()
		{
			return "State: " + State + ", Speed: " + Speed;
		}
	}
}