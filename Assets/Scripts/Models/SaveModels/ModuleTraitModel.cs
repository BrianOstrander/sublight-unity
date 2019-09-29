using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleTraitModel : SaveModel
	{
		[JsonProperty] string name;
		/// <summary>
		/// The name of this encounter, will be seen by the player and listed as
		/// the meta data.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] string description;
		/// <summary>
		/// An internal description of this encounter, only for editor use.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Description;

		[JsonProperty] ModuleTraitSeverity severity;
		[JsonIgnore] public readonly ListenerProperty<ModuleTraitSeverity> Severity;

		[JsonProperty] ModuleTypes compatibleModuleTypes;
		[JsonIgnore] public readonly ListenerProperty<ModuleTypes> CompatibleModuleTypes;

		[JsonProperty] string[] incompatibleTraitIds;
		[JsonIgnore] public readonly ListenerProperty<string[]> IncompatibleTraitIds;

		public ModuleTraitModel()
		{
			SaveType = SaveTypes.ModuleTrait;
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Severity = new ListenerProperty<ModuleTraitSeverity>(value => severity = value, () => severity);
			CompatibleModuleTypes = new ListenerProperty<ModuleTypes>(value => compatibleModuleTypes = value, () => compatibleModuleTypes);
			IncompatibleTraitIds = new ListenerProperty<string[]>(value => incompatibleTraitIds = value, () => incompatibleTraitIds);
		}
	}
}