using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class UniverseScaleModel : Model
	{
		public static UniverseScaleModel Create(UniverseScales scale)
		{
			var result = new UniverseScaleModel();
			result.Scale.Value = scale;
			return result;
		}

		[JsonProperty] UniverseScales scale;
		[JsonProperty] float opacity;
		[JsonProperty] UniverseTransform transform;

		[JsonIgnore]
		public ListenerProperty<UniverseScales> Scale;
		[JsonIgnore]
		public ListenerProperty<float> Opacity;
		[JsonIgnore]
		public ListenerProperty<UniverseTransform> Transform;

		public UniverseScaleModel()
		{
			Scale = new ListenerProperty<UniverseScales>(value => scale = value, () => scale);
			Opacity = new ListenerProperty<float>(value => opacity = value, () => opacity);
			Transform = new ListenerProperty<UniverseTransform>(value => transform = value, () => transform);
		}
	}
}