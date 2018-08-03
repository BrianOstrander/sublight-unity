using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InventoryReferenceListModel : Model
	{
		#region Assigned Values
		[JsonProperty] OrbitalCrewReferenceModel[] orbitalCrews = new OrbitalCrewReferenceModel[0];
		[JsonProperty] ModuleReferenceModel[] modules = new ModuleReferenceModel[0];
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<IInventoryReferenceModel[]> All;
		#endregion

		public InventoryReferenceListModel()
		{
			// Derived Values
			All = new ListenerProperty<IInventoryReferenceModel[]>(OnSetReferences, OnGetReferences);
		}

		#region Utility
		public IInventoryReferenceModel[] GetReferences(Func<IInventoryReferenceModel, bool> predicate = null)
		{
			return GetReferences<IInventoryReferenceModel>(predicate);
		}

		public T[] GetReferences<T>(Func<T, bool> predicate = null) where T : IInventoryReferenceModel
		{
			if (predicate == null) return All.Value.OfType<T>().ToArray();
			return All.Value.OfType<T>().Where(predicate).ToArray();
		}

		public IInventoryReferenceModel GetReferenceFirstOrDefault(string inventoryId)
		{
			return GetReferenceFirstOrDefault<IInventoryReferenceModel>(inventoryId);
		}

		public T GetReferenceFirstOrDefault<T>(string inventoryId) where T : IInventoryReferenceModel
		{
			return GetReferenceFirstOrDefault<T>(i => i.RawModel.InventoryId.Value == inventoryId);
		}

		public IInventoryReferenceModel GetReferenceFirstOrDefault(Func<IInventoryReferenceModel, bool> predicate = null)
		{
			return GetReferenceFirstOrDefault<IInventoryReferenceModel>(predicate);
		}

		public T GetReferenceFirstOrDefault<T>(Func<T, bool> predicate = null) where T : IInventoryReferenceModel
		{
			if (predicate == null) return All.Value.OfType<T>().FirstOrDefault();
			return All.Value.OfType<T>().FirstOrDefault(predicate);
		}

		public InventoryTypes[] GetTypes()
		{
			return All.Value.Select(i => i.RawModel.InventoryType).Distinct().ToArray();
		}
		#endregion

		#region Events
		void OnSetReferences(IInventoryReferenceModel[] newReferences)
		{
			var orbitalCrewList = new List<OrbitalCrewReferenceModel>();
			var moduleList = new List<ModuleReferenceModel>();

			foreach (var reference in newReferences)
			{
				switch (reference.RawModel.InventoryType)
				{
					case InventoryTypes.OrbitalCrew:
						orbitalCrewList.Add(reference as OrbitalCrewReferenceModel);
						break;
					case InventoryTypes.Module:
						moduleList.Add(reference as ModuleReferenceModel);
						break;
					default:
						Debug.LogError("Unrecognized InventoryType: " + reference.RawModel.InventoryType);
						break;
				}
			}

			orbitalCrews = orbitalCrewList.ToArray();
			modules = moduleList.ToArray();
		}

		IInventoryReferenceModel[] OnGetReferences()
		{
			return orbitalCrews.Cast<IInventoryReferenceModel>().Concat(modules)
																.ToArray();
		}
		#endregion
	}
}