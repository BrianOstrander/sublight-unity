namespace LunraGames.SpaceFarm
{
	public class BodyFocusRequest : FocusRequest
	{
		public BodyFocusRequest(States state = States.Request) : base(Focuses.Body, state) { }

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new BodyFocusRequest(
				state == States.Unknown ? State : state
			);
		}
	}
}