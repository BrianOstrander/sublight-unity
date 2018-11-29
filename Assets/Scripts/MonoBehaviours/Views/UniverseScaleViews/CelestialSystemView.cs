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

			public static class MaximizedProgress
			{
				public const float Minimized = 0f;
				public const float Maximized = 1f;
			}
		}

		[Serializable]
		struct VisualState
		{
			public float DropLineTopOffset;
			public float DropLineThickness;
			public float DropLineBaseOpacity;
			public float BaseDistanceThickness;
			public float BaseDistanceOpacity;
			public float DetailsOpacity;
			public float ConfirmOpacity;
			public float AnalysisOpacity;
			public float SelectedOpacity;
			public float IconColorProgress;
			public float Height;
			public float Dimming;
			public float MaximizedProgress;
		}

		[Serializable]
		struct MaximizeOpacityGraphic
		{
			public MeshRenderer Graphic;
			public string AlphaKey;
			public bool AppearsWhenMinimized;
		}

		[Serializable]
		struct MaximizeArea
		{
			public Transform Area;
			public float MinimumScale;
		}
		#endregion

		#region Serialized Properties
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
		AnimationCurve dropLineRadiusOpacity;

		[SerializeField]
		MeshRenderer bottomCenterMesh;
		[SerializeField]
		AnimationCurve bottomCenterOpacity;

		[SerializeField]
		MeshRenderer selectedGraphic;
		[SerializeField]
		MeshRenderer colorGraphic;
		[SerializeField]
		MeshRenderer iconGraphic;

		[SerializeField]
		CanvasGroup detailsGroup;
		[SerializeField]
		CanvasGroup confirmGroup;
		[SerializeField]
		CanvasGroup baseDistanceGroup;

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
		#endregion

		#region View Properties & Methods
		public string DetailsName { set { detailsNameLabel.text = value ?? string.Empty; isDetailsLayoutStale = true; } }
		public string DetailsDescription { set { detailsDescriptionLabel.text = value ?? string.Empty; isDetailsLayoutStale = true; } }
		public string Confirm { set { confirmLabel.text = value ?? string.Empty; } }
		public string ConfirmDescription { set { confirmDescriptionLabel.text = value ?? string.Empty; } }
		public string Distance { set { distanceLabel.text = value ?? string.Empty; } }
		public string DistanceUnit { set { distanceUnitLabel.text = value ?? string.Empty; } }

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

			if (wasChanged) CalculateVisuals();

			if (instant)
			{
				ProcessAllVisuals(1f, true);
				isStale = false;
			}
			else isStale = isStale || wasChanged;

			return wasChanged;
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
		#endregion

		#region Overrides
		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;

				SetMeshAlpha(dropLine.material, ShaderConstants.HoloDistanceFieldColorConstant.Alpha);
			}
		}

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{
			var positionWithHeight = new Vector3(0f, yMinimumOffset + (rawPosition - position).y, 0f);
			verticalLookAtArea.transform.localPosition = positionWithHeight;

			var radiusNormal = RadiusNormal(dropLine.transform.position);
			dropLine.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstant.Alpha, Opacity * dropLineRadiusOpacity.Evaluate(radiusNormal));
			dropLine.SetPosition(1, positionWithHeight);

			SetMeshAlpha(selectedGraphic.material, ShaderConstants.HoloTextureColorAlpha.Alpha, currentVisuals.SelectedOpacity);

			SetMaximizeOpacity(currentVisuals.MaximizedProgress);

			var inBounds = radiusNormal < 1f;

			interactableGroup.interactable = inBounds;
			interactableGroup.blocksRaycasts = inBounds;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

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
							modified.DropLineThickness = Constants.DropLineThickness.Normal;
							break;
					}
					break;
				case Celestial.HighlightStates.OtherHighlighted:
					if (VisitState == Celestial.VisitStates.Current) modified.DropLineThickness = Constants.DropLineThickness.Full;
					break;
				case Celestial.HighlightStates.Highlighted:
				case Celestial.HighlightStates.HighlightedAnalysis:
					modified.DropLineThickness = Constants.DropLineThickness.Full;
					break;
			}

			//	DropLineBaseOpacity

			//	BaseDistanceThickness

			//	BaseDistanceOpacity
			modified.BaseDistanceOpacity = Constants.BaseDistanceOpacity.None;
			switch (HighlightState)
			{
				case Celestial.HighlightStates.Highlighted:
				case Celestial.HighlightStates.HighlightedAnalysis:
					modified.BaseDistanceOpacity = Constants.BaseDistanceOpacity.Full;
					break;
			}

			//	DetailsOpacity
			modified.DetailsOpacity = Constants.DetailsOpacity.None;
			switch (SelectedState)
			{
				case Celestial.SelectedStates.NotSelected:
				case Celestial.SelectedStates.OtherSelected:
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
			switch (SelectedState)
			{
				case Celestial.SelectedStates.Selected:
					modified.SelectedOpacity = Constants.SelectedOpacity.Full;
					break;
				default:
					switch (HighlightState)
					{
						case Celestial.HighlightStates.Idle:
						case Celestial.HighlightStates.OtherHighlighted:
							break;
						default:
							modified.SelectedOpacity = Constants.SelectedOpacity.Full;
							break;
					}
					break;
			}

			//	IconColorProgress
			modified.IconColorProgress = Constants.IconColorProgress.NotVisited;
			switch (VisitState)
			{
				case Celestial.VisitStates.Visited: modified.IconColorProgress = Constants.IconColorProgress.Visited; break;
				case Celestial.VisitStates.Current: modified.IconColorProgress = Constants.IconColorProgress.Current; break;
			}

			//	Height

			//	Dimming

			// MaximizedProgress
			modified.MaximizedProgress = Constants.MaximizedProgress.Minimized;
			switch (SelectedState)
			{
				case Celestial.SelectedStates.NotSelected:
				case Celestial.SelectedStates.OtherSelected:
					switch (HighlightState)
					{
						case Celestial.HighlightStates.Highlighted:
						case Celestial.HighlightStates.HighlightedAnalysis:
							modified.MaximizedProgress = Constants.MaximizedProgress.Maximized;
							break;
					}
					break;
				default:
					modified.MaximizedProgress = Constants.MaximizedProgress.Maximized;
					break;
			}

			targetVisuals = modified;
		}

		bool ProcessAllVisuals(float delta, bool force = false)
		{
			var wasChanged = false;

			// TODO: these...
			currentVisuals.DropLineTopOffset = ProcessVisual(currentVisuals.DropLineTopOffset, targetVisuals.DropLineTopOffset, delta, ref wasChanged, force);
			// ---

			currentVisuals.DropLineThickness = ProcessVisual(currentVisuals.DropLineThickness, targetVisuals.DropLineThickness, delta, ref wasChanged, force, ApplyDropLineThickness);

			// TODO: these...
			currentVisuals.DropLineBaseOpacity = ProcessVisual(currentVisuals.DropLineBaseOpacity, targetVisuals.DropLineBaseOpacity, delta, ref wasChanged, force);
			currentVisuals.BaseDistanceThickness = ProcessVisual(currentVisuals.BaseDistanceThickness, targetVisuals.BaseDistanceThickness, delta, ref wasChanged, force);
			// ---

			currentVisuals.BaseDistanceOpacity = ProcessVisual(currentVisuals.BaseDistanceOpacity, targetVisuals.BaseDistanceOpacity, delta, ref wasChanged, force, ApplyBaseDistanceOpacity);
			currentVisuals.DetailsOpacity = ProcessVisual(currentVisuals.DetailsOpacity, targetVisuals.DetailsOpacity, delta, ref wasChanged, force, ApplyDetailsOpacity);
			currentVisuals.ConfirmOpacity = ProcessVisual(currentVisuals.ConfirmOpacity, targetVisuals.ConfirmOpacity, delta, ref wasChanged, force, ApplyConfirmOpacity);

			// TODO: these...
			currentVisuals.AnalysisOpacity = ProcessVisual(currentVisuals.AnalysisOpacity, targetVisuals.AnalysisOpacity, delta, ref wasChanged, force);
			// ---

			currentVisuals.SelectedOpacity = ProcessVisual(currentVisuals.SelectedOpacity, targetVisuals.SelectedOpacity, delta, ref wasChanged, force, ApplySelectedOpacity);
			currentVisuals.IconColorProgress = ProcessVisual(currentVisuals.IconColorProgress, targetVisuals.IconColorProgress, delta, ref wasChanged, force, ApplyIconColorProgress);

			// TODO: these...
			currentVisuals.Height = ProcessVisual(currentVisuals.Height, targetVisuals.Height, delta, ref wasChanged, force);
			currentVisuals.Dimming = ProcessVisual(currentVisuals.Dimming, targetVisuals.Dimming, delta, ref wasChanged, force);
			// ---

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
			SetMeshAlpha(dropLine.material, ShaderConstants.HoloDistanceFieldColorConstant.Alpha, dropLineThicknessOpacity.Evaluate(value));
			SetMeshAlpha(bottomCenterMesh.material, ShaderConstants.HoloTextureColorAlpha.Alpha, bottomCenterOpacity.Evaluate(value));
		}

		void ApplySelectedOpacity(float value)
		{
			SetMeshAlpha(selectedGraphic.material, ShaderConstants.HoloTextureColorAlpha.Alpha, value);
		}

		void ApplyIconColorProgress(float value)
		{
			colorGraphic.material.SetFloat(ShaderConstants.HoloCelestialSystemIconColor.Progress, value);
			iconGraphic.material.SetFloat(ShaderConstants.HoloCelestialSystemIcon.Progress, value);
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
			if (Enter != null) Enter();
		}

		public void OnExit()
		{
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
			if (Click != null) Click();
		}
		#endregion

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
	}

	public interface ICelestialSystemView : IUniverseScaleView
	{
		string DetailsName { set; }
		string DetailsDescription { set; }
		string Confirm { set; }
		string ConfirmDescription { set; }
		string Distance { set; }
		string DistanceUnit { set; }

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