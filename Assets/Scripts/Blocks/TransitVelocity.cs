using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TransitVelocity
	{
		public static TransitVelocity Default
		{
			get
			{
				return new TransitVelocity(
					0f,
					0f,
					0,
					0,
					0
				);
			}
		}

		#region Provided
		public readonly float VelocityMinimum;
		public readonly float VelocityShip;

		/// <summary>
		/// The current multiplier, 0 is base speed, multiplier maximum is max
		/// speed.
		/// </summary>
		public readonly int MultiplierCurrent;
		/// <summary>
		/// The maximum multiplier possible with the amount of propellent
		/// available.
		/// </summary>
		public readonly int MultiplierMaximum;
		/// <summary>
		/// The maximum multiplier with the current fuel supply.
		/// </summary>
		public readonly int MultiplierEnabledMaximum;
		#endregion

		#region Calculated
		/// <summary>
		/// The velocity minimum and ship velocity combined.
		/// </summary>
		public readonly float VelocityBase;

		public readonly float VelocityMinimumLightSpeed;
		public readonly float VelocityShipLightSpeed;
		/// <summary>
		/// The velocity minimum and ship velocity combined.
		/// </summary>
		public readonly float VelocityBaseLightSpeed;

		public readonly float VelocityCurrent;
		public readonly float VelocityLightYearsCurrent;
		public readonly float VelocityNewtonianCurrent;
		public readonly float VelocityNewtonianLightYearsCurrent;

		public readonly float VelocityRelativityRatioCurrent;

		public readonly float[] MultiplierVelocities;
		public readonly float[] MultiplierVelocitiesLightYears;

		public readonly float[] MultiplierVelocitiesNewtonian;
		public readonly float[] MultiplierVelocitiesNewtonianLightYears;

		public readonly float[] VelocityRelativityRatios;
		/// <summary>
		/// The velocities, from 0 to 1, between the minimum and maximum
		/// multiplier speeds.
		/// </summary>
		public readonly float[] VelocityNormals;
		#endregion

		TransitVelocity(
			float velocityMinimum,
			float velocityShip,
			int multiplierCurrent,
			int multiplierMaximum,
			int multiplierEnabledMaximum
		)
		{
			VelocityMinimum = velocityMinimum;
			VelocityShip = velocityShip;
			MultiplierCurrent = multiplierCurrent;
			MultiplierMaximum = multiplierMaximum;
			MultiplierEnabledMaximum = multiplierEnabledMaximum;

			VelocityBase = VelocityMinimum + VelocityShip;
			VelocityMinimumLightSpeed = UniversePosition.ToLightYearDistance(VelocityMinimum);
			VelocityShipLightSpeed = UniversePosition.ToLightYearDistance(VelocityShip);
			VelocityBaseLightSpeed = UniversePosition.ToLightYearDistance(VelocityBase);

			MultiplierVelocities = new float[MultiplierMaximum + 1];
			MultiplierVelocitiesLightYears = new float[MultiplierVelocities.Length];
			MultiplierVelocitiesNewtonian = new float[MultiplierVelocities.Length];
			MultiplierVelocitiesNewtonianLightYears = new float[MultiplierVelocities.Length];

			VelocityRelativityRatios = new float[MultiplierVelocities.Length];

			for (var i = 0; i < MultiplierVelocities.Length; i++)
			{
				var currRelative = 0f;
				var currNewtonian = 0f;
				RelativityUtility.VelocityByEnergyMultiplier(VelocityBaseLightSpeed, i + 1, out currRelative, out currNewtonian);
				MultiplierVelocitiesLightYears[i] = currRelative;
				MultiplierVelocities[i] = UniversePosition.ToUniverseDistance(currRelative);
				MultiplierVelocitiesNewtonian[i] = currNewtonian;
				MultiplierVelocitiesNewtonianLightYears[i] = UniversePosition.ToUniverseDistance(currNewtonian);

				VelocityRelativityRatios[i] = currRelative / currNewtonian;
			}

			VelocityNormals = new float[MultiplierVelocities.Length];
			var maximumVelocity = MultiplierVelocities.LastOrDefault();

			if (Mathf.Approximately(maximumVelocity, 0f))
			{
				for (var i = 0; i < MultiplierVelocities.Length; i++) VelocityNormals[i] = 0f;
			}
			else
			{
				var range = maximumVelocity - VelocityBase;
				for (var i = 0; i < MultiplierVelocities.Length; i++)
				{
					VelocityNormals[i] = (MultiplierVelocities[i] - VelocityBase) / range;
				}
			}

			VelocityCurrent = MultiplierVelocities[MultiplierCurrent];
			VelocityLightYearsCurrent = MultiplierVelocitiesLightYears[multiplierCurrent];
			VelocityNewtonianCurrent = MultiplierVelocitiesNewtonian[MultiplierCurrent];
			VelocityNewtonianLightYearsCurrent = MultiplierVelocitiesNewtonianLightYears[multiplierCurrent];

			VelocityRelativityRatioCurrent = VelocityRelativityRatios[multiplierCurrent];
		}

		public TransitVelocity NewVelocityMinimum(float velocityMinimum)
		{
			return Duplicate(velocityMinimum: velocityMinimum);
		}

		public TransitVelocity NewVelocityShip(float velocityShip)
		{
			return Duplicate(velocityShip: velocityShip);
		}

		public TransitVelocity NewMultiplierCurrent(int multiplierCurrent)
		{
			return Duplicate(multiplierCurrent: multiplierCurrent);
		}

		public TransitVelocity NewMultiplierMaximum(int multiplierMaximum)
		{
			return Duplicate(multiplierMaximum: multiplierMaximum);
		}

		public TransitVelocity NewMultiplierEnabledMaximum(int multiplierEnabledMaximum)
		{
			return Duplicate(multiplierEnabledMaximum: multiplierEnabledMaximum);
		}

		public TransitVelocity Duplicate(
			float? velocityMinimum = null,
			float? velocityShip = null,
			int? multiplierCurrent = null,
			int? multiplierMaximum = null,
			int? multiplierEnabledMaximum = null
		)
		{
			return new TransitVelocity(
				velocityMinimum ?? VelocityMinimum,
				velocityShip ?? VelocityShip,
				multiplierCurrent ?? MultiplierCurrent,
				multiplierMaximum ?? MultiplierMaximum,
				multiplierEnabledMaximum ?? MultiplierEnabledMaximum
			);
		}

		public bool Approximately(TransitVelocity other, bool includingState = false)
		{
			if (includingState)
			{
				if (MultiplierCurrent != other.MultiplierCurrent) return false;
				if (MultiplierEnabledMaximum != other.MultiplierEnabledMaximum) return false;
			}

			if (MultiplierMaximum != other.MultiplierMaximum) return false;

			for (var i = 0; i < MultiplierVelocities.Length; i++)
			{
				if (!Mathf.Approximately(MultiplierVelocities[i], other.MultiplierVelocities[i])) return false;
			}

			return true;
		}
	}
}