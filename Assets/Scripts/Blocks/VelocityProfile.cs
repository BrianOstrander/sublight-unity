﻿/*
using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct VelocityProfile
	{
		public static VelocityProfile Default { get { return new VelocityProfile(0f, 0); } }

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

			public bool Approximately(Velocity other)
			{
				if (Multiplier != other.Multiplier) return false;
				if (!Mathf.Approximately(Normal, other.Normal)) return false;
				return Mathf.Approximately(Relativistic, other.Relativistic) && Mathf.Approximately(Newtonian, other.Newtonian);
			}
		}

		#region Provided
		[JsonProperty] readonly float velocityMinimumLightYears;
		#endregion

		#region Calculated
		[JsonProperty] public readonly Velocity[] Velocities;
		#endregion

		public VelocityProfile(
			float velocityMinimumLightYears,
			int count
		)
		{
			this.velocityMinimumLightYears = velocityMinimumLightYears;
			var velocityMinimum = UniversePosition.ToUniverseDistance(velocityMinimumLightYears);

			Velocities = new Velocity[count];

			var maximumVelocity = 0f;

			for (var i = 0; i < count; i++)
			{
				var velocity = new Velocity();

				velocity.Multiplier = i + 1;

				var relativeLightYears = 0f;
				var newtonianLightYears = 0f;
				RelativityUtility.VelocityByEnergyMultiplier(
					velocityMinimumLightYears,
					velocity.Multiplier,
					out relativeLightYears,
					out newtonianLightYears
				);

				velocity.Relativistic = UniversePosition.ToUniverseDistance(relativeLightYears);
				velocity.Newtonian = UniversePosition.ToUniverseDistance(newtonianLightYears);
				velocity.RelativisticLightYears = relativeLightYears;
				velocity.NewtonianLightYears = newtonianLightYears;
				velocity.RelativityRatio = relativeLightYears / newtonianLightYears;

				maximumVelocity = velocity.Relativistic;

				Velocities[i] = velocity;
			}

			if (Velocities.Any()) velocityMinimum = Velocities.FirstOrDefault().Relativistic;

			if (!Mathf.Approximately(0f, maximumVelocity))
			{
				var velocityRange = maximumVelocity - velocityMinimum;
				for (var i = 0; i < Velocities.Length; i++) Velocities[i].Normal = (Velocities[i].Relativistic - velocityMinimum) / velocityRange;
			}
		}

		[JsonIgnore] public int Count { get { return Velocities == null ? 0 : Velocities.Length; } }
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
			int? count = null
		)
		{
			return new VelocityProfile(
				velocityMinimumLightYears ?? this.velocityMinimumLightYears,
				count ?? Count
			);
		}
	}
}
*/