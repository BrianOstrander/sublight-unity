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

		[JsonIgnore]
		public ResourceInventoryModel Duplicate { get { return new ResourceInventoryModel(Rations, Fuel); } }

		[JsonConstructor]
		ResourceInventoryModel()
		{
			Rations = new ListenerProperty<float>(value => rations = value, () => rations, value => AnyChange(this));
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel, value => AnyChange(this));
		}

		public ResourceInventoryModel(
			float rationsValue = 0f,
			float fuelValue = 0f
		) : this()
		{
			rations = rationsValue;
			fuel = fuelValue;
		}

		/// <summary>
		/// Add the other resources to this one.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Add(ResourceInventoryModel other)
		{
			AddOut(other, this);
			return this;
		}

		/// <summary>
		/// Add the other resources to this one, and assign any remainders to
		/// the out model.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		/// <param name="remainder">Remainder.</param>
		public ResourceInventoryModel Add(ResourceInventoryModel other, out ResourceInventoryModel remainder)
		{
			remainder = AddOut(other, this);
			return this;
		}

		/// <summary>
		/// Add the other resources to this one, but assign the result to the
		/// specified object. Returns any negative values.
		/// </summary>
		/// <returns>The remainder.</returns>
		/// <param name="other">Other.</param>
		/// <param name="assigned">Assigned.</param>
		ResourceInventoryModel AddOut(ResourceInventoryModel other, ResourceInventoryModel assigned)
		{
			if (assigned == null) throw new ArgumentNullException("assigned");

			var remainder = Zero;

			var newRations = Rations.Value + other.Rations.Value;
			var newFuel = Fuel.Value + other.Fuel.Value;

			if (newRations < 0f) remainder.Rations.Value = Mathf.Abs(newRations);
			if (newFuel < 0f) remainder.Fuel.Value = Mathf.Abs(newFuel);

			assigned.Rations.Value = newRations;
			assigned.Fuel.Value = newFuel;

			return remainder;
		}

		/// <summary>
		/// Subtract the other resource to this one.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Subtract(ResourceInventoryModel other)
		{
			SubtractOut(other, this);
			return this;
		}

		/// <summary>
		/// Subtract the other resource from this one, and assign any
		/// remainders to the out model.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		/// <param name="remainder">Remainder.</param>
		public ResourceInventoryModel Subtract(ResourceInventoryModel other, out ResourceInventoryModel remainder)
		{
			remainder = SubtractOut(other, this);
			return this;
		}

		/// <summary>
		/// Subtract the other resources to this one, but assign the result to
		/// the specified object. Returns any negative values.
		/// </summary>
		/// <returns>The remainder.</returns>
		/// <param name="other">Other.</param>
		/// <param name="assigned">Assigned.</param>
		ResourceInventoryModel SubtractOut(ResourceInventoryModel other, ResourceInventoryModel assigned)
		{
			return AddOut(other.Inverse(), assigned);
		}

		/// <summary>
		/// Multiplies these resources by the specified value.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="value">Value.</param>
		public ResourceInventoryModel Multiply(float value)
		{
			Rations.Value *= value;
			Fuel.Value *= value;

			return this;
		}

		/// <summary>
		/// Clamp resources using the other as a maximum.
		/// </summary>
		/// <remarks>
		/// Maximum values should be above zeros, or unexpected behaviour may
		/// occur.
		/// </remarks>
		/// <returns>Itself.</returns>
		/// <param name="maximum">Maximum.</param>
		public ResourceInventoryModel Clamp(ResourceInventoryModel maximum)
		{
			ClampOut(maximum, this);
			return this;
		}

		/// <summary>
		/// Clamp resources using the other as a maximum, and assigns any
		/// difference to the out model.
		/// </summary>
		/// <remarks>
		/// Maximum values should be above zeros, or unexpected behaviour may
		/// occur.
		/// </remarks>
		/// <returns>Itself.</returns>
		/// <param name="maximum">Maximum.</param>
		/// <param name="difference">Difference.</param>
		public ResourceInventoryModel Clamp(ResourceInventoryModel maximum, out ResourceInventoryModel difference)
		{
			difference = ClampOut(maximum, this);
			return this;
		}

		/// <summary>
		/// Clamp resources using the other as a maximum, but assign the result
		/// to the specified object. Returns a remainder, if any at all.
		/// </summary>
		/// Maximum values should be above zeros, or unexpected behaviour may
		/// occur.
		/// <returns>The out.</returns>
		/// <param name="maximum">Other.</param>
		/// <param name="assigned">Assigned.</param>
		ResourceInventoryModel ClampOut(ResourceInventoryModel maximum, ResourceInventoryModel assigned = null)
		{
			if (assigned == null) assigned = Zero;

			var remainder = Zero;

			var otherRations = Mathf.Max(0f, maximum.Rations);
			var otherFuel = Mathf.Max(0f, maximum.Fuel);

			var newRations = Mathf.Clamp(Rations.Value, 0f, otherRations);
			var newFuel = Mathf.Clamp(Fuel.Value, 0f, otherFuel);

			if (newRations < Rations) remainder.Rations.Value = Rations - otherRations;
			if (newFuel < Fuel) remainder.Fuel.Value = Fuel - otherFuel;

			assigned.Rations.Value = newRations;
			assigned.Fuel.Value = newFuel;

			return remainder;
		}

		/// <summary>
		/// Assigns the minimum values of this or the other model to itself.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Minimum(ResourceInventoryModel other)
		{
			MinimumMaximumOut(other, true, this);
			return this;
		}

		/// <summary>
		/// Assigns the minimum values of this or the other model to itself and
		/// assigns any differnce to the specified out model.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		/// <param name="difference">Difference.</param>
		public ResourceInventoryModel Minimum(ResourceInventoryModel other, out ResourceInventoryModel difference)
		{
			difference = MinimumMaximumOut(other, true, this);
			return this;
		}

		/// <summary>
		/// Keeps all values greater than or equal to zero, assigns zero to all
		/// its other values.
		/// </summary>
		/// <returns>Itself.</returns>
		public ResourceInventoryModel ClampNegatives()
		{
			return Maximum(Zero);
		}

		/// <summary>
		/// Keeps all values greater than or equal to zero, assigns zero to all
		/// its other values. Any difference is assigned to the specified model.
		/// </summary>
		/// <returns>Itself.</returns>
		public ResourceInventoryModel ClampNegatives(out ResourceInventoryModel difference)
		{
			return Maximum(Zero, out difference);
		}

		/// <summary>
		/// Assigns the maximum values of this or the other model to itself.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		public ResourceInventoryModel Maximum(ResourceInventoryModel other)
		{
			MinimumMaximumOut(other, false, this);
			return this;
		}

		/// <summary>
		/// Assigns the maximum values of this or the other model to itself and
		/// assigns any difference to the specified out model.
		/// </summary>
		/// <returns>Itself.</returns>
		/// <param name="other">Other.</param>
		/// <param name="difference">Difference.</param>
		public ResourceInventoryModel Maximum(ResourceInventoryModel other, out ResourceInventoryModel difference)
		{
			difference = MinimumMaximumOut(other, false, this);
			return this;
		}

		/// <summary>
		/// Assigns the maximum or minimum values, as specified by the bool, of
		/// this or the other model to the specified object and returns the
		/// difference.
		/// </summary>
		/// <returns>The difference.</returns>
		/// <param name="other">Other.</param>
		/// <param name="minimum">If set to <c>true</c> minimum.</param>
		/// <param name="assigned">Assigned.</param>
		ResourceInventoryModel MinimumMaximumOut(ResourceInventoryModel other, bool minimum, ResourceInventoryModel assigned = null)
		{
			if (assigned == null) assigned = Zero;

			var difference = Zero;

			var minRations = Mathf.Min(Rations, other.Rations);
			var minFuel = Mathf.Min(Fuel, other.Fuel);

			var maxRations = Mathf.Max(Rations, other.Rations);
			var maxFuel = Mathf.Max(Fuel, other.Fuel);

			difference.Rations.Value = maxRations - minRations;
			difference.Fuel.Value = maxFuel - minFuel;

			if (minimum)
			{
				assigned.Rations.Value = minRations;
				assigned.Fuel.Value = minFuel;
			}
			else
			{
				assigned.Rations.Value = maxRations;
				assigned.Fuel.Value = maxFuel;
			}

			return difference;
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

		public bool ValuesEqual(ResourceInventoryModel other)
		{
			if (other == null) return false;
			if (other == this) return true;

			return Mathf.Approximately(Rations, other.Rations)
						&& Mathf.Approximately(Fuel, other.Fuel);
		}

		public override string ToString()
		{
			return "Resources:\n\tRations: \t" + Rations.Value + "\n\tFuel: \t" + Fuel.Value;
		}
	}
}