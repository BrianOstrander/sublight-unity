using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;
using UnityEngine;

namespace LunraGames.SubLight
{
	public class ModuleSwapLogHandler : EncounterLogHandler<ModuleSwapEncounterLogModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.ModuleSwap;

		public ModuleSwapLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ModuleSwapEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			FilterAll(
				(status, result) => OnHandleFilterAll(status, result, linearDone),
				edge => edge.Filtering,
				logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray()		
			);
		}

		void OnHandleFilterAll(
			RequestStatus status,
			ModuleSwapEdgeModel[] filtered,
			Action linearDone
		)
		{
			if (status != RequestStatus.Success || filtered.None())
			{
				linearDone();
				return;
			}

			Configuration.Model.Context.ModuleService.GenerateModulesWithTraits(
				filtered.Select(f => new ModuleService.ModuleConstraintWithTraitLimits(f.ModuleConstraint.Value, f.Traits.Value)).ToArray(),
				result => OnHandleGeneration(result, linearDone)
			);
		}

		void OnHandleGeneration(
			ResultArray<ModuleModel> result,
			Action linearDone
		)
		{
			switch (result.Status)
			{
				case RequestStatus.Success: break;
				default:
					result.LogIfNotSuccess("The following error interrupted module swap log handling generation, skipping");
					linearDone();
					return;
			}

			var text = "The following modules were generated:";
			var index = 0;
			foreach (var module in result.Payloads)
			{
				text += "\n[ " + index + " ] : " + module.ToReadableJson();
				index++;
			}
			Debug.Log(text);

			linearDone();
		}

		/*
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

			Action successClick = () => done(successId);
			Action failureClick = () => done(failureId);
			Action cancelClick = () => done(cancelId);

			var result = new DialogHandlerModel(
				logModel
			);

			result.Dialog.Value = new DialogLogBlock(
				edge.Title.Value,
				edge.Message.Value,
				edge.DialogType.Value,
				edge.DialogStyle.Value,
				edge.SuccessText.Value,
				edge.FailureText.Value,
				edge.CancelText.Value,
				successClick,
				failureClick,
				cancelClick
			);

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}

		string GetValidId(string target, string fallback) { return string.IsNullOrEmpty(target) ? fallback : target; }
		*/
	}
}