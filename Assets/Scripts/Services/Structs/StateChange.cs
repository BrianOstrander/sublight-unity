namespace LunraGames.SpaceFarm
{
	public struct StateChange
	{
		public StateMachine.States State { get; private set; }
		public StateMachine.Events Event { get; private set; }

		public StateChange(StateMachine.States state, StateMachine.Events stateEvent)
		{
			State = state;
			Event = stateEvent;
		}
	}
}