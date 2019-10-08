using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleModel : Model
	{
		#region Serialized
		[JsonProperty] string name;
		/// <summary>
		/// The procedurally generated name of this module seen by the player.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Name;
		
		[JsonProperty] string yearManufactured;
		[JsonIgnore] public readonly ListenerProperty<string> YearManufactured;
		
		[JsonProperty] string manufacturerId;
		[JsonIgnore] public readonly ListenerProperty<string> ManufacturerId;
		
		[JsonProperty] string description;
		[JsonIgnore] public readonly ListenerProperty<string> Description;
		
		[JsonProperty] ModuleTypes type;
		[JsonIgnore] public readonly ListenerProperty<ModuleTypes> Type;
		
		[JsonProperty] float powerProduction;
		[JsonIgnore] public readonly ListenerProperty<float> PowerProduction;
		
		[JsonProperty] float powerConsumption;
		[JsonIgnore] public readonly ListenerProperty<float> PowerConsumption;
		
		[JsonProperty] float navigationRange;
		[JsonIgnore] public readonly ListenerProperty<float> NavigationRange;
		
		[JsonProperty] float navigationVelocity;
		[JsonIgnore] public readonly ListenerProperty<float> NavigationVelocity;
		
		[JsonProperty] string[] traitIds = new string[0];
		[JsonIgnore] public readonly ListenerProperty<string[]> TraitIds;
		
		[JsonProperty] float repairCost;
		[JsonIgnore] public readonly ListenerProperty<float> RepairCost;
		
		#endregion
		
		public ModuleModel()
		{
			Name = new ListenerProperty<string>(value => name = value, () => name);
			YearManufactured = new ListenerProperty<string>(value => yearManufactured = value, () => yearManufactured);
			ManufacturerId = new ListenerProperty<string>(value => manufacturerId = value, () => manufacturerId);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Type = new ListenerProperty<ModuleTypes>(value => type = value, () => type);
			PowerProduction = new ListenerProperty<float>(value => powerProduction = value, () => powerProduction);
			PowerConsumption = new ListenerProperty<float>(value => powerConsumption = value, () => powerConsumption);
			NavigationRange = new ListenerProperty<float>(value => navigationRange = value, () => navigationRange);
			NavigationVelocity = new ListenerProperty<float>(value => navigationVelocity = value, () => navigationVelocity);
			TraitIds = new ListenerProperty<string[]>(value => traitIds = value, () => traitIds);
			RepairCost = new ListenerProperty<float>(value => repairCost = value, () => repairCost);
		}
	}
}