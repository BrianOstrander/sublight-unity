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
			var switches = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).Where(e => !string.IsNullOrEmpty(e.NextLogId.Value)).ToList();

			OnFilter(
				null,
				null,
				switches,
				(status, result) => OnDone(status, result, logModel, nonLinearDone)
			);
		}

		void OnFilter(
			bool? result,
			string resultId,
			List<SwitchEntryModel> remaining,
			Action<RequestStatus, string> done
		)
		{
			if (result.HasValue && result.Value)
			{
				done(RequestStatus.Success, resultId);
				return;
			}

			if (!remaining.Any())
			{
				done(RequestStatus.Failure, null);
				return;
			}

			var next = remaining.First();
			var nextId = next.NextLogId.Value;
			remaining.RemoveAt(0);

			Configuration.ValueFilter.Filter(
				filterResult => OnFilter(filterResult, nextId, remaining, done),
				next.Filtering,
				Configuration.Model,
				Configuration.Encounter
			);
		}

		void OnDone(RequestStatus status, string nextLogId, SwitchEncounterLogModel logModel, Action<string> done)
		{
			if (status == RequestStatus.Success) done(nextLogId);
			else done(logModel.NextLog);
		}
	}
}