using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public static class Celestial
	{
		public enum StateTypes
		{
			Unknown = 0,
			Highlight = 10,
			Visit = 20,
			Range = 30,
			Selected = 40,
			Travel = 50
		}

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
		public Action Enter { set; private get; }
		public Action Exit { set; private get; }
		public Action Click { set; private get; }

		Celestial.HighlightStates HighlightState { get; set; }
		Celestial.VisitStates VisitState { get; set; }
		Celestial.RangeStates RangeState { get; set; }
		Celestial.SelectedStates SelectedState { get; set; }
		Celestial.TravelStates TravelState { get; set; }

		[Serializable]
		struct VisualState
		{
			public float DropLineTopOffset;
			public float DropLineThickness;
			public float DropLineBaseVisibility;
			public float BaseDistanceThickness;
			public float BaseDistanceVisibility;
			public float DetailsVisibility;
			public float ConfirmVisibility;
			public float AnalysisVisibility;
			public float OutlineThickness;
			public float IconColorThickness;
			public float Height;
			public float Dimming;
		}

		VisualState currentVisuals;
		VisualState targetVisuals;
		bool isStale;

		public bool SetStates(
			Celestial.HighlightStates highlightState = Celestial.HighlightStates.Unknown,
			Celestial.VisitStates visitState = Celestial.VisitStates.Unknown,
			Celestial.RangeStates rangeState = Celestial.RangeStates.Unknown,
			Celestial.SelectedStates selectedState = Celestial.SelectedStates.Unknown,
			Celestial.TravelStates travelState = Celestial.TravelStates.Unknown
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
			isStale = isStale || wasChanged;

			return wasChanged;
		}

		void CalculateVisuals()
		{
			var modified = targetVisuals;
			//	DropLineTopOffset


			//	DropLineThickness
			modified.DropLineThickness = 0f;
			switch (HighlightState)
			{
				case Celestial.HighlightStates.Idle:
					switch (VisitState)
					{
						case Celestial.VisitStates.Current:
							modified.DropLineThickness = 1f;
							break;
						default:
							modified.DropLineThickness = 0.5f;
							break;
					}
					break;
				case Celestial.HighlightStates.OtherHighlighted:
					if (VisitState == Celestial.VisitStates.Current) modified.DropLineThickness = 1f;
					break;
				case Celestial.HighlightStates.Highlighted:
				case Celestial.HighlightStates.HighlightedAnalysis:
					modified.DropLineThickness = 1f;
					break;
			}

			//	DropLineBaseVisibility

			//	BaseDistanceThickness

			//	BaseDistanceVisibility

			//	DetailsVisibility

			//	ConfirmVisibility

			//	AnalysisVisibility

			//	OutlineThickness

			//	IconColorThickness

			//	Height

			//	Dimming

			targetVisuals = modified;
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + App.V.CameraForward.FlattenY());
			//verticalLookAtArea.LookAt(verticalLookAtArea.position + App.V.CameraForward);
		}


		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			if (!isStale) return;

			var currDelta = delta * transitionSpeed;

			var wasChanged = false;

			currentVisuals.DropLineTopOffset = ProcessVisual(currentVisuals.DropLineTopOffset, targetVisuals.DropLineTopOffset, currDelta, ref wasChanged);
			currentVisuals.DropLineThickness = ProcessVisual(currentVisuals.DropLineThickness, targetVisuals.DropLineThickness, currDelta, ref wasChanged, ApplyDropLineThickness);
			currentVisuals.DropLineBaseVisibility = ProcessVisual(currentVisuals.DropLineBaseVisibility, targetVisuals.DropLineBaseVisibility, currDelta, ref wasChanged);
			currentVisuals.BaseDistanceThickness = ProcessVisual(currentVisuals.BaseDistanceThickness, targetVisuals.BaseDistanceThickness, currDelta, ref wasChanged);
			currentVisuals.BaseDistanceVisibility = ProcessVisual(currentVisuals.BaseDistanceVisibility, targetVisuals.BaseDistanceVisibility, currDelta, ref wasChanged);
			currentVisuals.DetailsVisibility = ProcessVisual(currentVisuals.DetailsVisibility, targetVisuals.DetailsVisibility, currDelta, ref wasChanged);
			currentVisuals.ConfirmVisibility = ProcessVisual(currentVisuals.ConfirmVisibility, targetVisuals.ConfirmVisibility, currDelta, ref wasChanged);
			currentVisuals.AnalysisVisibility = ProcessVisual(currentVisuals.AnalysisVisibility, targetVisuals.AnalysisVisibility, currDelta, ref wasChanged);
			currentVisuals.OutlineThickness = ProcessVisual(currentVisuals.OutlineThickness, targetVisuals.OutlineThickness, currDelta, ref wasChanged);
			currentVisuals.IconColorThickness = ProcessVisual(currentVisuals.IconColorThickness, targetVisuals.IconColorThickness, currDelta, ref wasChanged);
			currentVisuals.Height = ProcessVisual(currentVisuals.Height, targetVisuals.Height, currDelta, ref wasChanged);
			currentVisuals.Dimming = ProcessVisual(currentVisuals.Dimming, targetVisuals.Dimming, currDelta, ref wasChanged);
				
			isStale = wasChanged;
		}

		float ProcessVisual(float current, float target, float transitionDelta, ref bool wasChanged, Action<float> onChange = null)
		{
			var result = 0f;
			if (current < target) result = Mathf.Min(current + transitionDelta, target);
			else result = Mathf.Max(current - transitionDelta, target);
			var currentChanged = !Mathf.Approximately(current, result);
			if (currentChanged && onChange != null) onChange(result); 
			wasChanged = wasChanged || currentChanged;
			return result;
		}

		protected override void OnPosition(Vector3 position)
		{
			var radiusNormal = RadiusNormal(dropLine.transform.position);
			dropLine.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, Opacity * dropLineRadiusOpacity.Evaluate(radiusNormal));
			var inBounds = radiusNormal < 1f;
			group.interactable = inBounds;
			group.blocksRaycasts = inBounds;
		}

		#region Visual Applications
		void ApplyDropLineThickness(float value)
		{
			dropLine.widthMultiplier = dropLineThickness.Evaluate(value) * dropLineThicknessMaximum;
			bottomCenterMesh.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, Opacity * bottomCenterOpacity.Evaluate(value));
		}
		#endregion

		#region Children
		[Header("Children")]
		[SerializeField]
		float transitionSpeed;
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		CanvasGroup group;

		[SerializeField]
		float dropLineThicknessMaximum;
		[SerializeField]
		AnimationCurve dropLineThickness;
		[SerializeField]
		LineRenderer dropLine;
		[SerializeField]
		AnimationCurve dropLineRadiusOpacity;

		[SerializeField]
		MeshRenderer bottomCenterMesh;
		[SerializeField]
		AnimationCurve bottomCenterOpacity;
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

		public override void Reset()
		{
			base.Reset();

			Enter = ActionExtensions.Empty;
			Exit = ActionExtensions.Empty;
			Click = ActionExtensions.Empty;
		}

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;
				dropLine.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, value * dropLineRadiusOpacity.Evaluate(RadiusNormal(dropLine.transform.position)));

			}
		}
	}

	public interface ICelestialSystemView : IUniverseScaleView
	{
		Action Enter { set; }
		Action Exit { set; }
		Action Click { set; }

		bool SetStates(
			Celestial.HighlightStates highlightState = Celestial.HighlightStates.Unknown,
			Celestial.VisitStates visitState = Celestial.VisitStates.Unknown,
			Celestial.RangeStates rangeState = Celestial.RangeStates.Unknown,
			Celestial.SelectedStates selectedState = Celestial.SelectedStates.Unknown,
			Celestial.TravelStates travelState = Celestial.TravelStates.Unknown
		);

		//string Name { set; }
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