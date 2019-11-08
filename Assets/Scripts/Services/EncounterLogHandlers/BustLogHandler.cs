using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class BustLogHandler : EncounterLogHandler<BustEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Bust; } }

		public BustLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			BustEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var bustEvents = logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray();

			var result = new BustHandlerModel(logModel);
			// If there are any non-instant focuses this event is halting.
			result.HasHaltingEvents.Value = bustEvents.Any(b => b.Operation.Value == BustEdgeModel.Operations.Focus && !b.FocusInfo.Value.Instant);
			result.Entries.Value = bustEvents;
			result.HaltingDone.Value = linearDone;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}
	}
}