using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GalaxyLabelModel : Model
	{
		[JsonProperty] string labelId;
		[JsonProperty] string groupId;
		[JsonProperty] string name;
		[JsonProperty] string sourceKey;
		[JsonProperty] TextCurveBlock curveInfo = TextCurveBlock.Default;
		[JsonProperty] GalaxyLabelSources source = GalaxyLabelSources.Static;
		[JsonProperty] UniverseScales scale;
		[JsonProperty] UniversePosition beginAnchor;
		[JsonProperty] UniversePosition endAnchor;

		[JsonProperty] LanguageStringModel staticText = LanguageStringModel.Empty;
		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();

		[JsonIgnore]
		public readonly ListenerProperty<string> LabelId;
		[JsonIgnore]
		public readonly ListenerProperty<string> GroupId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> SourceKey;
		[JsonIgnore]
		public readonly ListenerProperty<TextCurveBlock> CurveInfo;
		[JsonIgnore]
		public readonly ListenerProperty<GalaxyLabelSources> Source;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScales> Scale;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> BeginAnchor;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> EndAnchor;

		[JsonIgnore]
		public LanguageStringModel StaticText { get { return staticText; } }
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public GalaxyLabelModel()
		{
			LabelId = new ListenerProperty<string>(value => labelId = value, () => labelId);
			GroupId = new ListenerProperty<string>(value => groupId = value, () => groupId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			SourceKey = new ListenerProperty<string>(value => sourceKey = value, () => sourceKey);
			CurveInfo = new ListenerProperty<TextCurveBlock>(value => curveInfo = value, () => curveInfo);
			Source = new ListenerProperty<GalaxyLabelSources>(value => source = value, () => source);
			Scale = new ListenerProperty<UniverseScales>(value => scale = value, () => scale);
			BeginAnchor = new ListenerProperty<UniversePosition>(value => beginAnchor = value, () => beginAnchor);
			EndAnchor = new ListenerProperty<UniversePosition>(value => endAnchor = value, () => endAnchor);
		}
	}
}