namespace LunraGames.SpaceFarm
{
	public struct PlayState
	{
		public enum States
		{
			Unknown = 0,
			Paused = 10,
			Playing = 20
		}

		public static PlayState Paused { get { return new PlayState(States.Paused); } }
		public static PlayState Playing { get { return new PlayState(States.Playing); } }

		public readonly States State;

		public PlayState(States state)
		{
			State = state;
		}
	}
}