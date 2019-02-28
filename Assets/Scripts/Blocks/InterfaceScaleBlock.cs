using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct InterfaceScaleBlock
	{
		public static InterfaceScaleBlock Default
		{
			get
			{
				return new InterfaceScaleBlock
				{
					Entries = new Entry[0]
				};
			}
		}

		[Serializable]
		public struct Entry
		{
			public int Index;
			public float Scale;
		}

		public Entry[] Entries;

		public float GetScale(int index)
		{
			if (Entries == null || Entries.Length == 0) return 1f;

			return (0 < index) ? Entries.OrderBy(e => e.Index).Last().Scale : Entries.OrderBy(e => e.Index).First().Scale;
		}

		public Vector3 GetVectorScale(int index)
		{
			return GetScale(index) * Vector3.one;
		}
	}
}