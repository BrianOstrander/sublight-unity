using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	public class BodyFocusRequest : FocusRequest
	{
		public enum Views
		{
			Unknown = 0,
			BodyHook = 10
		}

		public static BodyFocusRequest BodyHook(
			UniversePosition system,
			int body,
			States state = States.Request
		)
		{
			return new BodyFocusRequest(Views.BodyHook, system, body, state);
		}

		public override Focuses Focus { get { return Focuses.Body; } }

		[JsonProperty] public readonly Views View;
		[JsonProperty] public readonly UniversePosition System;
		[JsonProperty] public readonly int Body;

		[JsonConstructor]
		BodyFocusRequest(
			Views view,
			UniversePosition system,
			int body,
			States state = States.Request
		) : base(state) 
		{
			View = view;
			System = system;
			Body = body;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new BodyFocusRequest(
				View,
				System,
				Body,
				state == States.Unknown ? State : state
			);
		}
	}
}