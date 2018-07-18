using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	public class BodyFocusRequest : FocusRequest
	{
		public enum Views
		{
			Unknown = 0,
			ProbeList = 10,
			ProbeDetail = 20,
			Probing = 30,
			BodyHook = 40
		}

		public static BodyFocusRequest ProbeList(
			UniversePosition system,
			int body,
			States state = States.Request
		)
		{
			return new BodyFocusRequest(Views.ProbeList, system, body, state: state);
		}

		public static BodyFocusRequest ProbeDetail(
			UniversePosition system,
			int body,
			string probe,
			States state = States.Request
		)
		{
			return new BodyFocusRequest(Views.ProbeDetail, system, body, probe, state);
		}

		public static BodyFocusRequest Probing(
			UniversePosition system,
			int body,
			string probe,
			States state = States.Request
		)
		{
			return new BodyFocusRequest(Views.Probing, system, body, probe, state);
		}

		public static BodyFocusRequest BodyHook(
			UniversePosition system,
			int body,
			States state = States.Request
		)
		{
			return new BodyFocusRequest(Views.BodyHook, system, body, state: state);
		}

		public override Focuses Focus { get { return Focuses.Body; } }

		[JsonProperty] public readonly Views View;
		[JsonProperty] public readonly UniversePosition System;
		[JsonProperty] public readonly int Body;
		[JsonProperty] public readonly string Probe;

		[JsonConstructor]
		BodyFocusRequest(
			Views view,
			UniversePosition system,
			int body,
			string probe = null,
			States state = States.Request
		) : base(state) 
		{
			View = view;
			System = system;
			Body = body;
			Probe = probe;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new BodyFocusRequest(
				View,
				System,
				Body,
				Probe,
				state == States.Unknown ? State : state
			);
		}
	}
}