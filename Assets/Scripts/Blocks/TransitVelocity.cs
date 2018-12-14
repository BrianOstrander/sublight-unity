using System;
using System.Linq;

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
					new float[] { 0f }
				);
			}
		}

		#region Provided
		public readonly float VelocityMinimum;
		public readonly float VelocityShip;

		public readonly int MultiplierCurrent;
		public readonly int MultiplierMaximum;

		public readonly float[] MultiplierVelocityDeltas;
		#endregion

		#region Calculated
		public readonly float VelocityCurrent;
		public readonly float VelocityLightYearsCurrent;

		public readonly float[] MultiplierVelocityDeltasLightYears;
		#endregion

		TransitVelocity(
			float velocityMinimum,
			float velocityShip,
			int multiplierCurrent,
			float[] multiplierVelocityDeltas
		)
		{
			VelocityMinimum = velocityMinimum;
			VelocityShip = velocityShip;
			MultiplierCurrent = multiplierCurrent;
			MultiplierMaximum = multiplierVelocityDeltas.Length;
			MultiplierVelocityDeltas = multiplierVelocityDeltas;

			VelocityCurrent = MultiplierVelocityDeltas[MultiplierCurrent];

			MultiplierVelocityDeltasLightYears = new float[MultiplierVelocityDeltas.Length];
			for (var i = 0; i < MultiplierVelocityDeltas.Length; i++)
			{
				MultiplierVelocityDeltasLightYears[i] = UniversePosition.ToLightYearDistance(MultiplierVelocityDeltas[i]);
			}
			VelocityLightYearsCurrent = MultiplierVelocityDeltasLightYears[multiplierCurrent];
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

		public TransitVelocity NewMultiplierVelocityDeltas(float[] multiplierVelocityDeltas)
		{
			return Duplicate(multiplierVelocityDeltas: multiplierVelocityDeltas);
		}

		public TransitVelocity Duplicate(
			float? velocityMinimum = null,
			float? velocityShip = null,
			int? multiplierCurrent = null,
			float[] multiplierVelocityDeltas = null
		)
		{
			return new TransitVelocity(
				velocityMinimum.HasValue ? velocityMinimum.Value : VelocityMinimum,
				velocityShip.HasValue ? velocityShip.Value : VelocityShip,
				multiplierCurrent.HasValue ? multiplierCurrent.Value : MultiplierCurrent,
				multiplierVelocityDeltas != null ? multiplierVelocityDeltas : MultiplierVelocityDeltas.ToArray()
			);
		}
	}
}