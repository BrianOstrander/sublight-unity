using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ShipModel : Model
	{
		#region Assigned Values
		[JsonProperty] UniversePosition lastSystem;
		[JsonProperty] UniversePosition nextSystem;
		[JsonProperty] UniversePosition currentSystem;
		[JsonProperty] UniversePosition position;
		[JsonProperty] float speed;
		[JsonProperty] float rationConsumption;
		[JsonProperty] float rations;
		[JsonProperty] float fuel;
		[JsonProperty] float fuelConsumption;
		[JsonProperty] float resourceDetection;

		[JsonProperty] OrbitalProbeInventoryModel[] orbitalProbes = new OrbitalProbeInventoryModel[0];
		[JsonProperty] OrbitalCrewInventoryModel[] orbitalCrews = new OrbitalCrewInventoryModel[0];
		[JsonProperty] ResourceInventoryModel resources = new ResourceInventoryModel();

		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> LastSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> NextSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> CurrentSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;

		/// <summary>
		/// Basically the speed of the ship, expressed in universe units per day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Speed;
		/// <summary>
		/// The ration consumption rate in rations per day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> RationConsumption;
		/// <summary>
		/// The fuel consumed per trip, multiplies speed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> FuelConsumption;
		/// <summary>
		/// The likelyhood of detecting resources in a system.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> ResourceDetection;
		#endregion

		#region Derived Values
		[JsonProperty] TravelRadius travelRadius;
		[JsonProperty] float speedTotal;

		/// <summary>
		/// The travel radius of this ship, expressed as a ratio of speed and rations.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<TravelRadius, float, float, float, float> TravelRadius;
		/// <summary>
		/// The total speed of the ship, taking fuel consumption into account.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<float, float, float> SpeedTotal;

		[JsonIgnore]
		public readonly ListenerProperty<InventoryModel[]> Inventory;
		#endregion

		#region Shortcuts
		[JsonIgnore]
		public ResourceInventoryModel Resources { get { return resources; } }
		#endregion

		public ShipModel()
		{
			// Assigned Values

			LastSystem = new ListenerProperty<UniversePosition>(value => lastSystem = value, () => lastSystem);
			NextSystem = new ListenerProperty<UniversePosition>(value => nextSystem = value, () => nextSystem);
			CurrentSystem = new ListenerProperty<UniversePosition>(value => currentSystem = value, () => currentSystem);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Speed = new ListenerProperty<float>(value => speed = value, () => speed);
			RationConsumption = new ListenerProperty<float>(value => rationConsumption = value, () => rationConsumption);
			FuelConsumption = new ListenerProperty<float>(value => fuelConsumption = value, () => fuelConsumption);
			ResourceDetection = new ListenerProperty<float>(value => resourceDetection = value, () => resourceDetection);

			// Derived Values

			TravelRadius = new DerivedProperty<TravelRadius, float, float, float, float>(
				value => travelRadius = value, 
				() => travelRadius,
				DeriveTravelRadius,
				Speed,
				RationConsumption,
				resources.Rations,
				FuelConsumption
			);

			SpeedTotal = new DerivedProperty<float, float, float>(
				value => speedTotal = value,
				() => speedTotal,
				DeriveSpeedTotal,
				Speed,
				FuelConsumption
			);

			Inventory = new ListenerProperty<InventoryModel[]>(OnSetInventory, OnGetInventory);
		}

		#region Utility
		public T[] GetInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			if (predicate == null) return Inventory.Value.OfType<T>().ToArray();
			return Inventory.Value.OfType<T>().Where(predicate).ToArray();
		}

		public T GetInventoryFirstOrDefault<T>(string instanceId) where T : InventoryModel
		{
			return GetInventoryFirstOrDefault<T>(i => i.InstanceId == instanceId);
		}

		public T GetInventoryFirstOrDefault<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			if (predicate == null) return Inventory.Value.OfType<T>().FirstOrDefault();
			return Inventory.Value.OfType<T>().Where(predicate).FirstOrDefault();
		}
		#endregion

		#region Events
		float DeriveSpeedTotal(float speed, float fuelConsumption)
		{
			return speed * fuelConsumption;
		}

		TravelRadius DeriveTravelRadius(
			float speed,
			float rationConsumption,
			float rations,
			float fuelConsumption
		)
		{
			var rationDuration = new DayTime(rations / rationConsumption);
			var rationDistance = rationDuration.TotalTime * DeriveSpeedTotal(speed, fuelConsumption);
			// TODO: Find a better place for handling this weird range stuff?
			// Maybe not... this might be the correct place for it...
			return new TravelRadius(rationDistance * 0.8f, rationDistance * 0.9f, rationDistance);
		}

		void OnSetInventory(InventoryModel[] newInventory)
		{
			var orbitalProbeList = new List<OrbitalProbeInventoryModel>();
			var orbitalCrewList = new List<OrbitalCrewInventoryModel>();

			foreach (var inventory in newInventory)
			{
				switch (inventory.InventoryType)
				{
					case InventoryTypes.OrbitalProbe:
						orbitalProbeList.Add(inventory as OrbitalProbeInventoryModel);
						break;
					case InventoryTypes.OrbitalCrew:
						orbitalCrewList.Add(inventory as OrbitalCrewInventoryModel);
						break;
					case InventoryTypes.Resources:
						resources = inventory as ResourceInventoryModel;
						break;
					default:
						Debug.LogError("Unrecognized InventoryType: " + inventory.InventoryType);
						break;
				}
			}

			orbitalProbes = orbitalProbeList.ToArray();
			orbitalCrews = orbitalCrewList.ToArray();
		}

		InventoryModel[] OnGetInventory()
		{
			return orbitalProbes.Cast<InventoryModel>().Concat(orbitalCrews)
													   .Append(Resources)
													   .ToArray();
		}
		#endregion
	}
}