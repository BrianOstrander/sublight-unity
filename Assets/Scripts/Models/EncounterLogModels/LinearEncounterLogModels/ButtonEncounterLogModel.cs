using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<ButtonEdgeModel>
	{
		public enum Styles
		{
			Unknown = 0,
			Bust = 10
		}

		[Serializable]
		public struct BustStyleBlock
		{
			public static BustStyleBlock Default
			{
				get
				{
					return new BustStyleBlock
					{
						Theme = Themes.Foreigner
					};
				}
			}

			public enum Themes
			{
				Unknown = 0,
				Crew = 10,
				AwayTeam = 20,
				Foreigner = 30,
				Downlink = 40
			}

			public Themes Theme;
		}

		[JsonProperty] ButtonEdgeModel[] buttons = new ButtonEdgeModel[0];

		[JsonProperty] Styles style;

		[JsonProperty] BustStyleBlock bustStyle;

		[JsonIgnore]
		public readonly ListenerProperty<ButtonEdgeModel[]> Buttons;

		[JsonIgnore]
		public readonly ListenerProperty<Styles> Style;

		[JsonIgnore]
		public readonly ListenerProperty<BustStyleBlock> BustStyle;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Button; } }

		public override bool RequiresNextLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public ButtonEncounterLogModel()
		{
			Buttons = new ListenerProperty<ButtonEdgeModel[]>(value => buttons = value, () => buttons);

			Style = new ListenerProperty<Styles>(value => style = value, () => style);

			BustStyle = new ListenerProperty<BustStyleBlock>(value => bustStyle = value, () => bustStyle);
		}

		[JsonIgnore]
		public ButtonEdgeModel[] Edges
		{
			get { return Buttons.Value; }
			set { Buttons.Value = value; }
		}
	}
}