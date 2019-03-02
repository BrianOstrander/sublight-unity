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

		protected override UniversePosition PositionInUniverse { get { return beginPosition; } }

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

		UniversePosition beginPosition;
		UniversePosition endPosition;
		int? willLeapToTransitCount;

		TransitHistoryEntry GetHistory()
		{
			return Model.TransitHistory.Peek(e => e.TransitCount == TransitCount);
		}

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

			ScaleModel.Transform.Changed += OnScaleTransform;

			TransitCount = -1;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Context.TransitState.Changed -= OnTransitState;
			Model.TransitHistory.Stack.Changed -= OnTransitHistory;

			ScaleModel.Transform.Changed -= OnScaleTransform;
		}

		protected override void OnShowView()
		{
			OnScaleTransformForced(ScaleModel.Transform.Value);
			CalculateDistances();
		}

		void CalculateDistances()
		{
			var historyEntry = GetHistory();

			if (!historyEntry.IsValid)
			{
				View.SetDistance(1f, 1f);
				return;
			}

			var beginNormal = 1f;
			var endNormal = 1f;

			var latestEntry = Model.TransitHistory.Peek();

			var maximumDistance = Model.Context.TransitHistoryLineDistance.Value;

			if (Mathf.Approximately(maximumDistance, 0f))
			{
				View.SetDistance(beginNormal, endNormal);
				return;
			}

			var totalDistance = latestEntry.TotalTransitDistance;

			switch (Model.Context.TransitState.Value.State)
			{
				case TransitState.States.Active:
					totalDistance += Model.Context.TransitState.Value.DistanceElapsed;
					break;
			}

			var endDistance = totalDistance - historyEntry.TotalTransitDistance;
			var beginDistance = endDistance + historyEntry.TransitDistance;

			if (endDistance < maximumDistance)
			{
				endNormal = endDistance / maximumDistance;

				if (beginDistance < maximumDistance)
				{
					beginNormal = beginDistance / maximumDistance;
				}
			}

			View.SetDistance(beginNormal, endNormal);
		}

		void LeapAhead()
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
			UniversePosition begin,
			UniversePosition end,
			int transitCount,
			States state
		)
		{
			beginPosition = begin;
			endPosition = end;
			TransitCount = transitCount;
			State = state;
		}

		#region Events
		void OnTransitState(TransitState transitState)
		{
			if (IsLast && transitState.State == TransitState.States.Request)
			{
				var first = this;
				while (first.Next != null) first = first.Next;
				willLeapToTransitCount = Mathf.Max(1, first.TransitCount + 1);
				TransitCount = -1;
			}
			else if (willLeapToTransitCount.HasValue && transitState.State == TransitState.States.Complete)
			{
				var targetCount = willLeapToTransitCount.Value;
				willLeapToTransitCount = null;

				LeapAhead();

				SetTransit(
					transitState.BeginSystem.Position.Value,
					transitState.EndSystem.Position.Value,
					targetCount,
					States.Assigned
				);

				ShowViewInstant();
			}

			if (View.Visible && transitState.State == TransitState.States.Active && 0 < TransitCount)
			{
				CalculateDistances();
			}
		}

		void OnTransitHistory(TransitHistoryEntry[] entries)
		{
			//Debug.Log("homm called here");
			//if (historyEntry.IsValid || TransitCount == 0) return;

			//SetTransitHistory(entries.FirstOrDefault(e => e.TransitCount == TransitCount));
		}

		void OnScaleTransform(UniverseTransform transform)
		{
			if (!View.Visible) return;
			OnScaleTransformForced(transform);
		}

		void OnScaleTransformForced(UniverseTransform transform)
		{
			SetGrid(transform.UnityOrigin, transform.UnityRadius);

			View.SetPoints(
				transform.GetUnityPosition(beginPosition),
				transform.GetUnityPosition(endPosition)
			);
		}
		#endregion
	}
}