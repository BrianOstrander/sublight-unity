using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class TransitHistoryLinePresenter : UniverseScalePresenter<ITransitHistoryLineView>
	{
		public enum States
		{
			Unknown = 0,
			/// <summary>
			/// The line has never been assigned and should be hidden.
			/// </summary>
			Pooled = 10,
			/// <summary>
			/// The line has a valid begin, and an end equal to the ship's
			/// position, but no transit history corresponds with this line yet.
			/// </summary>
			Transit = 20,
			/// <summary>
			/// The line is assigned to a valid transit history entry, though it
			/// may not be visible depending on the maximum distance defined.
			/// </summary>
			Assigned = 30
		}

		protected override UniversePosition PositionInUniverse { get { return positionInUniverseBegin; } }

		protected override bool CanShow
		{
			get
			{
				switch (State)
				{
					case States.Assigned:
					case States.Transit:
						return true;
				}
				return false;
			}
		}

		public TransitHistoryLinePresenter Previous;
		public TransitHistoryLinePresenter Next;
		public int TransitCount { get; private set; }
		public States State { get; private set; }

		public bool IsFirst { get { return Next == null; } }
		public bool IsLast { get { return Previous == null; } }

		UniversePosition positionInUniverseBegin;
		UniversePosition positionInUniverseEnd;

		public TransitHistoryLinePresenter(
			GameModel model,
			UniverseScales scale,
			TransitHistoryLinePresenter previous = null
		) : base(model, scale)
		{
			Previous = previous;
			State = States.Pooled;

			Model.Context.TransitState.Changed += OnTransitState;
			Model.TransitHistory.Stack.Changed += OnTransitHistory;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Context.TransitState.Changed -= OnTransitState;
			Model.TransitHistory.Stack.Changed -= OnTransitHistory;
		}

		protected override void OnShowView()
		{

		}

		protected void LeapAhead()
		{
			if (Next == null)
			{
				Debug.LogError("Cannot leap first line ahead");
				return;
			}
			if (Previous != null)
			{
				Debug.LogError("Cannot leap a middle line ahead");
				return;
			}

			var firstLine = Next;
			while (firstLine.Next != null) firstLine = firstLine.Next;

			Next.Previous = null;
			firstLine.Next = this;
			Previous = firstLine;

			SetTransit(
				Previous.positionInUniverseEnd,
				Previous.positionInUniverseEnd,
				Previous.TransitCount + 1,
				States.Transit
			);
		}

		public void SetTransit(
			UniversePosition begin,
			UniversePosition end,
			int transitCount,
			States state
		)
		{
			positionInUniverseBegin = begin;
			positionInUniverseEnd = end;
			TransitCount = transitCount;
			State = state;
		}

		#region Events
		void OnTransitState(TransitState transitState)
		{
			if (!IsLast || transitState.State != TransitState.States.Request) return;

			LeapAhead();

			Debug.Log("do whatever now");
			//switch (View.TransitionState)
			//{
			//	case TransitionStates.Closed:
			//		ShowViewInstant()
			//}
		}

		void OnTransitHistory(TransitHistoryEntry[] entries)
		{
			if (!IsFirst) return;

			Debug.Log("todo set to assigned");
		}
		#endregion
	}
}