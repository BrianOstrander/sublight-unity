using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GamemodePortalView : View, IGamemodePortalView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField] Texture2D[] parallaxTextures;
		[SerializeField] MeshRenderer[] parallaxMeshes;
		[SerializeField] Transform parallaxAnchor;
		[SerializeField] float parallaxScalar;
		[SerializeField] AnimationCurve parallaxOffsetCurve;
		[SerializeField] AnimationCurve chromaticOffsetScaleCurve;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public Color HoloColor { set { } } // lipMesh.material.SetColor(ShaderConstants.HoloLipAdditive.LipColor, value); } }

		public Vector3 PointerViewport
		{
			set
			{
				var anchorViewport = App.V.Camera.WorldToViewportPoint(parallaxAnchor.position);
				var delta = (anchorViewport - value).NewZ(0f) * parallaxScalar;
				var deltaMagnitude = delta.magnitude;
				var chromaticAberration = chromaticOffsetScaleCurve.Evaluate(deltaMagnitude);

				delta = new Vector3(
					parallaxOffsetCurve.Evaluate(Mathf.Abs(delta.x)) * Mathf.Sign(delta.x),
					parallaxOffsetCurve.Evaluate(Mathf.Abs(delta.y)) * Mathf.Sign(delta.y),
					Mathf.Clamp01(delta.z)
				);

				var currMultiplier = 1f;
				var currZoom = 0.5f * ((Mathf.Clamp01(1f - deltaMagnitude) * 0.75f) + 0.25f);
				var currZoomScalar = deltaMagnitude;
				foreach (var mesh in parallaxMeshes)
				{
					mesh.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.ChromaticOffsetScale, chromaticAberration);
					mesh.material.SetVector(ShaderConstants.HoloGamemodePortalParallax.ParallaxCoordinates, (delta * currMultiplier).NewZ(currZoom));
					currMultiplier *= 0.6f;
					//currZoom *= currZoomScalar;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			HoloColor = Color.white;

			for (var i = 0; i < parallaxMeshes.Length; i++)
			{
				var mesh = parallaxMeshes[i];
				mesh.material = new Material(mesh.material);
				mesh.material.SetTexture(ShaderConstants.HoloGamemodePortalParallax.PrimaryTexture, parallaxTextures[i]);
			}
		}
	}

	public interface IGamemodePortalView : IView, IHoloColorView
	{
		Vector3 PointerViewport { set; }
	}
}