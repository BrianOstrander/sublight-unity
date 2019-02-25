using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct EncounterResume
	{
		public static EncounterResume Default { get { return new EncounterResume(null, EncounterTriggers.Unknown); } }

		[JsonProperty] public readonly string EncounterId;
		[JsonProperty] public readonly EncounterTriggers Trigger;

		[JsonIgnore] public bool CanResume { get { return !string.IsNullOrEmpty(EncounterId); } }

		public EncounterResume(
			string encounterId,
			EncounterTriggers trigger
		)
		{
			EncounterId = encounterId;
			Trigger = trigger;
		}
	}
}