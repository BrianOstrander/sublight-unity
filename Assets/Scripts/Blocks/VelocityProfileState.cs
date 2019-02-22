using System;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct VelocityProfileState
	{
		public static VelocityProfileState Default { get { return new VelocityProfileState(); } }

		[JsonProperty] public readonly VelocityProfile Profile;
		[JsonProperty] public readonly int PropellantUsage;
		[JsonProperty] public readonly int PropellantUsageLimit;

		[JsonIgnore]
		public VelocityProfile.Velocity Velocity
		{
			get
			{
				if (Profile.Count == 0) return default(VelocityProfile.Velocity);
				return Profile.Velocities[PropellantUsage];
			}
		}
	}
}