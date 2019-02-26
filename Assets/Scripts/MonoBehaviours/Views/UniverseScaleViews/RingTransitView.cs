using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class RingTransitView : UniverseScaleView, IRingTransitView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Material ringMaterial;
		[SerializeField]
		int outerRingRenderQueue;
		[SerializeField]
		int innerRingRenderQueue;
		[SerializeField]
		MeshRenderer outerRingMesh;
		[SerializeField]
		MeshRenderer innerRingMesh;
		[SerializeField]
		MeshRenderer[] ringMeshes;

		[Header("Animation")]
		[SerializeField]
		AnimationCurve heightCurve;
		[SerializeField]
		float heightMaximum;
		[SerializeField]
		AnimationCurve opacityCurve;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float opacityFromAnimation;

		public float AnimationProgress
		{
			set
			{
				var height = heightCurve.Evaluate(value) * heightMaximum;
				opacityFromAnimation = opacityCurve.Evaluate(value);

				foreach (var mesh in ringMeshes) mesh.material.SetFloat(ShaderConstants.HoloWidgetGridRing.VMaximum, height);

				SetOpacityStale();
			}
		}

		public float GridHazardOffset
		{
			set
			{
				foreach (var mesh in ringMeshes) mesh.material.SetFloat(ShaderConstants.HoloWidgetGridRing.UOffset, value);
			}
		}

		protected override void OnSetGrid(Vector3 gridOrigin, float gridRadius)
		{
			foreach (var mesh in ringMeshes)
			{
				mesh.material.SetVector(ShaderConstants.HoloWidgetGridRing.WorldOrigin, gridOrigin);
				mesh.material.SetFloat(ShaderConstants.HoloWidgetGridRing.WorldRadius, gridRadius);
			}
		}

		public override void Reset()
		{
			base.Reset();

			var outerMaterial = new Material(ringMaterial);
			var innerMaterial = new Material(ringMaterial);

			outerMaterial.renderQueue = outerRingRenderQueue;
			innerMaterial.renderQueue = innerRingRenderQueue;

			outerRingMesh.material = outerMaterial;
			innerRingMesh.material = innerMaterial;

			AnimationProgress = 0f;

			PushOpacity(() => opacityFromAnimation);
		}

		protected override void OnOpacityStack(float opacity)
		{
			foreach (var mesh in ringMeshes) mesh.material.SetFloat(ShaderConstants.HoloWidgetGridRing.Alpha, opacity);
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);


		}
	}

	public interface IRingTransitView : IUniverseScaleView
	{
		/// <summary>
		/// Sets the animation progress from 0 to 3.
		/// </summary>
		/// <value>The animation progress.</value>
		float AnimationProgress { set; }

		float GridHazardOffset { set; }
	}
}