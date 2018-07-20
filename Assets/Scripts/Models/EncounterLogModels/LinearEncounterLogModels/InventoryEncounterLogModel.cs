using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InventoryEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] AddResourceOperationModel[] addResources = new AddResourceOperationModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<InventoryOperationModel[]> Operations; 

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Inventory; } }

		public InventoryEncounterLogModel()
		{
			Operations = new ListenerProperty<InventoryOperationModel[]>(OnSetOperations, OnGetOperations);
		}

		#region Events
		void OnSetOperations(InventoryOperationModel[] entries)
		{
			var addResourceList = new List<AddResourceOperationModel>();

			foreach (var entry in entries)
			{
				switch (entry.Operation)
				{
					case InventoryOperations.AddResource:
						addResourceList.Add(entry as AddResourceOperationModel);
						break;
					default:
						Debug.LogError("Unrecognized InventoryOperation: " + entry.Operation);
						break;
				}
			}

			addResources = addResourceList.ToArray();
		}

		InventoryOperationModel[] OnGetOperations()
		{
			return addResources.ToArray();
		}
		#endregion
	}
}