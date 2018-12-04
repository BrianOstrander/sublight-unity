using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public static class Celestial
	{
		public enum HighlightStates
		{
			Unknown = 0,
			Idle = 10,
			Highlighted = 20,
			HighlightedAnalysis = 30,
			OtherHighlighted = 40
		}
		
		public enum VisitStates
		{
			Unknown = 0,
			Visited = 10,
			NotVisited = 20,
			Current = 30
		}
		
		public enum RangeStates
		{
			Unknown = 0,
			InRange = 10,
			OutOfRange = 20
		}
		
		public enum SelectedStates
		{
			Unknown = 0,
			Selected = 10,
			NotSelected = 20,
			OtherSelected = 30
		}
		
		public enum TravelStates
		{
			Unknown = 0,
			Traveling = 10,
			NotTraveling = 20
		}
	}

	public class CelestialSystemView : UniverseScaleView, ICelestialSystemView
	{
		#region Definitions & Blocks
		static class Constants
		{
			public static class DropLineThickness
			{
				public const float None = 0f;
				public const float Normal = 0.5f;
				public const float Full = 1f;
			}

			public static class DropLineShiftMapProgress
			{
				public const float Primary = 0f;
				public const float Secondary = 1f;
			}

			public static class SelectedOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}

			public static class IconColorProgress
			{
				public const float NotVisited = 0f;
				public const float Visited = 0.5f;
				public const float Current = 1f;
			}

			public static class IconScaleProgress
			{
				public const float NotVisited = 0f;
				public const float Visited = 0.5f;
				public const float Current = 1f;
			}

			public static class DetailsOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}

			public static class ConfirmOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}

			public static class BaseDistanceOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}

			public static class BaseRingOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}

			public static class DirectionRingOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}

			public static class MaximizedProgress
			{
				public const float Minimized = 0f;
				public const float Maximized = 1f;
			}

			public static class BaseCenterOpacity
			{
				public const float None = 0f;
				public const float Full = 1f;
			}
		}

		[Serializable]
		struct VisualState
		{
			public float DropLineThickness;
			public float DropLineShiftMapProgress;
			public float DropLineBaseOpacity;
			public float BaseDistanceThickness;
			public float BaseDistanceOpacity;
			public float BaseRingOpacity;
			public float BaseCenterOpacity;
			public float DetailsOpacity;
			public float ConfirmOpacity;
			public float AnalysisOpacity;
			public float SelectedOpacity;
			public float MaximizedProgress;
		}

		[Serializable]
		struct ClassificationIcons
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public SystemClassifications Classification;
			public Texture2D Icon;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

		[Serializable]
		struct MaximizeOpacityGraphic
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public MeshRenderer Graphic;
			public string AlphaKey;
			public bool AppearsWhenMinimized;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

		[Serializable]
		struct MaximizeArea
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public Transform Area;
			public float MinimumScale;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}
		#endregion

		#region Serialized Properties
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[Header("Children")]
		[SerializeField]
		float transitionSpeed;
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		Transform verticalLookAtArea;
		[SerializeField]
		CanvasGroup interactableGroup;

		[SerializeField]
		float yMinimumOffset;

		[SerializeField]
		float dropLineThicknessMaximum;
		[SerializeField]
		AnimationCurve dropLineThickness;
		[SerializeField]
		AnimationCurve dropLineThicknessOpacity;
		[SerializeField]
		LineRenderer dropLine;
		[SerializeField]
		CurveStyleBlock dropLineRadiusOpacity = CurveStyleBlock.Default;
		[SerializeField]
		AnimationCurve dropLineShiftMapProgress;

		[SerializeField]
		MeshRenderer bottomCenterMesh;

		[SerializeField]
		MeshRenderer selectedGraphic;
		[SerializeField]
		MeshRenderer selectedInsideGraphic;
		[SerializeField]
		MeshRenderer colorGraphic;
		[SerializeField]
		MeshRenderer minimizeGraphic;

		[SerializeField]
		CanvasGroup detailsGroup;
		[SerializeField]
		CanvasGroup confirmGroup;
		[SerializeField]
		CanvasGroup baseGroup;
		[SerializeField]
		CanvasGroup baseDistanceGroup;
		[SerializeField]
		CanvasGroup baseRingGroup;

		[SerializeField]
		TextMeshProUGUI detailsNameLabel;
		[SerializeField]
		TextMeshProUGUI detailsDescriptionLabel;
		[SerializeField]
		TextMeshProUGUI confirmLabel;
		[SerializeField]
		TextMeshProUGUI confirmDescriptionLabel;
		[SerializeField]
		TextMeshProUGUI distanceLabel;
		[SerializeField]
		TextMeshProUGUI distanceUnitLabel;

		[SerializeField]
		AnimationCurve maximizeScale;
		[SerializeField]
		MaximizeArea[] maximizeAreas;
		[SerializeField]
		MaximizeOpacityGraphic[] maximizeOpacityGraphics;

		[SerializeField]
		RectTransform detailsContainer;

		[SerializeField]
		float iconSaturationMinimizedBottom;
		[SerializeField]
		float iconSaturationMinimizedTop;
		[SerializeField]
		float dropLineSaturation;
		[SerializeField]
		AnimationCurve iconScale;
		[SerializeField]
		Vector2 iconScaleRange;
		[SerializeField]
		ParticleSystem minimizedParticles;
		[SerializeField]
		float minimizedParticlesBaseAlpha;

		[SerializeField]
		ParticleSystem selectedParticles;

		[SerializeField]
		TrailRenderer dragTrail;

		[SerializeField]
		GameObject[] enableOnInBounds;

		[SerializeField]
		float onEnterDelayDuration;
		[SerializeField]
		float interactableOpacityCutoff;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		#endregion

		#region View Properties & Methods
		public string DetailsName { set { detailsNameLabel.text = value ?? string.Empty; isDetailsLayoutStale = true; } }
		public string DetailsDescription { set { detailsDescriptionLabel.text = value ?? string.Empty; isDetailsLayoutStale = true; } }
		public string Confirm { set { confirmLabel.text = value ?? string.Empty; } }
		public string ConfirmDescription { set { confirmDescriptionLabel.text = value ?? string.Empty; } }
		public string Distance { set { distanceLabel.text = value ?? string.Empty; } }
		public string DistanceUnit { set { distanceUnitLabel.text = value ?? string.Empty; } }

		public Color IconColor
		{
			set
			{
				var modifiedColorPrimary = value.NewS(value.GetS() * iconSaturationMinimizedBottom);
				var modifiedColorTertiary = value.NewS(value.GetS() * iconSaturationMinimizedTop);
				colorGraphic.material.SetColor(ShaderConstants.HoloCelestialSystemIconColor.PrimaryColor, modifiedColorPrimary);
				colorGraphic.material.SetColor(ShaderConstants.HoloCelestialSystemIconColor.TertiaryColor, modifiedColorTertiary);

				minimizeGraphic.material.SetColor(ShaderConstants.HoloCelestialSystemIconColor.PrimaryColor, modifiedColorPrimary);
				minimizeGraphic.material.SetColor(ShaderConstants.HoloCelestialSystemIconColor.TertiaryColor, modifiedColorTertiary);

				dropLine.material.SetColor(ShaderConstants.HoloDistanceFieldColorShiftConstant.PrimaryColor, modifiedColorPrimary.NewS(value.GetS() * dropLineSaturation));

				minimizedParticles.startColor = modifiedColorPrimary;

				dragTrail.material.SetColor(ShaderConstants.HoloTextureColorAlphaMasked.PrimaryColor, modifiedColorTertiary);

				selectedParticles.startColor = modifiedColorPrimary;

				selectedInsideGraphic.material.SetColor(ShaderConstants.HoloTextureColorAlpha.PrimaryColor, modifiedColorTertiary);

				detailsNameLabel.color = modifiedColorPrimary;
				detailsDescriptionLabel.color = modifiedColorTertiary;
			}
		}

		public float IconScale
		{
			set
			{
				var scale = Vector3.one * (iconScaleRange.x + (iconScale.Evaluate(value) * (iconScaleRange.y - iconScaleRange.x)));
				colorGraphic.transform.localScale = scale;
				minimizeGraphic.transform.localScale = scale;
			}
		}

		public Action Enter { set; private get; }
		public Action Exit { set; private get; }
		public Action Click { set; private get; }

		public bool SetStates(
			Celestial.HighlightStates highlightState = Celestial.HighlightStates.Unknown,
			Celestial.VisitStates visitState = Celestial.VisitStates.Unknown,
			Celestial.RangeStates rangeState = Celestial.RangeStates.Unknown,
			Celestial.SelectedStates selectedState = Celestial.SelectedStates.Unknown,
			Celestial.TravelStates travelState = Celestial.TravelStates.Unknown,
			bool instant = true
		)
		{
			var wasChanged = false;
			if (highlightState != Celestial.HighlightStates.Unknown && highlightState != HighlightState)
			{
				wasChanged = true;
				HighlightState = highlightState;
			}
			if (visitState != Celestial.VisitStates.Unknown && visitState != VisitState)
			{
				wasChanged = true;
				VisitState = visitState;
			}
			if (rangeState != Celestial.RangeStates.Unknown && rangeState != RangeState)
			{
				wasChanged = true;
				RangeState = rangeState;
			}
			if (selectedState != Celestial.SelectedStates.Unknown && selectedState != SelectedState)
			{
				wasChanged = true;
				SelectedState = selectedState;
			}
			if (travelState != Celestial.TravelStates.Unknown && travelState != TravelState)
			{
				wasChanged = true;
				TravelState = travelState;
			}

			if (wasChanged || instant) CalculateVisuals();

			if (instant)
			{
				ProcessAllVisuals(1f, true);
				isStale = false;
			}
			else isStale = isStale || wasChanged;

			return wasChanged;
		}

		public void ResetSelectionCooldown()
		{

		}
		#endregion

		#region Local Properties
		Celestial.HighlightStates HighlightState { get; set; }
		Celestial.VisitStates VisitState { get; set; }
		Celestial.RangeStates RangeState { get; set; }
		Celestial.SelectedStates SelectedState { get; set; }
		Celestial.TravelStates TravelState { get; set; }

		VisualState currentVisuals;
		VisualState targetVisuals;
		bool isStale;

		bool isDetailsLayoutStale;
		int? detailsLayoutDelay;
		int? dragTrailDelay;

		bool? wasInBounds;
		bool isInBounds;

		float? onEnterDelayRemaining;
		#endregion

		#region Overrides
		protected override void OnPosition(Vector3 position, Vector3 rawPosition) // These are world positions
		{
			var radiusNormal = RadiusNormal(dropLine.transform.position);
			isInBounds = radiusNormal < 1f;

			if (!isInBounds)
			{
				// Trails need to wait a fram before enabling so they don't zig zag across the grid.
				dragTrail.emitting = false;
				dragTrailDelay = 3;
			}
			else
			{
				var localPositionWithHeight = new Vector3(0f, yMinimumOffset + (rawPosition - position).y, 0f);
				verticalLookAtArea.transform.localPosition = localPositionWithHeight;


				var radiusOpacity = Opacity * dropLineRadiusOpacity.Evaluate(radiusNormal);
				dropLine.material.SetFloat(ShaderConstants.HoloDistanceFieldColorShiftConstant.Alpha, radiusOpacity);
				dropLine.SetPosition(1, localPositionWithHeight);
				baseGroup.alpha = radiusOpacity;

				SetMeshAlpha(bottomCenterMesh.material, ShaderConstants.HoloTextureColorAlphaMasked.Alpha, Opacity * currentVisuals.BaseCenterOpacity);

				var particleOpacity = Opacity * radiusOpacity * minimizedParticlesBaseAlpha;
				minimizedParticles.startColor = minimizedParticles.startColor.NewA(particleOpacity);
				minimizedParticles.gameObject.SetActive(!Mathf.Approximately(0f, particleOpacity));

				SetMeshAlpha(selectedGraphic.material, ShaderConstants.HoloTextureColorAlpha.Alpha, currentVisuals.SelectedOpacity);
				SetMeshAlpha(selectedInsideGraphic.material, ShaderConstants.HoloTextureColorAlpha.Alpha, currentVisuals.SelectedOpacity);

				SetMaximizeOpacity(currentVisuals.MaximizedProgress);

				if (!dragTrail.emitting)
				{
					if (dragTrailDelay.HasValue) dragTrailDelay--;
					
					if (!dragTrailDelay.HasValue || dragTrailDelay <= 0)
					{
						dragTrail.Clear();
						dragTrail.emitting = true;
					}
				}
			}
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			if (onEnterDelayRemaining.HasValue)
			{
				onEnterDelayRemaining -= delta;
				if (onEnterDelayRemaining <= 0f)
				{
					onEnterDelayRemaining = null;
					selectedParticles.Emit(1);
					if (Enter != null) Enter();
				}
			}

			// Some kludge to make sure the faded background lines up nicely with the description AND the name.
			if (isDetailsLayoutStale)
			{
				if (!detailsLayoutDelay.HasValue) detailsLayoutDelay = 1;
				else detailsLayoutDelay--;

				if (detailsLayoutDelay <= 0)
				{
					isDetailsLayoutStale = false;
					detailsLayoutDelay = null;
					LayoutRebuilder.MarkLayoutForRebuild(detailsContainer);
				}
			}

			if (!isStale) return;

			var currDelta = delta * transitionSpeed;

			isStale = ProcessAllVisuals(currDelta);
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			if (!wasInBounds.HasValue || isInBounds != wasInBounds)
			{
				OnInBoundsChanged(isInBounds);
			}

			if (!isInBounds) return;

			lookAtArea.LookAt(lookAtArea.position + App.V.CameraForward.FlattenY());
			verticalLookAtArea.LookAt(verticalLookAtArea.position + App.V.CameraForward);
		}

		public override void Reset()
		{
			base.Reset();

			Enter = ActionExtensions.Empty;
			Exit = ActionExtensions.Empty;
			Click = ActionExtensions.Empty;

			DetailsName = string.Empty;
			DetailsDescription = string.Empty;
			Confirm = string.Empty;
			ConfirmDescription = string.Empty;
			Distance = string.Empty;
			DistanceUnit = string.Empty;

			IconColor = Color.white;
			IconScale = 1f;

			dragTrail.emitting = false;
			dragTrail.Clear();

			wasInBounds = null;
		}
		#endregion

		#region Visuals
		void CalculateVisuals()
		{
			var modified = targetVisuals;
			//	DropLineTopOffset


			//	DropLineThickness
			modified.DropLineThickness = Constants.DropLineThickness.None;
			switch (HighlightState)
			{
				case Celestial.HighlightStates.Idle:
					switch (VisitState)
					{
						case Celestial.VisitStates.Current:
							modified.DropLineThickness = Constants.DropLineThickness.Full;
							break;
						default:
							switch (SelectedState)
							{
								case Celestial.SelectedStates.Selected:
								case Celestial.SelectedStates.NotSelected:
									modified.DropLineThickness = Constants.DropLineThickness.Normal;
									break;
							}
							break;
					}
					break;
				case Celestial.HighlightStates.OtherHighlighted:
					switch (VisitState)
					{
						case Celestial.VisitStates.Current:
							modified.DropLineThickness = Constants.DropLineThickness.Full;
							break;
						default:
							switch (SelectedState)
							{
								case Celestial.SelectedStates.Selected:
									modified.DropLineThickness = Constants.DropLineThickness.Full;
									break;
							}
							break;
					}
					break;
				case Celestial.HighlightStates.Highlighted:
				case Celestial.HighlightStates.HighlightedAnalysis:
					modified.DropLineThickness = Constants.DropLineThickness.Full;
					break;
			}

			//	DropLineThickness
			modified.DropLineShiftMapProgress = Constants.DropLineShiftMapProgress.Primary;
			switch (SelectedState)
			{
				case Celestial.SelectedStates.NotSelected:
				case Celestial.SelectedStates.OtherSelected:
					switch (VisitState)
					{
						case Celestial.VisitStates.Current:
							modified.DropLineShiftMapProgress = Constants.DropLineShiftMapProgress.Secondary;
							break;
					}
					break;
				case Celestial.SelectedStates.Selected:
					modified.DropLineShiftMapProgress = Constants.DropLineShiftMapProgress.Secondary;
					break;
			}

			//	DropLineBaseOpacity

			//	BaseDistanceThickness

			//	BaseDistanceOpacity
			modified.BaseDistanceOpacity = Constants.BaseDistanceOpacity.None;
			switch (VisitState)
			{
				case Celestial.VisitStates.Current: break;
				default:
					switch (HighlightState)
					{
						case Celestial.HighlightStates.OtherHighlighted:
							switch (SelectedState)
							{
								case Celestial.SelectedStates.Selected:
									modified.BaseDistanceOpacity = Constants.BaseDistanceOpacity.Full;
									break;
							}
							break;
						case Celestial.HighlightStates.Highlighted:
						case Celestial.HighlightStates.HighlightedAnalysis:
							switch (SelectedState)
							{
								case Celestial.SelectedStates.Selected:
								case Celestial.SelectedStates.NotSelected:
									modified.BaseDistanceOpacity = Constants.BaseDistanceOpacity.Full;
									break;
							}
							break;
						case Celestial.HighlightStates.Idle:
							switch (SelectedState)
							{
								case Celestial.SelectedStates.Selected:
									modified.BaseDistanceOpacity = Constants.BaseDistanceOpacity.Full;
									break;
							}
							break;
					}
					break;
			}

			//	BaseRingOpacity
			modified.BaseRingOpacity = Constants.BaseRingOpacity.None;
			switch (HighlightState)
			{
				case Celestial.HighlightStates.OtherHighlighted:
					switch (SelectedState)
					{
						case Celestial.SelectedStates.Selected:
							modified.BaseRingOpacity = Constants.BaseRingOpacity.Full;
							break;
						default:
							switch (VisitState)
							{
								case Celestial.VisitStates.Current:
									modified.BaseRingOpacity = Constants.BaseRingOpacity.Full;
									break;
							}
							break;
					}
					break;
				case Celestial.HighlightStates.Highlighted:
				case Celestial.HighlightStates.HighlightedAnalysis:
					switch (SelectedState)
					{
						case Celestial.SelectedStates.OtherSelected: break;
						default:
							modified.BaseRingOpacity = Constants.BaseRingOpacity.Full;
							break;
					}
					break;
				case Celestial.HighlightStates.Idle:
					switch (SelectedState)
					{
						case Celestial.SelectedStates.Selected:
							modified.BaseRingOpacity = Constants.BaseRingOpacity.Full;
							break;
					}
					break;
			}

			// BaseCenterOpacity
			modified.BaseCenterOpacity = Constants.BaseCenterOpacity.None;
			switch (VisitState)
			{
				case Celestial.VisitStates.Current:
					modified.BaseCenterOpacity = Constants.BaseCenterOpacity.Full;
					break;
				default:
					switch (SelectedState)
					{
						case Celestial.SelectedStates.Selected:
							modified.BaseCenterOpacity = Constants.BaseCenterOpacity.Full;
							break;
						default:
							switch (HighlightState)
							{
								case Celestial.HighlightStates.Highlighted:
								case Celestial.HighlightStates.HighlightedAnalysis:
									modified.BaseCenterOpacity = Constants.BaseCenterOpacity.Full;
									break;
							}
							break;
					}
					break;
			}

			//	DetailsOpacity
			modified.DetailsOpacity = Constants.DetailsOpacity.None;
			switch (SelectedState)
			{
				case Celestial.SelectedStates.NotSelected:
					switch (HighlightState)
					{
						case Celestial.HighlightStates.Highlighted: modified.DetailsOpacity = Constants.DetailsOpacity.Full; break;
					}
					break;
			}

			//	ConfirmOpacity
			modified.ConfirmOpacity = Constants.ConfirmOpacity.None;
			switch (SelectedState)
			{
				case Celestial.SelectedStates.Selected:
					switch (HighlightState)
					{
						case Celestial.HighlightStates.Highlighted: modified.ConfirmOpacity = Constants.ConfirmOpacity.Full; break;
					}
					break;
			}

			//	AnalysisOpacity

			//	SelectedOpacity
			modified.SelectedOpacity = Constants.SelectedOpacity.None;
			switch (VisitState)
			{
				case Celestial.VisitStates.Current:
					modified.SelectedOpacity = Constants.SelectedOpacity.Full;
					break;
				default:
					switch (SelectedState)
					{
						case Celestial.SelectedStates.Selected:
							modified.SelectedOpacity = Constants.SelectedOpacity.Full;
							break;
						case Celestial.SelectedStates.NotSelected:
							switch (HighlightState)
							{
								case Celestial.HighlightStates.Idle:
								case Celestial.HighlightStates.OtherHighlighted: 
									break;
								case Celestial.HighlightStates.Highlighted:
								case Celestial.HighlightStates.HighlightedAnalysis:
									modified.SelectedOpacity = Constants.SelectedOpacity.Full;
									break;
							}
							break;
					}
					break;
			}

			// MaximizedProgress
			modified.MaximizedProgress = Constants.MaximizedProgress.Minimized;
			switch (VisitState)
			{
				case Celestial.VisitStates.Current:
					switch (HighlightState)
					{
						case Celestial.HighlightStates.Highlighted:
						case Celestial.HighlightStates.HighlightedAnalysis:
							modified.MaximizedProgress = Constants.MaximizedProgress.Maximized;
							break;
					}
					break;
				default:
					switch (SelectedState)
					{
						case Celestial.SelectedStates.OtherSelected: break;
						case Celestial.SelectedStates.NotSelected:
							switch (HighlightState)
							{
								case Celestial.HighlightStates.Highlighted:
								case Celestial.HighlightStates.HighlightedAnalysis:
									modified.MaximizedProgress = Constants.MaximizedProgress.Maximized;
									break;
							}
							break;
						case Celestial.SelectedStates.Selected:
							switch (HighlightState)
							{
								case Celestial.HighlightStates.Highlighted:
								case Celestial.HighlightStates.HighlightedAnalysis:
									modified.MaximizedProgress = Constants.MaximizedProgress.Maximized;
									break;
							}
							break;
					}
					break;
			}


			targetVisuals = modified;
		}

		bool ProcessAllVisuals(float delta, bool force = false)
		{
			var wasChanged = false;

			currentVisuals.DropLineThickness = ProcessVisual(currentVisuals.DropLineThickness, targetVisuals.DropLineThickness, delta, ref wasChanged, force, ApplyDropLineThickness);
			currentVisuals.DropLineShiftMapProgress = ProcessVisual(currentVisuals.DropLineShiftMapProgress, targetVisuals.DropLineShiftMapProgress, delta, ref wasChanged, force, ApplyDropLineShiftMapProgress);

			// TODO: these...
			currentVisuals.DropLineBaseOpacity = ProcessVisual(currentVisuals.DropLineBaseOpacity, targetVisuals.DropLineBaseOpacity, delta, ref wasChanged, force);
			currentVisuals.BaseDistanceThickness = ProcessVisual(currentVisuals.BaseDistanceThickness, targetVisuals.BaseDistanceThickness, delta, ref wasChanged, force);
			// ---

			currentVisuals.BaseDistanceOpacity = ProcessVisual(currentVisuals.BaseDistanceOpacity, targetVisuals.BaseDistanceOpacity, delta, ref wasChanged, force, ApplyBaseDistanceOpacity);
			currentVisuals.BaseRingOpacity = ProcessVisual(currentVisuals.BaseRingOpacity, targetVisuals.BaseRingOpacity, delta, ref wasChanged, force, ApplyBaseRingOpacity);
			currentVisuals.BaseCenterOpacity = ProcessVisual(currentVisuals.BaseCenterOpacity, targetVisuals.BaseCenterOpacity, delta, ref wasChanged, force, ApplyBaseCenterOpacity);
			currentVisuals.DetailsOpacity = ProcessVisual(currentVisuals.DetailsOpacity, targetVisuals.DetailsOpacity, delta, ref wasChanged, force, ApplyDetailsOpacity);
			currentVisuals.ConfirmOpacity = ProcessVisual(currentVisuals.ConfirmOpacity, targetVisuals.ConfirmOpacity, delta, ref wasChanged, force, ApplyConfirmOpacity);

			// TODO: these...
			currentVisuals.AnalysisOpacity = ProcessVisual(currentVisuals.AnalysisOpacity, targetVisuals.AnalysisOpacity, delta, ref wasChanged, force);
			// ---

			currentVisuals.SelectedOpacity = ProcessVisual(currentVisuals.SelectedOpacity, targetVisuals.SelectedOpacity, delta, ref wasChanged, force, ApplySelectedOpacity);
			currentVisuals.MaximizedProgress = ProcessVisual(currentVisuals.MaximizedProgress, targetVisuals.MaximizedProgress, delta, ref wasChanged, force, ApplyMaximizedProgress);

			return wasChanged;
		}

		// TODO: Make onChange a required parameter...
		float ProcessVisual(float current, float target, float transitionDelta, ref bool wasChanged, bool force, Action<float> onChange = null)
		{
			var result = 0f;
			if (current < target) result = Mathf.Min(current + transitionDelta, target);
			else result = Mathf.Max(current - transitionDelta, target);
			var currentChanged = !Mathf.Approximately(current, result);
			if (onChange != null && (force || currentChanged)) onChange(result);
			wasChanged = wasChanged || currentChanged;
			return result;
		}
		#endregion

		#region Visual Applications
		void ApplyDropLineThickness(float value)
		{
			dropLine.widthMultiplier = dropLineThickness.Evaluate(value) * dropLineThicknessMaximum;
			SetMeshAlpha(dropLine.material, ShaderConstants.HoloDistanceFieldColorShiftConstant.Alpha, dropLineThicknessOpacity.Evaluate(value));
		}

		void ApplyDropLineShiftMapProgress(float value)
		{
			dropLine.material.SetFloat(ShaderConstants.HoloDistanceFieldColorShiftConstant.ShiftMapProgress, dropLineShiftMapProgress.Evaluate(value));
		}

		void ApplySelectedOpacity(float value)
		{
			SetMeshAlpha(selectedGraphic.material, ShaderConstants.HoloTextureColorAlpha.Alpha, value);
			SetMeshAlpha(selectedInsideGraphic.material, ShaderConstants.HoloTextureColorAlpha.Alpha, value);
		}

		void ApplyDetailsOpacity(float value)
		{
			detailsGroup.alpha = value;
		}

		void ApplyConfirmOpacity(float value)
		{
			confirmGroup.alpha = value;
		}

		void ApplyBaseDistanceOpacity(float value)
		{
			baseDistanceGroup.alpha = value;
		}

		void ApplyBaseRingOpacity(float value)
		{
			baseRingGroup.alpha = value;
		}

		void ApplyBaseCenterOpacity(float value)
		{
			SetMeshAlpha(bottomCenterMesh.material, ShaderConstants.HoloTextureColorAlphaMasked.Alpha, value);
		}

		void ApplyMaximizedProgress(float value)
		{
			var scaleProgress = maximizeScale.Evaluate(value);
			foreach (var entry in maximizeAreas)
			{
				entry.Area.localScale = Vector3.one * (entry.MinimumScale + ((1f - entry.MinimumScale) * scaleProgress));
			}

			SetMaximizeOpacity(value);
		}
		#endregion

		#region Events
		public void OnEnter()
		{
			if (NotInteractable) return;

			onEnterDelayRemaining = onEnterDelayDuration;
		}

		public void OnExit()
		{
			if (NotInteractable) return;

			if (onEnterDelayRemaining.HasValue)
			{
				onEnterDelayRemaining = null;
				return;
			}

			switch(HighlightState)
			{
				case Celestial.HighlightStates.Highlighted:
				case Celestial.HighlightStates.HighlightedAnalysis:
					if (Exit != null) Exit();
					break;
			}
		}

		public void OnClick()
		{
			if (NotInteractable) return;

			if (Click != null) Click();
		}
		#endregion

		bool NotInteractable
		{
			get
			{
				if (!Interactable) return true;
				if (App.V.CameraHasMoved) return true;
				return Opacity <= interactableOpacityCutoff;
			}
		}

		void SetMaximizeOpacity(float value)
		{
			var appearOpacity = value;
			var disappearOpacity = 1f - value;

			foreach (var entry in maximizeOpacityGraphics)
			{
				SetMeshAlpha(entry.Graphic.material, entry.AlphaKey, entry.AppearsWhenMinimized ? disappearOpacity : appearOpacity);
			}
		}

		void SetMeshAlpha(Material material, string fieldName, float alpha = 1f)
		{
			material.SetFloat(fieldName, Opacity * alpha * dropLineRadiusOpacity.Evaluate(RadiusNormal(dropLine.transform.position)));
		}

		void OnInBoundsChanged(bool value)
		{
			foreach (var entry in enableOnInBounds)
			{
				entry.SetActive(value);
			}

			interactableGroup.interactable = value;
			interactableGroup.blocksRaycasts = value;

			if (value) isDetailsLayoutStale = true;

			wasInBounds = value;
		}
	}

	public interface ICelestialSystemView : IUniverseScaleView
	{
		string DetailsName { set; }
		string DetailsDescription { set; }
		string Confirm { set; }
		string ConfirmDescription { set; }
		string Distance { set; }
		string DistanceUnit { set; }

		Color IconColor { set; }
		float IconScale { set; }

		Action Enter { set; }
		Action Exit { set; }
		Action Click { set; }

		bool SetStates(
			Celestial.HighlightStates highlightState = Celestial.HighlightStates.Unknown,
			Celestial.VisitStates visitState = Celestial.VisitStates.Unknown,
			Celestial.RangeStates rangeState = Celestial.RangeStates.Unknown,
			Celestial.SelectedStates selectedState = Celestial.SelectedStates.Unknown,
			Celestial.TravelStates travelState = Celestial.TravelStates.Unknown,
			bool instant = false
		);

		//string ClassificationText { set; }
		//string DistanceText { set; }
		//string DistanceUnitText { set; }
		//string AnalysisText { set; }
		//string AnalysisDetailsText { set; }
		//string ConfirmText { set; }
		//string ConfirmDetailsText { set; }
		//Texture2D ClassificationIcon { set; }
		//Texture2D SystemIcon { set; }
		//float ClassificationHue { set; }
		//float SystemHue { set; }
		//Action ClassificationClick { set; }
		//Action SystemClick { set; }
		//float Height { set; }
	}
}