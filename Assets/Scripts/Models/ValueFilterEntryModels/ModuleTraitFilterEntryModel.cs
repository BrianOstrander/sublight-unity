using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleTraitFilterEntryModel : ValueFilterEntryModel<string>
	{
		[JsonProperty] ModuleTraitFilterOperations operation;
		[JsonIgnore] public readonly ListenerProperty<ModuleTraitFilterOperations> Operation;
		[JsonProperty] ModuleTypes[] validModuleTypes = new ModuleTypes[0];
		[JsonIgnore] public readonly ListenerProperty<ModuleTypes[]> ValidModuleTypes;

		public override ValueFilterTypes FilterType => ValueFilterTypes.ModuleTrait;
		public override KeyValueTypes FilterValueType => KeyValueTypes.String;
		
		public ModuleTraitFilterEntryModel()
		{
			Operation = new ListenerProperty<ModuleTraitFilterOperations>(value => operation = value, () => operation);
			ValidModuleTypes = new ListenerProperty<ModuleTypes[]>(value => validModuleTypes = value, () => validModuleTypes);
		}
	}
}