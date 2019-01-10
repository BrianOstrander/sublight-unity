namespace LunraGames.SubLight
{
	// I really can't remember why I made this class... I guess it's to let other classes know what the GridPresenter is up to?
	public struct GridInputRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public enum Transforms
		{
			Unknown = 0,
			Input = 10,
			Animation = 20,
			Settle = 30
		}

		public readonly States State;
		public readonly Transforms Transform;

		public GridInputRequest(
			States state,
			Transforms transform
		)
		{
			State = state;
			Transform = transform;
		}
	}
}