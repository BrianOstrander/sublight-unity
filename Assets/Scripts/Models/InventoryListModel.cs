using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InventoryListModel : Model
	{
		#region Assigned Values
		[JsonProperty] OrbitalProbeInventoryModel[] orbitalProbes = new OrbitalProbeInventoryModel[0];
		[JsonProperty] OrbitalCrewInventoryModel[] orbitalCrews = new OrbitalCrewInventoryModel[0];
		[JsonProperty] ResourceInventoryModel resources = new ResourceInventoryModel();
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<InventoryModel[]> All;
		#endregion

		#region Shortcuts
		[JsonIgnore]
		public ResourceInventoryModel Resources { get { return resources; } }
		#endregion

		public InventoryListModel()
		{
			// Derived Values
			All = new ListenerProperty<InventoryModel[]>(OnSetInventory, OnGetInventory);
		}

		#region Utility
		public T[] GetInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			if (predicate == null) return All.Value.OfType<T>().ToArray();
			return All.Value.OfType<T>().Where(predicate).ToArray();
		}

		public T GetInventoryFirstOrDefault<T>(string instanceId) where T : InventoryModel
		{
			return GetInventoryFirstOrDefault<T>(i => i.InstanceId == instanceId);
		}

		public T GetInventoryFirstOrDefault<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			if (predicate == null) return All.Value.OfType<T>().FirstOrDefault();
			return All.Value.OfType<T>().FirstOrDefault(predicate);
		}

		public InventoryTypes[] GetTypes()
		{
			return All.Value.Select(i => i.InventoryType).Distinct().ToArray();
		}
		#endregion

		#region Events
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