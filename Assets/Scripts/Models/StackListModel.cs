using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class StackListModel<T> : Model
	{
		public struct Entry
		{
			public readonly string Id;
			public readonly T Value;

			public Entry(string id, T value)
			{
				Id = id;
				Value = value;
			}
		}

		Entry[] entries = new Entry[0];
		readonly ListenerProperty<Entry[]> entriesListener;
		public readonly ReadonlyProperty<Entry[]> Entries;

		public StackListModel()
		{
			Entries = new ReadonlyProperty<Entry[]>(value => entries = value, () => entries, out entriesListener);
		}

		public bool None { get { return Entries.Value.Length == 0; } }
		public bool Any { get { return 0 < Entries.Value.Length; } }

		public T Peek() { return Entries.Value.FirstOrDefault().Value; }

		public Action Push(T value)
		{
			return Push(value, Guid.NewGuid().ToString());
		}

		public Action Push(T value, string id)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

			if (entriesListener.Value.Any(e => e.Id == id))
			{
				Debug.LogError("Pushing an entry with duplicate Id \"" + id + "\", unpredictable behaviour may occur");
			}

			entriesListener.Value = entriesListener.Value.Prepend(new Entry(id, value)).ToArray();

			return () => Pop(id);
		}

		public bool Pop(string id)
		{
			var newEntries = Entries.Value.Where(b => b.Id != id).ToArray();
			var result = newEntries.Length != Entries.Value.Length;
			entriesListener.Value = newEntries;
			return result;
		}
	}
}