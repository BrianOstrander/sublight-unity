using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public class StateMachine
	{
		public enum States
		{
			Unknown,
			Initialize,
			Home,
			Game
		}

		public enum Events
		{
			Unknown,
			Begin,
			Idle,
			End
		}

		interface IEntry
		{
			States State { get; }
			Events Event { get; }
			bool IsRepeating { get; }
			bool Trigger();
		}

		abstract class Entry : IEntry
		{
			public States State { get; protected set; }
			public Events Event { get; protected set; }
			public bool IsRepeating { get; protected set; }
			public abstract bool Trigger();
		}

		class BlockingEntry : Entry
		{
			Action<Action> action;
			bool? isDone;

			public BlockingEntry(Action<Action> action, States state, Events stateEvent)
			{
				this.action = action;
				State = state;
				Event = stateEvent;
				IsRepeating = false;
			}

			public override bool Trigger()
			{
				if (!isDone.HasValue)
				{
					isDone = false;
					action(OnDone);
				}
				return isDone.Value;
			}

			void OnDone()
			{
				isDone = true;
			}
		}

		class NonBlockingEntry : Entry
		{
			Action action;

			public NonBlockingEntry(Action action, States state, Events stateEvent, bool repeating)
			{
				this.action = action;
				State = state;
				Event = stateEvent;
				IsRepeating = repeating;
			}

			public override bool Trigger()
			{
				action();
				return true;
			}
		}

		IState[] stateEntries;

		IState currentState;
		IState nextState;
		Events currentEvent;
		List<IEntry> queued = new List<IEntry>();
		List<IEntry> entries = new List<IEntry>();

		public States CurrentState 
		{ 
			get 
			{
				if (currentState == null) return States.Unknown;
				return currentState.HandledState;
			}
		}

		public Events CurrentEvent
		{
			get
			{
				if (currentState == null) return Events.Unknown;
				return currentEvent;
			}
		}

		public bool Is(States isState, Events isEvent) { return isState == CurrentState && isEvent == CurrentEvent; }

		public StateMachine(Heartbeat heartbeat, params IState[] states)
		{
			stateEntries = states;
			heartbeat.Update += Update;
		}

		void Update(float delta)
		{
			entries.AddRange(queued);
			queued.Clear();
			var persisted = new List<IEntry>();
			var isBlocked = false;
			foreach (var entry in entries)
			{
				if (isBlocked)
				{
					persisted.Add(entry);
					continue;
				}

				if (entry.State != currentState.HandledState)
				{
					if (entry.State == nextState.HandledState) persisted.Add(entry);
					continue;
				}

				if (entry.IsRepeating) persisted.Add(entry);
				try 
				{
					isBlocked = !entry.Trigger();
				}
				catch (Exception exception) { Debug.LogException(exception); }
				finally
				{
					if (!entry.IsRepeating && isBlocked) persisted.Add(entry);
				}
			}
			entries = persisted;

			// We can't change states until we're unblocked.
			if (isBlocked) return;

			if (nextState.HandledState != currentState.HandledState)
			{
				if (currentEvent == Events.Idle)
				{
					// We transitioning states, but we haven't completed yet.
					SetState(currentState, Events.End);
				}
				else if (currentEvent == Events.End)
				{
					// We already completed last frame, so now we'll be starting.
					SetState(nextState, Events.Begin);
				}
			}
			else
			{
				if (currentEvent == Events.Begin)
				{
					// We already started last frame, so now we go to idle
					SetState(currentState, Events.Idle);
				}
			}
		}

		void SetState(IState state, Events stateEvent)
		{
			currentState = state;
			currentEvent = stateEvent;

			currentState.UpdateState(currentState.HandledState, currentEvent);
		}

		IState GetState<P>(P payload)
			where P : class, IStatePayload
		{
			foreach (var state in stateEntries)
			{
				if (state.AcceptsPayload(payload)) return state;
			}
			return null;
		}

		public void Push(Action action, bool repeating)
		{
			Push(action, currentState.HandledState, currentEvent, repeating);
		}

		public void PushBlocking(Action<Action> action)
		{
			PushBlocking(action, currentState.HandledState, currentEvent);
		}

		public void Push(Action action, States state, Events stateEvent, bool repeating)
		{
			if (action == null) throw new ArgumentNullException("action");
			if (state == States.Unknown) throw new ArgumentException("Cannot bind to States.Unknown");
			if (stateEvent == Events.Unknown) throw new ArgumentException("Cannot bind to Events.Unknown");

			queued.Add(new NonBlockingEntry(action, state, stateEvent, repeating));
		}

		public void PushBlocking(Action<Action> action, States state, Events stateEvent)
		{
			if (action == null) throw new ArgumentNullException("action");
			if (state == States.Unknown) throw new ArgumentException("Cannot bind to States.Unknown");
			if (stateEvent == Events.Unknown) throw new ArgumentException("Cannot bind to Events.Unknown");

			queued.Add(new BlockingEntry(action, state, stateEvent));
		}

		/// <summary>
		/// Requests to transition to the first state that accepts the specified payload.
		/// </summary>
		/// <param name="payload">Payload.</param>
		/// <typeparam name="P">Payload type.</typeparam>
		public void RequestState<P>(P payload = null)
			where P : class, IStatePayload, new()
		{
			payload = payload ?? new P();
			var handlingState = GetState(payload);
			if (handlingState == null) throw new Exception("Cannot find a handler for payload of type " + typeof(P));
			var state = handlingState.HandledState;

			if (state == States.Unknown) throw new ArgumentException("Cannot switch to an Unknown state");
			if (currentState != null && nextState != null)
			{
				if (currentState.HandledState == state || nextState.HandledState == state) return;
				if (currentState.HandledState != nextState.HandledState) throw new NotImplementedException("Cannot switch to another state while already transitioning");
			}

			nextState = handlingState;
			nextState.Initialize(payload);
			if (currentState == null) SetState(nextState, Events.Begin);

		}
	}
}