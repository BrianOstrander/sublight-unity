using System;
using System.Linq;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
    public struct GamemodeBlock
    {
        public enum LockStates
        {
            Unknown = 0,
            Unlocked = 10,
            Locked = 20,
            InDevelopment = 30
        }

        public string Title;
        public string SubTitle;
        public string Description;
        public string StartText;
        public Texture2D Icon;
        public LockStates LockState;
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

        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI subTitleLabel;
        [SerializeField] TextMeshProUGUI descriptionLabel;
        [SerializeField] TextMeshProUGUI startLabel;

        [SerializeField] float transitionDuration;
        [SerializeField] AnimationCurve transitionGroupsOpacityCurve;
        [SerializeField] CanvasGroup[] transitionGroups;
        [SerializeField] AnimationCurve transitionIconOpacityCurve;
        [SerializeField] AnimationCurve transitionIconOffsetCurve;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

        GamemodeBlock currentGamemode;
        GamemodeBlock targetGamemode;
        bool transitionToRight;
        float? transitionRemaining;
        bool transitionHasSetData;

        float lastTransitionIconOpacity;
		float lastTransitionIconOffset;

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
					var currCoordinates = (delta * currMultiplier).NewZ(currZoom);
					
					currMultiplier *= 0.6f;

					switch (entry.Layer)
					{
						case ParallaxEntry.Layers.Default:
							entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, OpacityStack * entry.Alpha);
							break;
						case ParallaxEntry.Layers.Icon:
							entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, OpacityStack * entry.Alpha * lastTransitionIconOpacity);
							currCoordinates = currCoordinates.NewX(currCoordinates.x + lastTransitionIconOffset);
							break;
					}

					entry.Renderer.material.SetVector(ShaderConstants.HoloGamemodePortalParallax.ParallaxCoordinates, currCoordinates);
				}
			}
		}

		public Action StartClick { set; private get; }
		public Action NextClick { set; private get; }
		public Action PreviousClick { set; private get; }

        public void SetGamemode(GamemodeBlock gamemode, bool instant = false, bool toRight = false)
        {
            transitionHasSetData = false;

			if (instant)
			{
				currentGamemode = gamemode;
				targetGamemode = gamemode;
				transitionToRight = false;
				transitionRemaining = null;
				OnSetGamemodeData(gamemode);
				OnSetGamemodeAnimation(1f);
				return;
			}

            currentGamemode = targetGamemode;
            targetGamemode = gamemode;
            transitionToRight = toRight;
            transitionRemaining = transitionDuration;
        }

        protected override void OnOpacityStack(float opacity)
        {
            rootGroup.alpha = opacity;

            var particleSystemMain = scanningParticles.main;
            particleSystemMain.startColor = scanningParticlesStartColor.NewA(scanningParticlesStartColor.a * opacity);

            foreach (var entry in parallaxEntries)
            {
                switch (entry.Layer)
                {
                    case ParallaxEntry.Layers.Default:
                        entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, opacity * entry.Alpha);
                        break;
                    case ParallaxEntry.Layers.Icon:
                        entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, opacity * entry.Alpha * lastTransitionIconOpacity);
                        break;
                }
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

			currentGamemode = new GamemodeBlock();
			targetGamemode = new GamemodeBlock();
			transitionToRight = false;
			transitionRemaining = null;

			titleLabel.text = string.Empty;
			subTitleLabel.text = string.Empty;
			descriptionLabel.text = string.Empty;
			startLabel.text = string.Empty;

			lastTransitionIconOpacity = 0f;
			lastTransitionIconOffset = 0f;
        }

        protected override void OnIdle(float delta)
        {
            base.OnIdle(delta);

            if (!transitionRemaining.HasValue) return;

            transitionRemaining = Mathf.Max(0f, transitionRemaining.Value - delta);
            var transitionScalar = 1f - (transitionRemaining.Value / transitionDuration);

            if (!transitionHasSetData && 0.5f < transitionScalar)
            {
                OnSetGamemodeData(targetGamemode);
            }

			OnSetGamemodeAnimation(transitionScalar);

			if (Mathf.Approximately(0f, transitionRemaining.Value))
			{
				transitionRemaining = null;
			}
		}

        #region Events
        void OnSetGamemodeData(GamemodeBlock gamemode)
        {
            titleLabel.text = gamemode.Title + " /";
            subTitleLabel.text = gamemode.SubTitle;
            descriptionLabel.text = gamemode.Description;
            startLabel.text = gamemode.StartText;

			foreach (var entry in parallaxEntries.Where(e => e.Layer == ParallaxEntry.Layers.Icon))
			{
				entry.Renderer.material.SetTexture(ShaderConstants.HoloGamemodePortalParallax.PrimaryTexture, gamemode.Icon);
			}

			transitionHasSetData = true;
        }

		void OnSetGamemodeAnimation(float scalar)
		{
			var transitionGroupOpacity = transitionGroupsOpacityCurve.Evaluate(scalar);
			lastTransitionIconOpacity = transitionIconOpacityCurve.Evaluate(scalar);
			lastTransitionIconOffset = transitionIconOffsetCurve.Evaluate(scalar);
			if (!transitionToRight) lastTransitionIconOffset *= -1f;

			foreach (var entry in parallaxEntries.Where(e => e.Layer == ParallaxEntry.Layers.Icon))
			{
				entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, OpacityStack * entry.Alpha * lastTransitionIconOpacity);
			}

			foreach (var group in transitionGroups) group.alpha = transitionGroupOpacity;
		}
		#endregion

		#region Button Events
		public void OnStartClick()
        {
            if (transitionRemaining.HasValue) return;

            StartClick();
        }

        public void OnNextClick()
        {
            if (transitionRemaining.HasValue) return;

            NextClick();
        }

        public void OnPreviousClick()
        {
            if (transitionRemaining.HasValue) return;

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

        void SetGamemode(GamemodeBlock gamemode, bool instant = false, bool toRight = false);
    }
}