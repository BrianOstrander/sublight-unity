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
		[JsonIgnore]
		public Action<ResourceInventoryModel> AnyChange = ActionExtensions.GetEmpty<ResourceInventoryModel>();

		public override InventoryTypes InventoryType { get { return InventoryTypes.Resources; } }
		public override bool SlotRequired { get { return false; } }

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

			Rations = new ListenerProperty<float>(value => rations = value, () => rations, value => AnyChange(this));
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel, value => AnyChange(this));
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
		public ResourceInventoryModel AddOut(ResourceInventoryModel other, ResourceInventoryModel assigned = null)
		{
			if (assigned == null) assigned = Zero;

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
		/// Subtract the other resources to this one, but assign the result to
		/// the specified object. Returns any negative values.
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="other">Other.</param>
		/// <param name="assigned">Assigned.</param>
		public ResourceInventoryModel SubtractOut(ResourceInventoryModel other, ResourceInventoryModel assigned = null)
		{
			return AddOut(other.Inverse(), assigned);
		}

		/// <summary>
		/// Clamp resources using the other as a maximum, and returns the
		/// remainder if any at all.
		/// </summary>
		/// <returns>The clamp.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Clamp(ResourceInventoryModel other)
		{
			return ClampOut(other, this);
		}

		/// <summary>
		/// Clamp resources using the other as a maximum, but assign the result
		/// to the specified object. Returns a remainder, if any at all.
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="other">Other.</param>
		/// <param name="assigned">Assigned.</param>
		public ResourceInventoryModel ClampOut(ResourceInventoryModel other, ResourceInventoryModel assigned = null)
		{
			if (assigned == null) assigned = Zero;

			var remainder = Zero;

			var otherRations = Mathf.Max(0f, other.Rations);
			var otherFuel = Mathf.Max(0f, other.Fuel);

			var newRations = Mathf.Clamp(Rations.Value, 0f, otherRations);
			var newFuel = Mathf.Clamp(Fuel.Value, 0f, otherFuel);

			if (newRations < Rations) remainder.Rations.Value = Rations - otherRations;
			if (newFuel < Fuel) remainder.Fuel.Value = Fuel - otherFuel;

			assigned.Rations.Value = newRations;
			assigned.Fuel.Value = newFuel;

			return remainder;
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

		public bool IsEqual(ResourceInventoryModel other)
		{
			return Mathf.Approximately(Rations, other.Rations)
						&& Mathf.Approximately(Fuel, other.Fuel);
		}

		public override string ToString()
		{
			return "Resources:\n\tRations: \t" + Rations.Value + "\n\tFuel: \t" + Fuel.Value;
		}
	}
}