using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ButtonLogHandler : EncounterLogHandler<ButtonEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Button; } }

		public ButtonLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ButtonEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var buttons = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToList();

			Action<RequestStatus, List<ButtonLogBlock>> filteringDone = (status, filtered) => OnDone(status, filtered, logModel, nonLinearDone);

			OnFilter(
				logModel.NextLog,
				null,
				buttons,
				new List<ButtonLogBlock>(),
				nonLinearDone,
				filteringDone
			);
		}

		void OnFilter(
			string fallbackLogId,
			ButtonLogBlock? result,
			List<ButtonEdgeModel> remaining,
			List<ButtonLogBlock> filtered,
			Action<string> done,
			Action<RequestStatus, List<ButtonLogBlock>> filteringDone
		)
		{
			if (result.HasValue) filtered.Add(result.Value);

			if (remaining.None())
			{
				// No remaining to filter.
				if (filtered.Where(f => f.Interactable).Any()) filteringDone(RequestStatus.Success, filtered); // There are interactable buttons.
				else filteringDone(RequestStatus.Failure, null); // There are no interactable buttons.
				return;
			}

			Action<ButtonLogBlock?> nextDone = filterResult => OnFilter(fallbackLogId, filterResult, remaining, filtered, done, filteringDone);
			var next = remaining.First();
			remaining.RemoveAt(0);
			var possibleResult = new ButtonLogBlock(
				next.Message.Value,
				false,
				true,
				() => OnClick(next, () => done(string.IsNullOrEmpty(next.NextLogId.Value) ? fallbackLogId : next.NextLogId.Value))
			);

			if (next.AutoDisableEnabled.Value)
			{
				// When this button is pressed, it gets disabled, so we have to check.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						next.AutoDisabledKey,
						kvResult => OnAutoDisabled(kvResult, next, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disabled check.
				OnAutoDisabled(
					null,
					next,
					possibleResult,
					nextDone
				);
			}
		}

		void OnAutoDisabled(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled check, so see the result.
				if (kvResult.Value.Value)
				{
					// It has been auto disabled.
					nextDone(null);
					return;
				}
			}

			if (edge.AutoDisableInteractions.Value)
			{
				// When this button is pressed, interactions get disabled, so we have to check.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						edge.AutoDisabledInteractionsKey,
						interactionKvResult => OnAutoDisabledInteractions(interactionKvResult, edge, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disable interactions check.
				OnAutoDisabledInteractions(
					null,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnAutoDisabledInteractions(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Interactable = !kvResult.Value.Value;
			}

			if (!edge.NotAutoUsed.Value)
			{
				// When this button is pressed, it gets marked as used, so we have to check to see if that happened.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						edge.AutoUsedKey,
						autoUsedKvResult => OnAutoUsed(autoUsedKvResult, edge, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto used check.
				OnAutoUsed(
					null,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnAutoUsed(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Used = kvResult.Value.Value;
			}

			Configuration.ValueFilter.Filter(
				filterResult => OnEnabledFiltering(filterResult, edge, possibleResult, nextDone),
				edge.EnabledFiltering,
				Configuration.Model,
				Configuration.Encounter
			);
		}

		void OnEnabledFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (!filteringResult)
			{
				// This button isn't enabled.
				nextDone(null);
				return;
			}

			if (possibleResult.Interactable)
			{
				// It hasn't automatically been disabled, so we check the filter.
				Configuration.ValueFilter.Filter(
					filterResult => OnInteractableFiltering(filterResult, edge, possibleResult, nextDone),
					edge.InteractableFiltering,
					Configuration.Model,
					Configuration.Encounter
				);
			}
			else
			{
				// Already not interactable, so bypass the filter.
				OnInteractableFiltering(
					false,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnInteractableFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Interactable &= filteringResult;

			if (!possibleResult.Used)
			{
				// Hasn't been auto marked as used, so we have to check the filter.
				Configuration.ValueFilter.Filter(
					filterResult => OnUsedFiltering(filterResult, edge, possibleResult, nextDone),
					edge.UsedFiltering,
					Configuration.Model,
					Configuration.Encounter
				);
			}
			else
			{
				// Auto using made it used, so we bypass the filter.
				OnUsedFiltering(
					true,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnUsedFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Used |= filteringResult;

			nextDone(possibleResult);
		}

		void OnDone(RequestStatus status, List<ButtonLogBlock> buttons, ButtonEncounterLogModel logModel, Action<string> done)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled and interactable buttons found.
				done(logModel.NextLog);
				return;
			}

			var result = new ButtonHandlerModel(
				logModel
			);
			result.Log.Value = logModel;
			result.Buttons.Value = buttons.ToArray();

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}

		#region Handler Callbacks
		void OnClick(ButtonEdgeModel edge, Action done)
		{
			if (!edge.NotAutoUsed.Value)
			{
				// We need to set this to be used.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						edge.AutoUsedKey,
						true,
						result => OnClickAutoUse(edge, done)
					)
				);
			}
			else
			{
				// Bypass setting it to be used.
				OnClickAutoUse(edge, done);
			}
		}

		void OnClickAutoUse(ButtonEdgeModel edge, Action done)
		{
			if (edge.AutoDisableInteractions.Value)
			{
				// We need to disable future interactions with this.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						edge.AutoDisabledInteractionsKey,
						true,
						result => OnClickAutoDisableInteractions(edge, done)
					)
				);
			}
			else
			{
				// Bypass setting future interactions.
				OnClickAutoDisableInteractions(edge, done);
			}
		}

		void OnClickAutoDisableInteractions(ButtonEdgeModel edge, Action done)
		{
			if (edge.AutoDisableEnabled.Value)
			{
				// We need to disable this button for future interactions.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						edge.AutoDisabledKey,
						true,
						result => OnClickAutoDisableEnabled(edge, done)
					)
				);
			}
			else
			{
				// Bypass disabling this button.
				OnClickAutoDisableEnabled(edge, done);
			}
		}

		void OnClickAutoDisableEnabled(ButtonEdgeModel edge, Action done)
		{
			done();
		}
		#endregion
	}
}