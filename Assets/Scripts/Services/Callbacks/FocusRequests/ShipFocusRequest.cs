using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	public class ShipFocusRequest : FocusRequest
	{
		public enum Views
		{
			Unknown = 0,
			SlotEditor = 10
		}

		public static ShipFocusRequest SlotEditor(
			States state = States.Request
		)
		{
			return new ShipFocusRequest(Views.SlotEditor, state);
		}

		public override Focuses Focus { get { return Focuses.Ship; } }

		[JsonProperty] public readonly Views View;

		[JsonConstructor]
		ShipFocusRequest(
			Views view,
			States state = States.Request
		) : base(state)
		{
			View = view;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new ShipFocusRequest(
				View,
				state == States.Unknown ? State : state
			);
		}
	}
}