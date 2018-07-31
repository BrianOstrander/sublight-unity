using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ModuleInventoryModel : InventoryModel
	{
		[JsonProperty] bool isRoot;
		[JsonProperty] ModuleSlotListModel slots = new ModuleSlotListModel();

		[JsonIgnore]
		public readonly ListenerProperty<bool> IsRoot;

		[JsonIgnore]
		public ModuleSlotListModel Slots { get { return slots; } }

		public override InventoryTypes InventoryType { get { return InventoryTypes.Module; } }
		public override bool IsUsable { get { return base.IsUsable || IsRoot.Value; } }

		public ModuleInventoryModel()
		{
			IsRoot = new ListenerProperty<bool>(value => isRoot = value, () => isRoot);

			InstanceId.Changed += OnInstanceId;
		}

		#region Events
		void OnInstanceId(string newInstanceId)
		{
			slots.ParentId.Value = newInstanceId;
		}
		#endregion
	}
}