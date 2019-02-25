using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class ShipPinView : UniverseScaleView, IShipPinView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float unityScale;
		[SerializeField]
		FloatRange verticalBobbingYRange;
		[SerializeField]
		AnimationCurve verticalBobbingYCurve;
		[SerializeField]
		FloatRange verticalBobbingSpeedRange;
		[SerializeField]
		AnimationCurve verticalBobbingSpeedCurve;
		[SerializeField]
		Transform verticalBobbingAnchor;
		[SerializeField]
		CurveStyleBlock radiusOpacity = CurveStyleBlock.Default;
		[SerializeField]
		MeshRenderer[] opacityMeshes;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float verticalBobbingSpeed;
		float verticalBobbing;

		float timeScalar;
		public float TimeScalar
		{
			private get { return timeScalar; }
			set
			{
				timeScalar = value;
				verticalBobbingSpeed = verticalBobbingSpeedRange.Evaluate(verticalBobbingSpeedCurve.Evaluate(value));
			}
		}

		public float UnityScale { get { return unityScale; } }
		public float AdditionalYOffset { set; private get; }

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{
			SetOpacityStale();
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			verticalBobbing = (verticalBobbing + (verticalBobbingSpeed * delta)) % 1f;
			var currY = (verticalBobbingYRange.Evaluate(verticalBobbingYCurve.Evaluate(verticalBobbing))) * (1f - TimeScalar);
			verticalBobbingAnchor.localPosition = new Vector3(0f, currY + AdditionalYOffset, 0f);

		}

		public override void Reset()
		{
			base.Reset();

			TimeScalar = 0f;
			AdditionalYOffset = 0f;

			PushOpacity(() => radiusOpacity.Evaluate(RadiusNormal));
		}

		protected override void OnOpacityStack(float opacity)
		{
			foreach (var mesh in opacityMeshes) mesh.material.SetFloat(ShaderConstants.HoloPinOutside.Alpha, opacity);
		}
	}

	public interface IShipPinView : IUniverseScaleView
	{
		float UnityScale { get; }
		float TimeScalar { set; }
		float AdditionalYOffset { set; }
	}
}