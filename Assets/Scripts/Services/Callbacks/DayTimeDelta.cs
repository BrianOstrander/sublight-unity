namespace LunraGames.SubLight
{
	public struct DayTimeDelta
	{
		public readonly DayTime Current;
		public readonly DayTime Previous;
		public readonly DayTime Delta;
		public float TotalTime { get { return Delta.TotalTime; } }

		public DayTimeDelta(DayTime current, DayTime previous)
		{
			Current = current;
			Previous = previous;
			Delta = DayTime.Elapsed(current, previous);
		}
	}
}