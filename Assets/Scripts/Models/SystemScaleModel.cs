using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SystemScaleModel : Model
	{
		[JsonProperty] SystemScales systemScale;
		[JsonProperty] float opacity;
		[JsonProperty] Vector3 unitToUnityScalar;

		[JsonIgnore]
		public ListenerProperty<SystemScales> SystemScale;
		[JsonIgnore]
		public ListenerProperty<float> Opacity;
		[JsonIgnore]
		public ListenerProperty<Vector3> UnitToUnityScalar;

		public SystemScaleModel()
		{
			SystemScale = new ListenerProperty<SystemScales>(value => systemScale = value, () => systemScale);
			Opacity = new ListenerProperty<float>(value => opacity = value, () => opacity);
			UnitToUnityScalar = new ListenerProperty<Vector3>(value => unitToUnityScalar = value, () => unitToUnityScalar);
		}
	}
}