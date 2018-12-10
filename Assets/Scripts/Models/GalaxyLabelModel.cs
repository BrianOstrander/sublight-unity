using UnityEngine;

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
		[JsonProperty] Vector3 beginAnchorNormal;
		[JsonProperty] Vector3 endAnchorNormal;
		[JsonProperty] int sliceLayer;

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
		public readonly ListenerProperty<Vector3> BeginAnchorNormal;
		[JsonIgnore]
		public readonly ListenerProperty<Vector3> EndAnchorNormal;
		[JsonIgnore]
		public readonly ListenerProperty<int> SliceLayer;

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
			BeginAnchorNormal = new ListenerProperty<Vector3>(value => beginAnchorNormal = value, () => beginAnchorNormal);
			EndAnchorNormal = new ListenerProperty<Vector3>(value => endAnchorNormal = value, () => endAnchorNormal);
			SliceLayer = new ListenerProperty<int>(value => sliceLayer = value, () => sliceLayer);
		}

		public float Proximity(Vector3 normal, int sampling = 1)
		{
			var lastProximity = float.MaxValue;

			sampling = Mathf.Max(sampling, 1);
			var delta = 1f / sampling;
			var offset = delta * 0.5f;
			for (var i = 0; i < sampling; i++)
			{
				var progress = offset + (i / sampling);
				var currProximity = Vector3.Distance(normal, CurveInfo.Value.Evaluate(BeginAnchorNormal.Value, EndAnchorNormal.Value, progress, false));
				if (currProximity < lastProximity) lastProximity = currProximity;
			}
			return lastProximity;
		}
	}
}