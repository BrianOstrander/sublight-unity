using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	public class EncyclopediaFocusRequest : FocusRequest
	{
		public enum Views
		{
			Unknown = 0,
			Home = 10
		}

		public static EncyclopediaFocusRequest Home(
			States state = States.Request
		)
		{
			return new EncyclopediaFocusRequest(Views.Home, state);
		}

		public override Focuses Focus { get { return Focuses.Encyclopedia; } }

		[JsonProperty] public readonly Views View;

		[JsonConstructor]
		EncyclopediaFocusRequest(
			Views view,
			States state = States.Request
		) : base(state)
		{
			View = view;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new EncyclopediaFocusRequest(
				View,
				state == States.Unknown ? State : state
			);
		}
	}
}