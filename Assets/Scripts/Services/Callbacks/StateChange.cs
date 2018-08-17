namespace LunraGames.SubLight
{
	public struct StateChange
	{
		public readonly StateMachine.States State;
		public readonly StateMachine.Events Event;

		public StateChange(StateMachine.States state, StateMachine.Events stateEvent)
		{
			State = state;
			Event = stateEvent;
		}
	}
}