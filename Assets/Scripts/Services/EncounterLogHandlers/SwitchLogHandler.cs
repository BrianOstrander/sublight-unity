using System;
using System.Linq;

using UnityEngine;

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
			var switches = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray();

			switch (logModel.SelectionMethod.Value)
			{
				case SwitchEncounterLogModel.SelectionMethods.FirstFilter:
					FilterFirst(
						(status, result) => OnDone(status, result, logModel, nonLinearDone),
						entry => entry.Filtering,
						switches
					);
					break;
				case SwitchEncounterLogModel.SelectionMethods.Random:
				case SwitchEncounterLogModel.SelectionMethods.RandomWeighted:
					FilterAll(
						(status, result) => OnHandleFilterAll(status, result, logModel, nonLinearDone),
						entry => entry.Filtering,
						switches
					);
					break;
				default:
					Debug.LogError("Unrecognized SelectionMethod: " + logModel.SelectionMethod.Value + ", attempting to skip...");
					OnDone(RequestStatus.Failure, null, logModel, nonLinearDone);
					break;
			}
		}

		void OnHandleFilterAll(
			RequestStatus status,
			SwitchEntryModel[] filtered,
			SwitchEncounterLogModel logModel,
			Action<string> nonLinearDone
		)
		{
			if (status != RequestStatus.Success || filtered.None())
			{
				OnDone(
					RequestStatus.Failure,
					null,
					logModel,
					nonLinearDone
				);
				return;
			}

			switch (logModel.SelectionMethod.Value)
			{
				case SwitchEncounterLogModel.SelectionMethods.Random:
					OnDone(
						RequestStatus.Success,
						filtered.Random(),
						logModel,
						nonLinearDone
					);
					break;
				case SwitchEncounterLogModel.SelectionMethods.RandomWeighted:
					OnDone(
						RequestStatus.Success,
						filtered.RandomWeighted(e => e.RandomWeight.Value),
						logModel,
						nonLinearDone
					);
					break;
				default:
					Debug.LogError("Unrecognized random SelectionMethod: " + logModel.SelectionMethod.Value + ", attempting to skip...");
					OnDone(
						RequestStatus.Failure,
						null,
						logModel,
						nonLinearDone
					);
					break;
			}
		}

		void OnDone(RequestStatus status, SwitchEntryModel entry, SwitchEncounterLogModel logModel, Action<string> done)
		{
			if (status == RequestStatus.Success) done(string.IsNullOrEmpty(entry.NextLogId.Value) ? logModel.NextLog : entry.NextLogId.Value);
			else done(logModel.NextLog);
		}
	}
}