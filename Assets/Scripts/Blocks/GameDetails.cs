using System;
using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct GameDetails
	{
		[Serializable]
		public struct SystemDetails
		{
			public string Name;
			public DateTime EnterTime;
			public RelativeDayTime RelativeEnterTime;
			public UniversePosition Position;
			public int TransitCount;
			public float TotalDistanceTraveled;
		}

		public DateTime Time;
		public RelativeDayTime RelativeTime;

		public SystemDetails[] SystemsVisited;

		public SystemDetails BeginSystem;
		public SystemDetails EndSystem;

		[JsonIgnore]
		public SystemDetails CurrentSystem
		{
			get
			{
				return SystemsVisited == null ? default(SystemDetails) : SystemsVisited.OrderBy(v => v.TransitCount).LastOrDefault();
			}
		}

		public void PushVisitedSystem(SystemDetails system)
		{
			SystemsVisited = SystemsVisited ?? new SystemDetails[0];

			SystemsVisited = SystemsVisited.Append(system).ToArray();
		}
	}
}