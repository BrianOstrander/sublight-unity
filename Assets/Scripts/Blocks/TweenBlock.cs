using UnityEngine;
using System;

namespace LunraGames.SubLight
{
	public enum TweenTransitions
	{
		Unknown = 0,
		NoChange = 10,
		ToLower = 20,
		ToHigher = 30
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
			return new TweenBlock<float>(begin, end, current, progress, transition);
		}

		public static TweenBlock<float> CreateInstant(float begin, float end)
		{
			var transition = TweenTransitions.Unknown;
			if (Mathf.Approximately(begin, end)) transition = TweenTransitions.NoChange;
			else if (begin < end) transition = TweenTransitions.ToHigher;
			else transition = TweenTransitions.ToLower;
			return new TweenBlock<float>(begin, end, end, 1f, transition);
		}
	}

	[Serializable]
	public struct TweenBlock<T>
	{
		public readonly T Begin;
		public readonly T End;
		public readonly T Current;
		public readonly float Progress;
		public readonly TweenTransitions Transition;
		public readonly TweenStates State;

		public TweenBlock(T begin, T end, T current, float progress, TweenTransitions transition)
		{
			Begin = begin;
			End = end;
			Current = current;
			Progress = progress;
			Transition = transition;

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
			return new TweenBlock<T>(Begin, End, current, progress, Transition);
		}
	}
}