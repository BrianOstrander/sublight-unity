namespace LunraGames.SpaceFarm
{
	public struct DayTimeDelta
	{
		public DayTime Current { get; private set; }
		public DayTime Previous { get; private set; }
		public DayTime Delta { get; private set; }
		public float TotalTime { get { return Delta.TotalTime; } }

		public DayTimeDelta(DayTime current, DayTime previous)
		{
			Current = current;
			Previous = previous;
			Delta = DayTime.DayTimeElapsed(current, previous);
		}
	}
}