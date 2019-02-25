using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustEntryModel : EdgeEntryModel
	{
		public enum Events
		{
			Unknown = 0,
			Initialize = 10,
			Focus = 20
		}

		public enum AvatarTypes
		{
			Unknown = 0,
			Static = 10
		}

		public enum TransmissionStrengths
		{
			Unknown = 0,
			Hidden = 10,
			Failed = 20,
			Weak = 30,
			Intermittent = 40,
			Strong = 50
		}

		[Serializable]
		public struct InitializeBlock
		{
			public static InitializeBlock Default
			{
				get
				{
					return new InitializeBlock
					{
						TitleClassification = "Interstellar Ark",

						TransmitionType = "Transmission",
						TransmitionStrength = "Strong",
						TransmitionStrengthIcon = TransmissionStrengths.Strong,
						AvatarType = AvatarTypes.Static
					};
				}
			}

			public string TitleSource;
			public string TitleClassification;

			public string TransmitionType;
			public string TransmitionStrength;
			public TransmissionStrengths TransmitionStrengthIcon;

			public string PlacardName;
			public string PlacardDescription;

			public AvatarTypes AvatarType;

			public int AvatarStaticIndex;
		}

		[Serializable]
		public struct FocusBlock
		{
			public static FocusBlock Default { get { return new FocusBlock(); } }

			public bool Instant;
		}

		[JsonProperty] string bustId;
		[JsonProperty] Events bustEvent;

		[JsonProperty] InitializeBlock initializeInfo;
		[JsonProperty] FocusBlock focusInfo;

		[JsonIgnore]
		public readonly ListenerProperty<string> BustId;
		[JsonIgnore]
		public readonly ListenerProperty<Events> BustEvent;

		[JsonIgnore]
		public readonly ListenerProperty<InitializeBlock> InitializeInfo;
		[JsonIgnore]
		public readonly ListenerProperty<FocusBlock> FocusInfo;

		public BustEntryModel()
		{
			BustId = new ListenerProperty<string>(value => bustId = value, () => bustId);
			BustEvent = new ListenerProperty<Events>(value => bustEvent = value, () => bustEvent);

			InitializeInfo = new ListenerProperty<InitializeBlock>(value => initializeInfo = value, () => initializeInfo);
			FocusInfo = new ListenerProperty<FocusBlock>(value => focusInfo = value, () => focusInfo);
		}
	}
}