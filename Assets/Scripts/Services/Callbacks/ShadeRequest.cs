namespace LunraGames.SubLight
{
	public struct ShadeRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static ShadeRequest Shade { get { return new ShadeRequest(States.Request, true); } }
		public static ShadeRequest UnShade { get { return new ShadeRequest(States.Request, false); } }

		public readonly States State;
		public readonly bool IsShaded;

		public ShadeRequest(States state, bool isShaded)
		{
			State = state;
			IsShaded = isShaded;
		}

		public ShadeRequest Duplicate(States state = States.Unknown, bool? isShaded = null)
		{
			return new ShadeRequest(
				state == States.Unknown ? State : state,
				isShaded.HasValue ? isShaded.Value : IsShaded
			);
		}
	}
}