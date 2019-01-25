using System;

namespace LunraGames.SubLight
{
	public class StateMachineWrapper
	{
		Type type;
		StateMachine stateMachine;

		public StateMachineWrapper(StateMachine stateMachine, Type type)
		{
			this.type = type;
			this.stateMachine = stateMachine;
		}

		#region StateMachine Helper Methods
		public void Push(
			Action action,
			string description,
			bool repeating = false
		)
		{
			App.SM.Push(action, type, description, repeating);
		}

		public void PushBlocking(Action<Action> action, string description)
		{
			App.SM.PushBlocking(action, type, description);
		}

		public void PushBlocking(Action action, Func<bool> condition, string description)
		{
			App.SM.PushBlocking(action, condition, type, description);
		}
		#endregion
	}
}