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
					default:
						Debug.LogError("Unrecognized InventoryOperation: " + entry.Operation);
						break;
				}
			}

			addResources = addResourceList.ToArray();
			addInstances = addInstanceList.ToArray();
		}

		InventoryOperationModel[] OnGetOperations()
		{
			return addResources.Cast<InventoryOperationModel>().Concat(addInstances)
															   .ToArray();
		}
		#endregion
	}
}