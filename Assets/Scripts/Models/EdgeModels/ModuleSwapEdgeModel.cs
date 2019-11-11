using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleSwapEdgeModel : EdgeModel
	{
		[JsonProperty] ModuleService.ModuleConstraint moduleConstraint = ModuleService.ModuleConstraint.Default;
		[JsonIgnore] public readonly ListenerProperty<ModuleService.ModuleConstraint> ModuleConstraint;
		
		[JsonProperty] ModuleService.TraitLimit[] traits = new ModuleService.TraitLimit[0];
		[JsonIgnore] public readonly ListenerProperty<ModuleService.TraitLimit[]> Traits;
		
		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore] public ValueFilterModel Filtering => filtering;
		
		public ModuleSwapEdgeModel()
		{
			ModuleConstraint = new ListenerProperty<ModuleService.ModuleConstraint>(value => moduleConstraint = value, () => moduleConstraint);
			Traits = new ListenerProperty<ModuleService.TraitLimit[]>(value => traits = value, () => traits);
		}
	}
}