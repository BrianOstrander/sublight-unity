namespace LunraGames.SubLight.Models
{
	public class UniverseScaleModel : Model
	{
		public static UniverseScaleModel Create(UniverseScales scale)
		{
			var result = new UniverseScaleModel();
			result.Scale.Value = scale;
			result.TransformDefault.Value = UniverseTransform.Default(scale);
			return result;
		}

		UniverseScales scale;
		public ListenerProperty<UniverseScales> Scale;

		float opacity;
		public ListenerProperty<float> Opacity;

		UniverseTransform transformDefault;
		public ListenerProperty<UniverseTransform> TransformDefault;

		UniverseTransform transform;
		public ListenerProperty<UniverseTransform> Transform;


		public UniverseScaleModel()
		{
			Scale = new ListenerProperty<UniverseScales>(value => scale = value, () => scale);
			Opacity = new ListenerProperty<float>(value => opacity = value, () => opacity);
			TransformDefault = new ListenerProperty<UniverseTransform>(value => transformDefault = value, () => transformDefault);
			Transform = new ListenerProperty<UniverseTransform>(value => transform = value, () => transform);
		}

		public bool IsVisible
		{
			get
			{
				return 0f < Opacity.Value;
			}
		}
	}
}