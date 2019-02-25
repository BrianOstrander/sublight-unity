using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class SwitchLogHandler : EncounterLogHandler<SwitchEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Switch; } }

		public SwitchLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			SwitchEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var switches = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).ToArray();

			FilterFirst(
				(status, result) => OnDone(status, result, logModel, nonLinearDone),
				entry => entry.Filtering,
				switches
			);
		}

		void OnDone(RequestStatus status, SwitchEntryModel entry, SwitchEncounterLogModel logModel, Action<string> done)
		{
			if (status == RequestStatus.Success) done(string.IsNullOrEmpty(entry.NextLogId.Value) ? logModel.NextLog : entry.NextLogId.Value);
			else done(logModel.NextLog);
		}
	}
}