using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InventoryEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] AddResourceOperationModel[] addResources = new AddResourceOperationModel[0];
		[JsonProperty] AddInstanceOperationModel[] addInstances = new AddInstanceOperationModel[0];
		[JsonProperty] AddRandomInstanceOperationModel[] addRandomInstances = new AddRandomInstanceOperationModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<InventoryOperationModel[]> Operations; 

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Inventory; } }

		public override bool EditableDuration { get { return false; } }

		public InventoryEncounterLogModel()
		{
			Operations = new ListenerProperty<InventoryOperationModel[]>(OnSetOperations, OnGetOperations);
		}

		#region Events
		void OnSetOperations(InventoryOperationModel[] entries)
		{
			var addResourceList = new List<AddResourceOperationModel>();
			var addInstanceList = new List<AddInstanceOperationModel>();
			var addRandomInstanceList = new List<AddRandomInstanceOperationModel>();

			foreach (var entry in entries)
			{
				switch (entry.Operation)
				{
					case InventoryOperations.AddResources:
						addResourceList.Add(entry as AddResourceOperationModel);
						break;
					case InventoryOperations.AddInstance:
						addInstanceList.Add(entry as AddInstanceOperationModel);
						break;
					case InventoryOperations.AddRandomInstance:
						addRandomInstanceList.Add(entry as AddRandomInstanceOperationModel);
						break;
					default:
						Debug.LogError("Unrecognized InventoryOperation: " + entry.Operation);
						break;
				}
			}

			addResources = addResourceList.ToArray();
			addInstances = addInstanceList.ToArray();
			addRandomInstances = addRandomInstanceList.ToArray();
		}

		InventoryOperationModel[] OnGetOperations()
		{
			return addResources.Cast<InventoryOperationModel>().Concat(addInstances)
															   .Concat(addRandomInstances)
															   .ToArray();
		}
		#endregion
	}
}