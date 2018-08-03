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

		[JsonProperty] float speed;
		[JsonProperty] float estimateFailureRange;

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

		/// <summary>
		/// Basically the speed of the ship, expressed in universe units per day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Speed;
		/// <summary>
		/// The estimate failure range modifier. This is a positive value that
		/// can be subtracted from 1 to find the proper range. Higher values are
		/// better.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> EstimateFailureRange;

		[JsonIgnore]
		public readonly ListenerProperty<float>[] Values;
		[JsonIgnore]
		public readonly int ValueCount;

		[JsonIgnore]
		public Action<ResourceInventoryModel> AnyChange = ActionExtensions.GetEmpty<ResourceInventoryModel>();

		public override InventoryTypes InventoryType { get { return InventoryTypes.Resources; } }
		public override bool SlotRequired { get { return false; } }

		[JsonIgnore]
		public bool IsZero
		{
			get
			{
				foreach (var value in Values)
				{
					if (!Mathf.Approximately(0f, value.Value)) return false;
				}
				return true;
			}
		}

		[JsonIgnore]
		public ResourceInventoryModel Duplicate { get { return new ResourceInventoryModel(Rations, Fuel, Speed, EstimateFailureRange); } }

		[JsonConstructor]
		ResourceInventoryModel()
		{
			Values = new ListenerProperty<float>[] {
				Rations = new ListenerProperty<float>(value => rations = value, () => rations, "Rations", value => AnyChange(this)),
				Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel, "Fuel", value => AnyChange(this)),
				Speed = new ListenerProperty<float>(value => speed = value, () => speed, "Speed", value => AnyChange(this)),
				EstimateFailureRange = new ListenerProperty<float>(value => estimateFailureRange = value, () => estimateFailureRange, "EstimateFailureRange", value => AnyChange(this))
			};
			ValueCount = Values.Length;
		}

		public ResourceInventoryModel(
			float rations,
			float fuel,
			float speed,
			float estimateFailureRange
		) : this()
		{
			this.rations = rations;
			this.fuel = fuel;
			this.speed = speed;
			this.estimateFailureRange = estimateFailureRange;
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

		void AddOutValue(
			ListenerProperty<float> current, 
			ListenerProperty<float> other, 
			ListenerProperty<float> remainder, 
			ListenerProperty<float> assigned
		)
		{
			var newValue = current.Value + other.Value;

			if (newValue < 0f) remainder.Value = Mathf.Abs(newValue);

			assigned.Value = newValue;
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

			for (var i = 0; i < ValueCount; i++) AddOutValue(Values[i], other.Values[i], remainder.Values[i], assigned.Values[i]);

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
			foreach (var curr in Values) curr.Value *= value;

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

		void ClampOutValue(
			ListenerProperty<float> current,
			ListenerProperty<float> maximum,
			ListenerProperty<float> remainder,
			ListenerProperty<float> assigned
		)
		{
			var otherValue = Mathf.Max(0f, maximum.Value);

			var newValue = Mathf.Clamp(current.Value, 0f, otherValue);

			if (newValue < current.Value) remainder.Value = current.Value - otherValue;

			assigned.Value = newValue;
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

			for (var i = 0; i < ValueCount; i++) ClampOutValue(Values[i], maximum.Values[i], remainder.Values[i], assigned.Values[i]);

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

		void MinimumMaximumOutValue(
			bool minimum,
			ListenerProperty<float> current,
			ListenerProperty<float> other,
			ListenerProperty<float> difference,
			ListenerProperty<float> assigned
		)
		{
			var minValue = Mathf.Min(current.Value, other.Value);

			var maxValue = Mathf.Max(current.Value, other.Value);

			difference.Value = maxValue - minValue;

			if (minimum) assigned.Value = minValue;
			else assigned.Value = maxValue;
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

			for (var i = 0; i < ValueCount; i++)
			{
				MinimumMaximumOutValue(minimum, Values[i], other.Values[i], difference.Values[i], assigned.Values[i]);
			}

			return difference;
		}

		/// <summary>
		/// Assign the values of other to this.
		/// </summary>
		/// <param name="other">Other.</param>
		public void Assign(ResourceInventoryModel other)
		{
			for (var i = 0; i < ValueCount; i++) Values[i].Value = other.Values[i].Value;
		}

		public ResourceInventoryModel Inverse()
		{
			var result = Zero;

			for (var i = 0; i < ValueCount; i++) result.Values[i].Value = -Values[i].Value;

			return result;
		}

		public bool ValuesEqual(ResourceInventoryModel other)
		{
			if (other == null) return false;
			if (other == this) return true;

			for (var i = 0; i < ValueCount; i++)
			{
				if (!Mathf.Approximately(Values[i].Value, other.Values[i].Value)) return false;
			}
			return true;
		}

		public override string ToString()
		{
			var result = "Resources:";
			foreach (var value in Values) result += "\n\t" + value.Name + ": \t" + value.Value;
			return result;
		}
	}
}