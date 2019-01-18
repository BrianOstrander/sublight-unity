using System;
using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class DialogLogHandler : EncounterLogHandler<DialogEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Dialog; } }

		public DialogLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			DialogEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var dialogs = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).ToArray();

			FilterFirst(
				(status, result) => OnDone(status, result, logModel, nonLinearDone),
				entry => entry.Filtering,
				dialogs
			);
		}

		void OnDone(RequestStatus status, DialogEntryModel entry, DialogEncounterLogModel logModel, Action<string> done)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled dialogs found.
				done(logModel.NextLog);
				return;
			}

			var fallthroughId = logModel.NextLog;
			var successId = GetValidId(entry.SuccessLogId.Value, fallthroughId);
			var failureId = GetValidId(entry.FailureLogId.Value, fallthroughId);
			var cancelId = GetValidId(entry.CancelLogId.Value, fallthroughId);

			Action successClick = () => done(successId);
			Action failureClick = () => done(failureId);
			Action cancelClick = () => done(cancelId);

			var result = new DialogHandlerModel(
				logModel
			);

			result.Dialog.Value = new DialogLogBlock(
				entry.Title.Value,
				entry.Message.Value,
				entry.DialogType.Value,
				entry.DialogStyle.Value,
				entry.SuccessText.Value,
				entry.FailureText.Value,
				entry.CancelText.Value,
				successClick,
				failureClick,
				cancelClick
			);

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}

		string GetValidId(string target, string fallback) { return string.IsNullOrEmpty(target) ? fallback : target; }
	}
}