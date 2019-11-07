using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterEventLogHandler : EncounterLogHandler<EncounterEventEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Event; } }

		public EncounterEventLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			EncounterEventEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var events = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToList();

			Action<RequestStatus, List<EncounterEventEntryModel>> filteringDone = (status, filtered) => OnDone(status, filtered, logModel, linearDone);

			OnFilter(
				null,
				events,
				new List<EncounterEventEntryModel>(),
				filteringDone
			);
		}

		void OnFilter(
			EncounterEventEntryModel result,
			List<EncounterEventEntryModel> remaining,
			List<EncounterEventEntryModel> filtered,
			Action<RequestStatus, List<EncounterEventEntryModel>> filteringDone
		)
		{
			if (result != null) filtered.Add(result);

			if (remaining.None())
			{
				// No remaining to filter.
				if (filtered.Any()) filteringDone(RequestStatus.Success, filtered); // There are callable events.
				else filteringDone(RequestStatus.Failure, null); // There are no callable events.
				return;
			}

			Action<EncounterEventEntryModel> nextDone = filterResult => OnFilter(filterResult, remaining, filtered, filteringDone);
			var next = remaining.First();
			remaining.RemoveAt(0);

			Configuration.ValueFilter.Filter(
				filterResult => OnFiltering(filterResult, next, nextDone),
				next.Filtering,
				Configuration.Model,
				Configuration.Encounter
			);
		}

		void OnFiltering(
			bool filteringResult,
			EncounterEventEntryModel possibleResult,
			Action<EncounterEventEntryModel> nextDone
		)
		{
			if (filteringResult) nextDone(possibleResult);
			else nextDone(null);
		}

		void OnDone(
			RequestStatus status,
			List<EncounterEventEntryModel> events,
			EncounterEventEncounterLogModel logModel,
			Action done
		)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled and callable events found.
				done();
				return;
			}

			var result = new EncounterEventHandlerModel(
				logModel
			);
			result.Log.Value = logModel;
			result.Events.Value = events.ToArray();
			result.AlwaysHalting.Value = logModel.AlwaysHalting.Value;
			result.HasHaltingEvents.Value = events.Any(e => e.IsHalting.Value);
			result.HaltingDone.Value = done;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}
	}
}