using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ModuleLogHandler : EncounterLogHandler<ModuleEncounterLogModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Module;

		public ModuleLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ModuleEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var result = new ModuleHandlerModel(logModel);
			result.Entries.Value = logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray();
			result.HaltingDone.Value = linearDone;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}
	}
}