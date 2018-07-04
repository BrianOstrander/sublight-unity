namespace LunraGames.SpaceFarm
{
	public class GalaxyFocusRequest : FocusRequest
	{
		public GalaxyFocusRequest(States state = States.Request) : base(Focuses.Galaxy, state) { }

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new GalaxyFocusRequest(
				state == States.Unknown ? State : state
			);
		}
	}
}