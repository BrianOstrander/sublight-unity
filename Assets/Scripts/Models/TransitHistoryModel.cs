using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class TransitHistoryModel : Model
	{
		#region Serialized
		[JsonProperty] TransitHistoryEntry[] stack = new TransitHistoryEntry[0];
		readonly ListenerProperty<TransitHistoryEntry[]> stackListener;
		[JsonIgnore] public readonly ReadonlyProperty<TransitHistoryEntry[]> Stack;
		#endregion

		public TransitHistoryModel()
		{
			Stack = new ReadonlyProperty<TransitHistoryEntry[]>(value => stack = value, () => stack, out stackListener);
		}

		public void Push(TransitHistoryEntry entry)
		{
			if (0 < stack.Length && stack.First().TransitCount != (entry.TransitCount - 1)) Debug.LogError("Pushing a non-sequencial TransitHistoryEntry to the stack, unperdictable behaviour may occur");

			stackListener.Value = stack.Prepend(entry).ToArray();
		}

		public TransitHistoryEntry Peek()
		{
			if (stack.Length == 0) Debug.LogError("Trying to peek at an empty stack");

			return Stack.Value.FirstOrDefault();
		}

		public TransitHistoryEntry Peek(int offset)
		{
			if (stack.Length == 0)
			{
				Debug.LogError("Trying to peek at an empty stack");
				return default(TransitHistoryEntry);
			}
			if (offset < 0) Debug.LogError("Trying to peek with offset less than zero");
			if (stack.Length <= offset) Debug.LogError("Trying to peek with an offset greater than or equal to the stack length");

			return Stack.Value[Mathf.Clamp(offset, 0, stack.Length - 1)];
		}

		public TransitHistoryEntry Peek(string id)
		{
			if (string.IsNullOrEmpty(id)) Debug.LogError("Trying to peek with null or empty id");
			return Stack.Value.FirstOrDefault(t => t.Id == id);
		}

		public TransitHistoryEntry Peek(Func<TransitHistoryEntry, bool> predicate)
		{
			return Stack.Value.FirstOrDefault(predicate);
		}

		[JsonIgnore] public int Count { get { return Stack.Value.Length; } }
	}
}