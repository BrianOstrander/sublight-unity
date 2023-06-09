﻿using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class DialogLogHandler : EncounterLogHandler<DialogEncounterLogModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Dialog;

		public DialogLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			DialogEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var dialogs = logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray();

			FilterFirst(
				(status, result) => OnDone(status, result, logModel, nonLinearDone),
				entry => entry.Filtering,
				dialogs
			);
		}

		void OnDone(RequestStatus status, DialogEdgeModel edge, DialogEncounterLogModel logModel, Action<string> done)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled dialogs found.
				done(logModel.NextLog);
				return;
			}

			var fallthroughId = logModel.NextLog;
			var successId = GetValidId(edge.SuccessLogId.Value, fallthroughId);
			var failureId = GetValidId(edge.FailureLogId.Value, fallthroughId);
			var cancelId = GetValidId(edge.CancelLogId.Value, fallthroughId);
			
			var request = new DialogHandlerModel(logModel);

			request.Dialog.Value = new DialogLogBlock(
				edge.Title.Value,
				edge.Message.Value,
				edge.DialogType.Value,
				edge.DialogStyle.Value,
				edge.SuccessText.Value,
				edge.FailureText.Value,
				edge.CancelText.Value,
				() => done(successId),
				() => done(failureId),
				() => done(cancelId)
			);

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(request));
		}

		string GetValidId(string target, string fallback) { return string.IsNullOrEmpty(target) ? fallback : target; }
	}
}