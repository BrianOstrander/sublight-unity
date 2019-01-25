using System;

namespace LunraGames.SubLight
{
	public interface IStatePayload {}

	public interface IState
	{
		StateMachine.States HandledState { get; }
		Type PayloadType { get; }
		bool AcceptsPayload(object payload, bool throws = false);
		void Initialize(object payload);
		void UpdateState(StateMachine.States state, StateMachine.Events stateEvent, object payload);
	}

	public interface IStateTyped<P> : IState
		where P : class
	{
		P Payload { get; }
	}

	public abstract class BaseState : IState
	{
		public abstract StateMachine.States HandledState { get; }
		public abstract Type PayloadType { get; }

		public object PayloadObject { get; set; }

		public virtual bool AcceptsPayload(object payload, bool throws = false)
		{
			Exception exception = null;
			if (payload == null) exception = new ArgumentNullException("payload");
			else if (payload.GetType() != PayloadType) exception = new ArgumentException("payload of type " + payload.GetType() + " is not supported by this state");

			var accepts = exception == null;
			if (throws && !accepts) throw exception;
			return accepts;
		}

		public virtual void Initialize(object payload)
		{
			AcceptsPayload(payload, true);
			PayloadObject = payload;
		}

		public virtual void UpdateState(StateMachine.States state, StateMachine.Events stateEvent, object payload)
		{
			if (state != HandledState) return;
			switch (stateEvent)
			{
				case StateMachine.Events.Begin:
					Begin();
					break;
				case StateMachine.Events.Idle:
					Idle();
					break;
				case StateMachine.Events.End:
					End();
					break;
			}
			App.Log("State is now " + state + "." + stateEvent + " - Payload " + payload.GetType(), LogTypes.StateMachine);
			App.Callbacks.StateChange(new StateChange(state, stateEvent, payload));
		}

		protected virtual void Begin() { }
		protected virtual void End() { }
		protected virtual void Idle() { }

		#region StateMachine Helper Methods
		protected void Push(
			Action action,
			string description,
			bool repeating = false
		)
		{
			App.SM.Push(action, GetType(), description, repeating);
		}

		protected void PushBlocking(Action<Action> action, string description)
		{
			App.SM.PushBlocking(action, GetType(), description);
		}

		protected void PushBlocking(Action action, Func<bool> condition, string description)
		{
			App.SM.PushBlocking(action, condition, GetType(), description);
		}
		#endregion
	}

	public abstract class State<P> : BaseState, IStateTyped<P>
		where P : class, IStatePayload
	{
		public override Type PayloadType { get { return typeof(P); } }
		public P Payload
		{
			get { return PayloadObject as P; }
			set { PayloadObject = value; }
		}
	}
}