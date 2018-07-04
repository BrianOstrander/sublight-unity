namespace LunraGames.SpaceFarm
{
	public class GalaxyFocusRequest : FocusRequest
	{
		public override Focuses Focus { get { return Focuses.Galaxy; } }

		public GalaxyFocusRequest(States state = States.Request) : base(state) { }

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new GalaxyFocusRequest(
				state == States.Unknown ? State : state
			);
		}
	}
}