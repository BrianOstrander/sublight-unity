using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct Changelog
	{
		public enum Changes
		{
			Unknown = 0,
			Feature = 10,
			Improvement = 20,
			Fix = 30,
			Deprecation = 40
		}

		[Serializable]
		public struct Entry
		{
			public int Index;
			public Changes Change;
			public string Description;
		}

		public string Title;
		[TextArea]
		public string Description;
		public string ReleaseType;
		public string Cyle;
		public int Version;
		public int Year;
		public int Month;
		public int Day;

		public Entry[] Entries;
	}
}