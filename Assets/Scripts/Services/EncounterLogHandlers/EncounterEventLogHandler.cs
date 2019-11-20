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
			var events = logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToList();

			Action<RequestStatus, List<EncounterEventEdgeModel>> filteringDone = (status, filtered) => OnDone(status, filtered, logModel, linearDone);

			OnFilter(
				null,
				events,
				new List<EncounterEventEdgeModel>(),
				filteringDone
			);
		}

		void OnFilter(
			EncounterEventEdgeModel result,
			List<EncounterEventEdgeModel> remaining,
			List<EncounterEventEdgeModel> filtered,
			Action<RequestStatus, List<EncounterEventEdgeModel>> filteringDone
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

			Action<EncounterEventEdgeModel> nextDone = filterResult => OnFilter(filterResult, remaining, filtered, filteringDone);
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
			EncounterEventEdgeModel possibleResult,
			Action<EncounterEventEdgeModel> nextDone
		)
		{
			if (filteringResult) nextDone(possibleResult);
			else nextDone(null);
		}

		void OnDone(
			RequestStatus status,
			List<EncounterEventEdgeModel> events,
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

			var request = new EncounterEventHandlerModel(
				logModel
			);
			request.Log.Value = logModel;
			request.Events.Value = events.ToArray();
			request.AlwaysHalting.Value = logModel.AlwaysHalting.Value;
			request.HasHaltingEvents.Value = events.Any(e => e.IsHalting.Value);
			request.HaltingDone.Value = done;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(request));
		}
	}
}