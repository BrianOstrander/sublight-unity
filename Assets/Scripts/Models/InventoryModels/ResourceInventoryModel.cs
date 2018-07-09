using System.Linq;

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

		public ResourceInventoryModel Add(ResourceInventoryModel other)
		{
			var remainder = Zero;

			var newRations = Rations.Value + other.Rations.Value;
			var newFuel = Fuel.Value + other.Fuel.Value;

			if (newRations < 0f) remainder.Rations.Value = Mathf.Abs(newRations);
			if (newFuel < 0f) remainder.Fuel.Value = Mathf.Abs(newFuel);

			Rations.Value = Mathf.Max(0f, newRations);
			Fuel.Value = Mathf.Max(0f, newFuel);

			return remainder;
		}

		public ResourceInventoryModel Subtract(ResourceInventoryModel other)
		{
			return Add(other.Inverse());
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