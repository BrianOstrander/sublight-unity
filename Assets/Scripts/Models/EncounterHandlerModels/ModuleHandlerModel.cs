using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleHandlerModel : EncounterHandlerModel<ModuleEncounterLogModel>
	{
		[JsonProperty] ModuleEdgeModel[] entries;
		[JsonIgnore] public readonly ListenerProperty<ModuleEdgeModel[]> Entries;

		Action haltingDone;
		[JsonIgnore] public readonly ListenerProperty<Action> HaltingDone;

		public ModuleHandlerModel(ModuleEncounterLogModel log) : base(log)
		{
			Entries = new ListenerProperty<ModuleEdgeModel[]>(value => entries = value, () => entries);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}