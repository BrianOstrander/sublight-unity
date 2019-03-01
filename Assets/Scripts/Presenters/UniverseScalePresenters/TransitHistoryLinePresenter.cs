using System;
using System.Linq;

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
			/// The line is assigned to a valid transit history entry, though it
			/// may not be visible depending on the maximum distance defined.
			/// </summary>
			Assigned = 20
		}

		protected override UniversePosition PositionInUniverse { get { return previousPosition; } }

		protected override bool CanShow
		{
			get
			{
				switch (State)
				{
					case States.Assigned:
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

		public TransitHistoryLinePresenter GetFirst()
		{
			var result = this;
			while (result.Next != null) result = result.Next;

			return result;
		}

		UniversePosition previousPosition;
		UniversePosition currentPosition;
		bool willLeap;

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
			View.SetPoints(
				ScaleModel.Transform.Value.GetUnityPosition(previousPosition),
				ScaleModel.Transform.Value.GetUnityPosition(currentPosition)
			);
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

			var newLast = Next;

			newLast.Previous = null;

			var oldFirst = newLast;
			while (oldFirst.Next != null) oldFirst = oldFirst.Next;

			oldFirst.Next = this;
			Previous = oldFirst;
			Next = null;
		}

		public void SetTransit(
			UniversePosition previous,
			UniversePosition current,
			int transitCount,
			States state
		)
		{
			previousPosition = previous;
			currentPosition = current;
			TransitCount = transitCount;
			State = state;
		}

		#region Events
		void OnTransitState(TransitState transitState)
		{
			if (IsLast && transitState.State == TransitState.States.Request)
			{
				willLeap = true;
			}
			else if (willLeap && transitState.State == TransitState.States.Complete)
			{
				willLeap = false;

				LeapAhead();

				SetTransit(
					transitState.BeginSystem.Position.Value,
					transitState.EndSystem.Position.Value,
					Previous.TransitCount + 1,
					States.Assigned
				);

				ShowViewInstant();
			}
		}

		void OnTransitHistory(TransitHistoryEntry[] entries)
		{
			if (!willLeap) return;
			// TODO: I think I can remove this...
		}
		#endregion
	}
}