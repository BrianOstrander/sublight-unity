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
		[JsonProperty] UniverseTransform transformDefault;
		[JsonProperty] UniverseTransform transform;

		[JsonIgnore]
		public ListenerProperty<UniverseScales> Scale;
		[JsonIgnore]
		public ListenerProperty<float> Opacity;
		[JsonIgnore]
		public ListenerProperty<UniverseTransform> TransformDefault;
		[JsonIgnore]
		public ListenerProperty<UniverseTransform> Transform;

		public UniverseScaleModel()
		{
			Scale = new ListenerProperty<UniverseScales>(value => scale = value, () => scale);
			Opacity = new ListenerProperty<float>(value => opacity = value, () => opacity);
			TransformDefault = new ListenerProperty<UniverseTransform>(value => transformDefault = value, () => transformDefault);
			Transform = new ListenerProperty<UniverseTransform>(value => transform = value, () => transform, OnTransform);
		}

		[JsonIgnore]
		public bool IsVisible
		{
			get
			{
				return 0f < Opacity.Value;
			}
		}

		#region Events
		void OnTransform(UniverseTransform transform)
		{
			TransformDefault.Value = TransformDefault.Value.Duplicate(transform.UniverseOrigin);
		}
		#endregion
	}
}