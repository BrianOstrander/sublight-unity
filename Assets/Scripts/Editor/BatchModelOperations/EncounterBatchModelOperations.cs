using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

using Shared = LunraGames.SubLight.SharedBatchModelOperations;

namespace LunraGames.SubLight
{
	public static class EncounterBatchModelOperations
	{
		#region Append Periods
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void AppendPeriods(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done,
			bool write
		)
		{
			var errors = string.Empty;

			var entries = new List<KeyValuePair<Func<string>, Action<string>>>();

			var listeners = model.Logs.GetLogs<ButtonEncounterLogModel>()
								 .SelectMany(l => l.Buttons.Value)
								 .Select(b => b.Message);

			var conversationEntries = model.Logs.GetLogs<ConversationEncounterLogModel>()
												.SelectMany(l => l.Entries.Value);

			foreach (var entry in conversationEntries)
			{
				switch (entry.ConversationType.Value)
				{
					case ConversationTypes.MessageIncoming:
					case ConversationTypes.MessageOutgoing:
						listeners = listeners.Append(entry.Message);
						break;
					case ConversationTypes.Prompt:
						switch (entry.PromptInfo.Value.Behaviour)
						{
							case ConversationButtonPromptBehaviours.ButtonOnly:
							case ConversationButtonPromptBehaviours.PrintMessage:
								listeners = listeners.Append(entry.Message);
								break;
							case ConversationButtonPromptBehaviours.PrintOverride:
								listeners = listeners.Append(entry.Message);
								entries.Add(
									new KeyValuePair<Func<string>, Action<string>>(
										() => entry.PromptInfo.Value.MessageOverride,
										value =>
										{
											var prompt = entry.PromptInfo.Value;
											prompt.MessageOverride = value;
											entry.PromptInfo.Value = prompt;
										}
									)
								);
								break;
						}
						break;
					default:
						errors += Shared.ModificationPrefix + "Unrecognized ConversationEntryType: " + entry.ConversationType.Value;
						break;
				}
			}

			foreach (var listener in listeners)
			{
				entries.Add(
					new KeyValuePair<Func<string>, Action<string>>(
						() => listener.Value,
						value => listener.Value = value
					)
				);
			}

			if (entries.None())
			{
				done(model, RequestResult.Success(Shared.GetUnmodifiedResult(model)));
				return;
			}

			var modifications = string.Empty;
			var modificationCount = 0;

			foreach (var entry in entries)
			{
				modifications += Shared.ModificationPrefix;

				var beginValue = entry.Key();
				var endValue = beginValue;

				var modified = AppendPeriodsUpdate(beginValue, ref endValue);

				var beginValueShort = Shared.ShortenValue(beginValue);
				var endValueShort = Shared.ShortenValue(endValue);

				if (modified)
				{
					modificationCount++;
					modifications += "+ \"" + beginValueShort + "\" modified to \"" + endValueShort + "\"";
					if (write) entry.Value(endValue);
				}
				else modifications += "- \"" + beginValueShort + "\" unmodified";
			}

			var result = Shared.GetModifiedResult(
				model,
				modificationCount,
				entries.Count,
				modifications,
				errors
			);

			done(model, RequestResult.Success(result));
		}

		static bool AppendPeriodsUpdate(
			string beginValue,
			ref string endValue
		)
		{
			if (string.IsNullOrEmpty(beginValue)) return false;
			var last = beginValue.Last();
			if (last == '.') return false;
			if (!char.IsLetter(last)) return false;
			if (char.IsUpper(last)) return false;

			endValue += '.';

			return true;
		}
		#endregion

		#region Consolidate Conversation Themes
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void ConsolidateConversationThemes(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done,
			bool write
		)
		{
			var errors = string.Empty;

			var bustEntries = model.Logs.GetLogs<BustEncounterLogModel>()
								   .SelectMany(l => l.Entries.Value)
								   .Where(e => e.BustEvent.Value == BustEntryModel.Events.Initialize);

			var modifications = string.Empty;
			var modificationCount = 0;

			foreach (var bust in bustEntries)
			{
				modifications += "\n\t";

				var initializeInfo = bust.InitializeInfo.Value;

				switch (initializeInfo.TransmitionStrengthIcon)
				{
					case BustEntryModel.TransmissionStrengths.Hidden:
						initializeInfo.Theme = ConversationThemes.Internal;
						break;
					default:
						initializeInfo.Theme = ConversationThemes.AwayTeam;
						break;
				}

				if (bust.InitializeInfo.Value.Theme != initializeInfo.Theme)
				{
					if (write) bust.InitializeInfo.Value = initializeInfo;
					modifications += "+ " + bust.BustId.Value + " modified to Theme." + initializeInfo.Theme;
					modificationCount++;
				}
				else modifications += "- " + bust.BustId.Value + " unmodified...";
			}

			var result = Shared.GetModifiedResult(
				model,
				modificationCount,
				bustEntries.Count(),
				modifications,
				errors
			);

			done(model, RequestResult.Success(result));
		}
		#endregion
	}
}