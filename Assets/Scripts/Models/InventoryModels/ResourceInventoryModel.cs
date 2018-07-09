using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ResourceInventoryModel : InventoryModel
	{
		public static ResourceInventoryModel Zero { get { return new ResourceInventoryModel(); } }

		[JsonProperty] float rations;
		[JsonProperty] float fuel;

		/// <summary>
		/// The years worth of rations on board, assuming a person uses 1 ration
		/// a year.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		/// <summary>
		/// The fuel onboard, multiplies speed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Fuel;

		public override InventoryTypes InventoryType { get { return InventoryTypes.Resources; } }

		[JsonIgnore]
		public bool IsZero
		{
			get
			{
				return Mathf.Approximately(0f, Rations.Value) &&
							Mathf.Approximately(0f, Fuel.Value);
			}
		}

		public ResourceInventoryModel(
			float rationsValue = 0f,
			float fuelValue = 0f
		)
		{
			rations = rationsValue;
			fuel = fuelValue;

			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel);
		}

		/// <summary>
		/// Add the other resources to this one, and returns the remainder if
		/// any values are negative.
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Add(ResourceInventoryModel other)
		{
			return AddOut(other, this);
		}

		/// <summary>
		/// Add the other resources to this one, but assign the result to the
		/// specified object. Returns any negative values.
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="other">Other.</param>
		/// <param name="assigned">Assigned.</param>
		public ResourceInventoryModel AddOut(ResourceInventoryModel other, ResourceInventoryModel assigned)
		{
			if (assigned == null) throw new ArgumentNullException("assigned");

			var remainder = Zero;

			var newRations = Rations.Value + other.Rations.Value;
			var newFuel = Fuel.Value + other.Fuel.Value;

			if (newRations < 0f) remainder.Rations.Value = Mathf.Abs(newRations);
			if (newFuel < 0f) remainder.Fuel.Value = Mathf.Abs(newFuel);

			assigned.Rations.Value = Mathf.Max(0f, newRations);
			assigned.Fuel.Value = Mathf.Max(0f, newFuel);

			return remainder;
		}

		/// <summary>
		/// Subtract the other resource to this one, and returns the remainder
		/// if any values are negative.
		/// </summary>
		/// <returns>The subtract.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Subtract(ResourceInventoryModel other)
		{
			return SubtractOut(other, this);
		}

		/// <summary>
		/// Subtract the other resources to this one, but assign the result to the
		/// specified object. Returns any negative values.
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="other">Other.</param>
		/// <param name="assigned">Assigned.</param>
		public ResourceInventoryModel SubtractOut(ResourceInventoryModel other, ResourceInventoryModel assigned)
		{
			return AddOut(other.Inverse(), assigned);
		}

		/// <summary>
		/// Assign the values of other to this.
		/// </summary>
		/// <param name="other">Other.</param>
		public void Assign(ResourceInventoryModel other)
		{
			Rations.Value = other.Rations.Value;
			Fuel.Value = other.Fuel.Value;
		}

		public ResourceInventoryModel Inverse()
		{
			var result = Zero;

			result.Rations.Value = -Rations.Value;
			result.Fuel.Value = -Fuel.Value;

			return result;
		}

		public override string ToString()
		{
			return "Resources:\n\tRations: \t" + Rations.Value + "\n\tFuel: \t" + Fuel.Value;
		}
	}
}