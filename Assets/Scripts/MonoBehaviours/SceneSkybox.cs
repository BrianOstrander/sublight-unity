using UnityEngine;

namespace LunraGames.SubLight
{
	public class SceneSkybox : MonoBehaviour
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Material skybox;
		[SerializeField]
		Color ambientLight = Color.white;
		[SerializeField]
		FloatRange skyboxRotationSpeedRange;
		[SerializeField]
		FloatRange skyboxExposureRange;
		[SerializeField]
		AnimationCurve skyboxExposureByTimeScalarCurve;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		Material skyboxInstance;
		float skyboxRotationSpeed;
		float skyboxRotation;

		float timeScalar;
		public float TimeScalar
		{
			set
			{
				timeScalar = value;
				ApplyTimeScalar();
			}
			private get { return timeScalar; }
		}

		void Awake()
		{
			skyboxInstance = new Material(skybox);

			// Don't remember why I commented this out... shouldn't be a problem though...
			//ApplyRenderSettings();
		}

		void Update()
		{
			skyboxRotation = (skyboxRotation + (skyboxRotationSpeed * Time.deltaTime)) % 360f;
			skyboxInstance.SetFloat(ShaderConstants.SkyboxPanoramic.Rotation, skyboxRotation);
		}

		public void ApplyRenderSettings()
		{
			RenderSettings.skybox = skyboxInstance;
			RenderSettings.ambientLight = ambientLight;

			ApplyTimeScalar();
		}

		void ApplyTimeScalar()
		{
			skyboxRotationSpeed = skyboxRotationSpeedRange.Evaluate(TimeScalar);
			skyboxInstance.SetFloat(ShaderConstants.SkyboxPanoramic.Exposure, skyboxExposureRange.Evaluate(skyboxExposureByTimeScalarCurve.Evaluate(TimeScalar)));
		}
	}
}