/*
using System;
using System.Linq;

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

		public VelocityProfileState(
			VelocityProfile profile,
			int propellantUsage,
			int propellantUsageLimit
		)
		{
			Profile = profile;
			PropellantUsage = propellantUsage;
			PropellantUsageLimit = propellantUsageLimit;
		}

		[JsonIgnore]
		public VelocityProfile.Velocity Current
		{
			get { return GetVelocityByUsage(PropellantUsage); }
		}

		public VelocityProfile.Velocity GetVelocityByUsage(int propellantUsage)
		{
			if (Profile.Count == 0) return default(VelocityProfile.Velocity);
			return Profile.Velocities.FirstOrDefault(v => v.Multiplier == propellantUsage);
		}

		public VelocityProfileState Duplicate(
			VelocityProfile? profile = null,
			int? propellantUsage = null,
			int? propellantUsageLimit = null
		)
		{
			return new VelocityProfileState(
				profile ?? Profile,
				propellantUsage ?? PropellantUsage,
				propellantUsageLimit ?? PropellantUsageLimit
			);
		}
	}
}
*/