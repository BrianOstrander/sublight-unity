using UnityEngine;
using System;

namespace LunraGames.SubLight
{
	public enum TweenTransitions
	{
		Unknown = 0,
		NoChange = 10,
		ToLower = 20,
		ToHigher = 30,
		PingPongToLower = 40,
		PingPongToHigher = 50
	}

	public enum TweenStates
	{
		Unknown = 0,
		Active = 10,
		Complete = 20
	}

	public static class TweenBlock
	{
		public static TweenBlock<float> Zero { get { return CreateInstant(0f, 0f); } }

		public static TweenBlock<float> Create(float begin, float end)
		{
			return Create(begin, end, begin, 0f);
		}

		public static TweenBlock<float> Create(float begin, float end, float current, float progress)
		{
			var transition = TweenTransitions.Unknown;
			if (Mathf.Approximately(begin, end)) transition = TweenTransitions.NoChange;
			else if (begin < end) transition = TweenTransitions.ToHigher;
			else transition = TweenTransitions.ToLower;
			return new TweenBlock<float>(begin, end, float.NaN, current, progress, transition);
		}

		public static TweenBlock<float> CreateInstant(float begin, float end)
		{
			var transition = TweenTransitions.Unknown;
			if (Mathf.Approximately(begin, end)) transition = TweenTransitions.NoChange;
			else if (begin < end) transition = TweenTransitions.ToHigher;
			else transition = TweenTransitions.ToLower;
			return new TweenBlock<float>(begin, end, float.NaN, end, 1f, transition);
		}

		public static TweenBlock<float> CreatePingPong(float beginEnd, float pingPong)
		{
			return CreatePingPong(beginEnd, pingPong, beginEnd, 0f);
		}

		public static TweenBlock<float> CreatePingPong(float beginEnd, float pingPong, float current, float progress)
		{
			var transition = TweenTransitions.Unknown;
			if (Mathf.Approximately(beginEnd, pingPong)) transition = TweenTransitions.NoChange;
			else if (beginEnd < pingPong) transition = TweenTransitions.PingPongToHigher;
			else transition = TweenTransitions.PingPongToLower;
			return new TweenBlock<float>(beginEnd, beginEnd, pingPong, current, progress, transition);
		}
	}

	public struct TweenBlock<T>
	{
		/// <summary>
		/// The begin value.
		/// </summary>
		public readonly T Begin;
		/// <summary>
		/// The end value, will be the same as Begin if we're ping-ponging.
		/// </summary>
		public readonly T End;
		/// <summary>
		/// The ping pong value.
		/// </summary>
		public readonly T PingPong;
		public readonly T Current;
		public readonly float Progress;
		public readonly TweenTransitions Transition;
		public readonly TweenStates State;

		public readonly bool IsPingPong;

		public TweenBlock(T begin, T end, T pingPong, T current, float progress, TweenTransitions transition)
		{
			Begin = begin;
			End = end;
			PingPong = pingPong;
			Current = current;
			Progress = progress;
			Transition = transition;
			IsPingPong = transition == TweenTransitions.PingPongToLower || transition == TweenTransitions.PingPongToHigher;

			if (progress < 0f || 1f < progress)
			{
				Debug.LogError("Unexpected progress value of " + progress + ", unexpected behaviour may occur");
				State = TweenStates.Unknown;
			}
			else if (Mathf.Approximately(progress, 1f)) State = TweenStates.Complete;
			else State = TweenStates.Active;
		}

		public TweenBlock<T> Duplicate(T current, float progress)
		{
			return new TweenBlock<T>(Begin, End, PingPong, current, progress, Transition);
		}

		public TweenBlock<T> DuplicateNoChange()
		{
			return new TweenBlock<T>(Begin, End, PingPong, End, 1f, TweenTransitions.NoChange);
		}
	}
}