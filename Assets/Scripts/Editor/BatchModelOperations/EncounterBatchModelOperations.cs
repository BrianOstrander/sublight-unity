using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class EncounterBatchModelOperations
	{
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void UpdateVersions(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done,
			bool write
		)
		{
			var message = string.Empty;

			if (model.Version.Value != BuildPreferences.Instance.Info.Version)
			{
				message = "Updated version of " + model.Name.Value + " from " + model.Version.Value + " to " + BuildPreferences.Instance.Info.Version;
			}

			done(model, RequestResult.Success(message));
		}

		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void AppendPeriods(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done,
			bool write
		)
		{
			var result = "Unmodified...";

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
						errors += "\tUnrecognized ConversationEntryType: " + entry.ConversationType.Value + "\n";
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
				done(model, RequestResult.Success(result));
				return;
			}

			var modifiedEntries = 0;
			result = string.Empty;

			foreach (var entry in entries)
			{
				result += "\n\t";

				var beginValue = entry.Key();
				var endValue = beginValue;

				var modified = AppendPeriodsUpdate(beginValue, ref endValue);

				var beginValueShort = AppendPeriodsShorten(beginValue);
				var endValueShort = AppendPeriodsShorten(endValue);

				if (modified)
				{
					modifiedEntries++;
					result += "+ \"" + beginValueShort + "\" modified to \"" + endValueShort + "\"";
					if (write) entry.Value(endValue);
				}
				else result += "- \"" + beginValueShort + "\" unmodified";
			}

			result = modifiedEntries + " of " + entries.Count + " modified:" + result;

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

		static string AppendPeriodsShorten(
			string value
		)
		{
			const int MaximumLength = 64;

			if (string.IsNullOrEmpty(value)) return value;
			if (value.Length < MaximumLength) return value;

			var begin = value.Substring(0, MaximumLength / 2);
			var end = value.Substring(value.Length - (MaximumLength / 2));

			return begin + " . . . " + end;
		}
	}
}