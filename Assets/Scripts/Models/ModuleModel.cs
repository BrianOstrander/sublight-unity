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
		
		[JsonProperty] float transitRange;
		/// <summary>
		/// Maximum transit range in universe units.
		/// </summary>
		/// <remarks>
		/// This may not be the actual maximum range a player can travel per turn,
		/// but rather the maximum before certain encounters are triggered.
		/// </remarks>
		[JsonIgnore] public readonly ListenerProperty<float> TransitRange;
		
		[JsonProperty] float transitVelocity;
		/// <summary>
		/// The maximum velocity as a fraction of the speed of light.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<float> TransitVelocity;
		
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
			TransitRange = new ListenerProperty<float>(value => transitRange = value, () => transitRange);
			TransitVelocity = new ListenerProperty<float>(value => transitVelocity = value, () => transitVelocity);
			TraitIds = new ListenerProperty<string[]>(value => traitIds = value, () => traitIds);
			RepairCost = new ListenerProperty<float>(value => repairCost = value, () => repairCost);
		}
	}
}