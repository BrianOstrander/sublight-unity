using UnityEngine;

namespace LunraGames.SubLight
{
	[ExecuteInEditMode]
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

		void Start()
		{
			ApplyRenderSettings();
		}

		void Update()
		{
			var isPausedEditor = Application.isEditor && !Application.isPlaying;
			if (isPausedEditor && DevPrefs.AutoApplySkybox) ApplyRenderSettings();

			if (isPausedEditor) return;

			skyboxRotation = (skyboxRotation + (skyboxRotationSpeed * Time.deltaTime)) % 360f;
			RenderSettings.skybox.SetFloat(ShaderConstants.SkyboxPanoramic.Rotation, skyboxRotation);
		}

		public void ApplyRenderSettings()
		{
			RenderSettings.skybox = new Material(skybox);
			RenderSettings.ambientLight = ambientLight;

			ApplyTimeScalar();
		}

		void ApplyTimeScalar()
		{
			skyboxRotationSpeed = skyboxRotationSpeedRange.Evaluate(TimeScalar);
			RenderSettings.skybox.SetFloat(ShaderConstants.SkyboxPanoramic.Exposure, skyboxExposureRange.Evaluate(skyboxExposureByTimeScalarCurve.Evaluate(TimeScalar)));
		}
	}
}