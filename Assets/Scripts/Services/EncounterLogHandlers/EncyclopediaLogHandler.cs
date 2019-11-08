using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncyclopediaLogHandler : EncounterLogHandler<EncyclopediaEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Encyclopedia; } }

		public EncyclopediaLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			EncyclopediaEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			Configuration.Model.Encyclopedia.Add(logModel.Edges.Value.Select(e => e.Duplicate).ToArray());

			linearDone();
		}
	}
}