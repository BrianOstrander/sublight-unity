using UnityEngine;

using LunraGames;

namespace LunraGames.SpaceFarm
{
	[ExecuteInEditMode]
	public class Wind : MonoBehaviour
	{
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