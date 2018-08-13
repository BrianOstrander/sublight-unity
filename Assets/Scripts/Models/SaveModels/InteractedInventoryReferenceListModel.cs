using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InteractedInventoryReferenceListModel : SaveModel
	{
		[JsonProperty] InteractedInventoryReferenceModel[] references = new InteractedInventoryReferenceModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<InteractedInventoryReferenceModel[]> References;

		public InteractedInventoryReferenceListModel()
		{
			SaveType = SaveTypes.InteractedInventoryReferenceList;
			References = new ListenerProperty<InteractedInventoryReferenceModel[]>(value => references = value, () => references);
		}

		public InteractedInventoryReferenceModel GetReference(string inventory)
		{
			var result = References.Value.FirstOrDefault(e => e.InventoryId.Value == inventory);
			if (result == null)
			{
				result = new InteractedInventoryReferenceModel();
				result.InventoryId.Value = inventory;
				References.Value = References.Value.Append(result).ToArray();
			}
			return result;
		}
	}
}