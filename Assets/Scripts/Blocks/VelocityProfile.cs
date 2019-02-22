using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct VelocityProfile
	{
		public static VelocityProfile Default { get { return new VelocityProfile(0f, 0f, 0); } }

		[Serializable]
		public struct Velocity
		{
			public int Multiplier;

			public float Relativistic;
			public float Newtonian;

			public float RelativisticLightYears;
			public float NewtonianLightYears;

			public float RelativityRatio;

			/// <summary>
			/// The velocities, from 0 to 1, between the minimum and maximum
			/// multiplier speeds.
			/// </summary>
			public float Normal;

			public float PropellantRequired;

			public bool Approximately(Velocity other)
			{
				if (Multiplier != other.Multiplier) return false;
				if (!Mathf.Approximately(Normal, other.Normal)) return false;
				if (!Mathf.Approximately(PropellantRequired, other.PropellantRequired)) return false;
				return Mathf.Approximately(Relativistic, other.Relativistic) && Mathf.Approximately(Newtonian, other.Newtonian);
			}
		}

		#region Provided
		[JsonProperty] readonly float velocityMinimumLightYears;
		[JsonProperty] public readonly float PropellantConsumptionMultiplier;
		#endregion

		#region Calculated
		[JsonProperty] public readonly Velocity[] Velocities;
		#endregion

		public VelocityProfile(
			float velocityMinimumLightYears,
			float propellantConsumptionMultiplier,
			int count
		)
		{
			this.velocityMinimumLightYears = velocityMinimumLightYears;
			var velocityMinimum = UniversePosition.ToUniverseDistance(velocityMinimumLightYears);
			PropellantConsumptionMultiplier = propellantConsumptionMultiplier;

			Velocities = new Velocity[count];

			var maximumVelocity = 0f;

			for (var i = 0; i < count; i++)
			{
				var velocity = new Velocity();

				velocity.Multiplier = i + 1;

				if (i == 0)
				{
					velocity.Relativistic = velocityMinimum;
					velocity.Newtonian = velocityMinimum;
					velocity.RelativisticLightYears = velocityMinimumLightYears;
					velocity.NewtonianLightYears = velocityMinimumLightYears;
					velocity.RelativityRatio = 1f;
					velocity.Normal = 0f;
					velocity.PropellantRequired = propellantConsumptionMultiplier;
				}
				else
				{
					var relativeLightYears = 0f;
					var newtonianLightYears = 0f;
					RelativityUtility.VelocityByEnergyMultiplier(
						velocityMinimumLightYears,
						i,
						out relativeLightYears,
						out newtonianLightYears
					);

					velocity.Relativistic = UniversePosition.ToUniverseDistance(relativeLightYears);
					velocity.Newtonian = UniversePosition.ToUniverseDistance(newtonianLightYears);
					velocity.RelativisticLightYears = relativeLightYears;
					velocity.NewtonianLightYears = newtonianLightYears;
					velocity.RelativityRatio = relativeLightYears / newtonianLightYears;
					velocity.PropellantRequired = propellantConsumptionMultiplier * (i + 1);
				}

				maximumVelocity = velocity.Relativistic;

				Velocities[i] = velocity;
			}

			if (!Mathf.Approximately(0f, maximumVelocity))
			{
				var velocityRange = maximumVelocity - velocityMinimum;
				for (var i = 0; i < Velocities.Length; i++) Velocities[i].Normal = (Velocities[i].Relativistic - velocityMinimum) / velocityRange;
			}
		}

		[JsonIgnore] public int Count { get { return Velocities.Length; } }
		[JsonIgnore] public Velocity Minimum { get { return Velocities.FirstOrDefault(); } }
		[JsonIgnore] public Velocity Maximum { get { return Velocities.LastOrDefault(); } }

		public bool Approximately(VelocityProfile other)
		{
			if (Count != other.Count) return false;

			for (var i = 0; i < Velocities.Length; i++)
			{
				if (!Velocities[i].Approximately(other.Velocities[i])) return false;
			}

			return true;
		}

		public VelocityProfile Duplicate(
			float? velocityMinimumLightYears = null,
			float? propellantConsumptionMultiplier = null,
			int? count = null
		)
		{
			return new VelocityProfile(
				velocityMinimumLightYears ?? this.velocityMinimumLightYears,
				propellantConsumptionMultiplier ?? PropellantConsumptionMultiplier,
				count ?? Count
			);
		}
	}
}