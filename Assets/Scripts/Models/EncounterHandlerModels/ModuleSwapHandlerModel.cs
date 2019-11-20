using System;
using System.Linq;
using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleSwapHandlerModel : EncounterHandlerModel<ModuleSwapEncounterLogModel>
	{
		[Serializable]
		public struct State
		{
			public readonly ModuleModel[] Available;
			public readonly ModuleModel[] Current;
			public readonly ModuleModel[] Removed;

			public State(
				ModuleModel[] available = null,
				ModuleModel[] current = null,
				ModuleModel[] removed = null
			)
			{
				Available = (available ?? new ModuleModel[0]).OrderBy(m => m.Type.Value).ToArray();
				Current = (current ?? new ModuleModel[0]).OrderBy(m => m.Type.Value).ToArray();
				Removed = (removed ?? new ModuleModel[0]).OrderBy(m => m.Type.Value).ToArray();
			}
		}
		
		[JsonProperty] State initialState;
		[JsonIgnore] public readonly ListenerProperty<State> InitialState;
		
		[JsonProperty] State finalState;
		[JsonIgnore] public readonly ListenerProperty<State> FinalState;

		Action done;
		[JsonIgnore] public readonly ListenerProperty<Action> Done;

		public ModuleSwapHandlerModel(ModuleSwapEncounterLogModel log) : base(log)
		{
			InitialState = new ListenerProperty<State>(value => initialState = value, () => initialState);
			FinalState = new ListenerProperty<State>(value => finalState = value, () => finalState);
			Done = new ListenerProperty<Action>(value => done = value, () => done);
		}
	}
}