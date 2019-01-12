using UnityEngine;

using LunraGames;

namespace LunraGames.SubLight
{
	[ExecuteInEditMode]
	public class Wind : MonoBehaviour
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float timeScalar;
		[SerializeField]
		AnimationCurve intensityX = AnimationCurveExtensions.Constant();
		[SerializeField]
		AnimationCurve intensityY = AnimationCurveExtensions.Constant();
		[SerializeField]
		AnimationCurve intensityZ = AnimationCurveExtensions.Constant();
		[SerializeField]
		Vector3 intensityScalar;
		[SerializeField]
		float multiplier;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float scalar;

		void Update()
		{
			scalar = (scalar + (Time.deltaTime * timeScalar)) % 1f;
			var finalScale = intensityScalar * multiplier;
			Shader.SetGlobalVector(
				ShaderConstants.Globals.WindIntensity,
				new Vector4(
					intensityX.Evaluate(scalar) * finalScale.x,
					intensityY.Evaluate(scalar) * finalScale.y,
					intensityZ.Evaluate(scalar) * finalScale.z
				)
			);
		}
	}
}