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
		[JsonProperty] ModuleInventoryModel[] modules = new ModuleInventoryModel[0];
		[JsonProperty] ResourceInventoryModel resources = new ResourceInventoryModel();
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<InventoryModel[]> All;
		#endregion

		#region Shortcuts
		[JsonIgnore]
		public ResourceInventoryModel MaximumResources
		{
			get
			{
				var result = ResourceInventoryModel.Zero;
				foreach (var module in modules) result.Add(module.Slots.MaximumResources);
				return result;
			}
		}

		[JsonIgnore]
		public ResourceInventoryModel Resources { get { return resources; } }

		[JsonIgnore]
		public ResourceInventoryModel UsableResources
		{
			get
			{
				var result = ResourceInventoryModel.Zero;
				Resources.ClampOut(MaximumResources, result);
				return result;
			}
		}

		[JsonIgnore]
		public ResourceInventoryModel UnUsableResources { get { return Resources.ClampOut(MaximumResources); } }
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

		/// <summary>
		/// Gets the usable inventory, entries that are slotted or don't need
		/// to be slotted.
		/// </summary>
		/// <returns>The usable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		public InventoryModel[] GetUsableInventory(Func<InventoryModel, bool> predicate = null)
		{
			return GetUsableInventory<InventoryModel>(predicate);
		}

		/// <summary>
		/// Gets the usable inventory, entries that are slotted or don't need
		/// to be slotted.
		/// </summary>
		/// <returns>The usable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T[] GetUsableInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			var result = GetInventory<T>(i => i.InventoryType != InventoryTypes.Resources && i.IsUsable);
			if (typeof(T) == typeof(InventoryModel) || typeof(T) == typeof(ResourceInventoryModel))
			{
				result = result.Append(UsableResources as T).ToArray();
			}
			if (predicate != null) result = result.Where(predicate).ToArray();
			return result;
		}

		/// <summary>
		/// Gets the unusable inventory, entries that are not slotted but are
		/// required to have one.
		/// </summary>
		/// <returns>The unusable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		public InventoryModel[] GetUnUsableInventory(Func<InventoryModel, bool> predicate = null)
		{
			return GetUnUsableInventory<InventoryModel>(predicate);
		}

		/// <summary>
		/// Gets the unusable inventory, entries that are not slotted but are
		/// required to have one.
		/// </summary>
		/// <returns>The unusable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T[] GetUnUsableInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			var result = GetInventory<T>(i => i.InventoryType != InventoryTypes.Resources && !i.IsUsable);
			if (typeof(T) == typeof(InventoryModel) || typeof(T) == typeof(ResourceInventoryModel))
			{
				var unUsableResources = UnUsableResources;
				if (!unUsableResources.IsZero) result = result.Append(unUsableResources as T).ToArray();
			}
			if (predicate != null) result = result.Where(predicate).ToArray();
			return result;

		}
		#endregion

		#region Events
		void OnSetInventory(InventoryModel[] newInventory)
		{
			var orbitalProbeList = new List<OrbitalProbeInventoryModel>();
			var orbitalCrewList = new List<OrbitalCrewInventoryModel>();
			var moduleList = new List<ModuleInventoryModel>();

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
					case InventoryTypes.Module:
						moduleList.Add(inventory as ModuleInventoryModel);
						break;
					default:
						Debug.LogError("Unrecognized InventoryType: " + inventory.InventoryType);
						break;
				}
			}

			orbitalProbes = orbitalProbeList.ToArray();
			orbitalCrews = orbitalCrewList.ToArray();
			modules = moduleList.ToArray();
		}

		InventoryModel[] OnGetInventory()
		{
			return orbitalProbes.Cast<InventoryModel>().Concat(orbitalCrews)
													   .Concat(modules)
													   .Append(resources)
													   .ToArray();
		}
		#endregion
	}
}