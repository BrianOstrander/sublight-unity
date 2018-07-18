using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	[Serializable]
	public abstract class FocusRequest
	{
		public enum Focuses
		{
			Unknown = 0,
			Galaxy = 10,
			Systems = 20,
			SystemBodies = 30,
			Body = 40,
			Encounter = 50
		}
		
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		[JsonIgnore]
		public abstract Focuses Focus { get; }

		[JsonProperty] public readonly States State;

		protected FocusRequest(States state)
		{
			State = state;
		}

		public abstract FocusRequest Duplicate(States state = States.Unknown);

		public override string ToString()
		{
			return "Focus: " + Focus + "." + State;
		}
	}
}