using System;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
    public enum GamemodePortalTransitions
    {
        Unknown = 0,
        None = 10,
        Previous = 20,
        Next = 30
    }

    public class GamemodePortalView : View, IGamemodePortalView
	{
		[Serializable]
		struct ParallaxEntry
		{
            public enum Layers
            {
                Unknown = 0,
                Default = 10,
                Icon = 20
            }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
            public Layers Layer;
            public Material OverrideMaterial;
			public Texture2D OverrideTexture;
			public float Alpha;
			public MeshRenderer Renderer;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
        }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
        [SerializeField] CanvasGroup rootGroup;
        [SerializeField] MeshRenderer rimRenderer;
        [SerializeField] Color rimColor;
        [SerializeField] MeshRenderer chromaticRenderer;
        [SerializeField] Color chromaticColor;
        [SerializeField] ParticleSystem scanningParticles;
        [SerializeField] Color scanningParticlesStartColor;
        [SerializeField] Transform parallaxAnchor;
		[SerializeField] ParallaxEntry[] parallaxEntries;
		[SerializeField] float parallaxScalar;
		[SerializeField] AnimationCurve parallaxOffsetCurve;
		[SerializeField] AnimationCurve chromaticOffsetScaleCurve;

		[SerializeField] Transform startButtonRoot;
		[SerializeField] Transform startButtonAnchor;
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
				foreach (var entry in parallaxEntries)
				{
					entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.ChromaticOffsetScale, chromaticAberration);
					entry.Renderer.material.SetVector(ShaderConstants.HoloGamemodePortalParallax.ParallaxCoordinates, (delta * currMultiplier).NewZ(currZoom));
					currMultiplier *= 0.6f;

                    entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, OpacityStack * entry.Alpha);
					//currZoom *= currZoomScalar;
				}
			}
		}

        public Action StartClick { set; private get; }
        public Action NextClick { set; private get; }
        public Action PreviousClick { set; private get; }

        protected override void OnOpacityStack(float opacity)
        {
            rootGroup.alpha = opacity;

            var particleSystemMain = scanningParticles.main;
            particleSystemMain.startColor = scanningParticlesStartColor.NewA(scanningParticlesStartColor.a * opacity);

            foreach (var entry in parallaxEntries)
            {
                entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, opacity * entry.Alpha);
            }

            rimRenderer.material.SetColor(ShaderConstants.HoloGamemodePortalRim.PrimaryColor, rimColor.NewA(rimColor.a * opacity));
            chromaticRenderer.material.color = chromaticColor.NewV(chromaticColor.GetV() * opacity);
        }

        public override void Reset()
		{
			base.Reset();

			HoloColor = Color.white;

			foreach (var entry in parallaxEntries)
			{
				if (entry.OverrideMaterial == null) entry.Renderer.material = new Material(entry.Renderer.material);
				else entry.Renderer.material = new Material(entry.OverrideMaterial);
				if (entry.OverrideTexture != null) entry.Renderer.material.SetTexture(ShaderConstants.HoloGamemodePortalParallax.PrimaryTexture, entry.OverrideTexture);
				entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, entry.Alpha);

                switch (entry.Layer)
                {
                    case ParallaxEntry.Layers.Default:
                    case ParallaxEntry.Layers.Icon:
                        break;
                    default:
                        Debug.LogError("Unrecognized Parallax Layer: " + entry.Layer);
                        break;
                }
            }

			startButtonRoot.SetParent(startButtonAnchor, false);
			startButtonRoot.localPosition = Vector3.zero;

			rimRenderer.material = new Material(rimRenderer.material);
            chromaticRenderer.material = new Material(chromaticRenderer.material);

            StartClick = ActionExtensions.Empty;
            NextClick = ActionExtensions.Empty;
            PreviousClick = ActionExtensions.Empty;
		}

        #region Events
        public void OnStartClick()
        {
            StartClick();
        }

        public void OnNextClick()
        {
            NextClick();
        }

        public void OnPreviousClick()
        {
            PreviousClick();
        }
        #endregion
    }

    public interface IGamemodePortalView : IView, IHoloColorView
	{
		Vector3 PointerViewport { set; }
        Action StartClick { set; }
        Action NextClick { set; }
        Action PreviousClick { set; }
    }
}