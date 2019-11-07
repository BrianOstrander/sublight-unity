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
			List<ButtonEntryModel> remaining,
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
			ButtonEntryModel entry,
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

			if (entry.AutoDisableInteractions.Value)
			{
				// When this button is pressed, interactions get disabled, so we have to check.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						entry.AutoDisabledInteractionsKey,
						interactionKvResult => OnAutoDisabledInteractions(interactionKvResult, entry, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disable interactions check.
				OnAutoDisabledInteractions(
					null,
					entry,
					possibleResult,
					nextDone
				);
			}
		}

		void OnAutoDisabledInteractions(
			KeyValueResult<bool>? kvResult,
			ButtonEntryModel entry,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Interactable = !kvResult.Value.Value;
			}

			if (!entry.NotAutoUsed.Value)
			{
				// When this button is pressed, it gets marked as used, so we have to check to see if that happened.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						entry.AutoUsedKey,
						autoUsedKvResult => OnAutoUsed(autoUsedKvResult, entry, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto used check.
				OnAutoUsed(
					null,
					entry,
					possibleResult,
					nextDone
				);
			}
		}

		void OnAutoUsed(
			KeyValueResult<bool>? kvResult,
			ButtonEntryModel entry,
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
				filterResult => OnEnabledFiltering(filterResult, entry, possibleResult, nextDone),
				entry.EnabledFiltering,
				Configuration.Model,
				Configuration.Encounter
			);
		}

		void OnEnabledFiltering(
			bool filteringResult,
			ButtonEntryModel entry,
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
					filterResult => OnInteractableFiltering(filterResult, entry, possibleResult, nextDone),
					entry.InteractableFiltering,
					Configuration.Model,
					Configuration.Encounter
				);
			}
			else
			{
				// Already not interactable, so bypass the filter.
				OnInteractableFiltering(
					false,
					entry,
					possibleResult,
					nextDone
				);
			}
		}

		void OnInteractableFiltering(
			bool filteringResult,
			ButtonEntryModel entry,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Interactable &= filteringResult;

			if (!possibleResult.Used)
			{
				// Hasn't been auto marked as used, so we have to check the filter.
				Configuration.ValueFilter.Filter(
					filterResult => OnUsedFiltering(filterResult, entry, possibleResult, nextDone),
					entry.UsedFiltering,
					Configuration.Model,
					Configuration.Encounter
				);
			}
			else
			{
				// Auto using made it used, so we bypass the filter.
				OnUsedFiltering(
					true,
					entry,
					possibleResult,
					nextDone
				);
			}
		}

		void OnUsedFiltering(
			bool filteringResult,
			ButtonEntryModel entry,
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
		void OnClick(ButtonEntryModel entry, Action done)
		{
			if (!entry.NotAutoUsed.Value)
			{
				// We need to set this to be used.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						entry.AutoUsedKey,
						true,
						result => OnClickAutoUse(entry, done)
					)
				);
			}
			else
			{
				// Bypass setting it to be used.
				OnClickAutoUse(entry, done);
			}
		}

		void OnClickAutoUse(ButtonEntryModel entry, Action done)
		{
			if (entry.AutoDisableInteractions.Value)
			{
				// We need to disable future interactions with this.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						entry.AutoDisabledInteractionsKey,
						true,
						result => OnClickAutoDisableInteractions(entry, done)
					)
				);
			}
			else
			{
				// Bypass setting future interactions.
				OnClickAutoDisableInteractions(entry, done);
			}
		}

		void OnClickAutoDisableInteractions(ButtonEntryModel entry, Action done)
		{
			if (entry.AutoDisableEnabled.Value)
			{
				// We need to disable this button for future interactions.
				Configuration.Callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						entry.AutoDisabledKey,
						true,
						result => OnClickAutoDisableEnabled(entry, done)
					)
				);
			}
			else
			{
				// Bypass disabling this button.
				OnClickAutoDisableEnabled(entry, done);
			}
		}

		void OnClickAutoDisableEnabled(ButtonEntryModel entry, Action done)
		{
			done();
		}
		#endregion
	}
}