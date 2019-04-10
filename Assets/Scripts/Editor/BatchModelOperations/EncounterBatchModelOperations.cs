using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class EncounterBatchModelOperations
	{
		#region Update Versions
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void UpdateVersions(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done,
			bool write
		)
		{
			var result = GetUnmodifiedResult(model);

			if (model.Version.Value != BuildPreferences.Instance.Info.Version)
			{
				result = GetModifiedResult(
					model,
					1,
					1,
					"\n\tUpdated version of " + model.Name.Value + " from " + model.Version.Value + " to " + BuildPreferences.Instance.Info.Version
				);
			}

			done(model, RequestResult.Success(result));
		}
		#endregion

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
								 .Select(b => b.Entry.Message);

			var conversationEntries = model.Logs.GetLogs<ConversationEncounterLogModel>()
							   .SelectMany(l => l.Entries.Value)
							   .Select(e => e.Entry);

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
						errors += "\n\tUnrecognized ConversationEntryType: " + entry.ConversationType.Value;
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
				done(model, RequestResult.Success(GetUnmodifiedResult(model)));
				return;
			}

			var modifications = string.Empty;
			var modificationCount = 0;

			foreach (var entry in entries)
			{
				modifications += "\n\t";

				var beginValue = entry.Key();
				var endValue = beginValue;

				var modified = AppendPeriodsUpdate(beginValue, ref endValue);

				var beginValueShort = ShortenValue(beginValue);
				var endValueShort = ShortenValue(endValue);

				if (modified)
				{
					modificationCount++;
					modifications += "+ \"" + beginValueShort + "\" modified to \"" + endValueShort + "\"";
					if (write) entry.Value(endValue);
				}
				else modifications += "- \"" + beginValueShort + "\" unmodified";
			}

			var result = GetModifiedResult(
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
								   .Select(e => e.Entry)
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
						initializeInfo.Theme = ConversationThemes.Crew;
						initializeInfo.Style = ConversationButtonStyles.Conversation;
						break;
					default:
						initializeInfo.Theme = ConversationThemes.AwayTeam;
						initializeInfo.Style = ConversationButtonStyles.Conversation;
						break;
				}

				if (bust.InitializeInfo.Value.Theme != initializeInfo.Theme || bust.InitializeInfo.Value.Style != initializeInfo.Style)
				{
					if (write) bust.InitializeInfo.Value = initializeInfo;
					modifications += "+ " + bust.BustId.Value + " modified to Style." + initializeInfo.Style + " & Theme." + initializeInfo.Theme;
					modificationCount++;
				}
				else modifications += "- " + bust.BustId.Value + " unmodified...";
			}

			var result = GetModifiedResult(
				model,
				modificationCount,
				bustEntries.Count(),
				modifications,
				errors
			);

			done(model, RequestResult.Success(result));
		}
		#endregion

		#region Shared
		static string GetUnmodifiedResult(SaveModel model)
		{
			return GetName(model) + " was unmodified...";
		}

		static string GetModifiedResult(
			SaveModel model,
			int modificationCount,
			int possibleModificationCount,
			string modifications,
			string errors = null
		)
		{
			if (modificationCount == 0 && possibleModificationCount == 0) return GetUnmodifiedResult(model);
			var result = GetName(model) + " had " + modificationCount + " of " + possibleModificationCount + " matches modified..." + modifications;
			if (string.IsNullOrEmpty(errors)) return result;
			return result += "\n\n\tErrors:" + errors;
		}

		static string GetName(SaveModel model)
		{
			//return model.SaveType + " \"" + (string.IsNullOrEmpty(model.Meta.Value) ? ShortenValue(model.Path.Value) : model.Meta.Value) + "\"";
			return "\"" + (string.IsNullOrEmpty(model.Meta.Value) ? ShortenValue(model.Path.Value) : model.Meta.Value) + "\"";
		}

		static string ShortenValue(
			string value,
			int maximumLength = 64
		)
		{
			maximumLength = Mathf.Max(maximumLength, 2);

			if (string.IsNullOrEmpty(value)) return value;
			if (value.Length < maximumLength) return value;

			var begin = value.Substring(0, maximumLength / 2);
			var end = value.Substring(value.Length - (maximumLength / 2));

			return begin + " . . . " + end;
		}
		#endregion
	}
}