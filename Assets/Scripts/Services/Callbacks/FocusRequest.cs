using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public abstract class FocusRequest
	{
		public enum Focuses
		{
			Unknown = 0,
			// TODO: Merge the below v (these should be views within the focus!)
			Galaxy = 10,
			Systems = 20,
			SystemBodies = 30,
			Body = 40,
			// ---------------
			Encounter = 50,
			Ship = 60,
			Encyclopedia = 70
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