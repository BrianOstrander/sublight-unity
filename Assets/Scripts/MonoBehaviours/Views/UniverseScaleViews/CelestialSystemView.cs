using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

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
		}

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
		}

		VisualState currentVisuals;
		VisualState targetVisuals;
		bool isStale;

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
					switch(HighlightState)
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

			targetVisuals = modified;
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + App.V.CameraForward.FlattenY());
			verticalLookAtArea.LookAt(verticalLookAtArea.position + App.V.CameraForward);
		}


		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			if (!isStale) return;

			var currDelta = delta * transitionSpeed;

			isStale = ProcessAllVisuals(currDelta);
		}

		bool ProcessAllVisuals(float delta, bool force = false)
		{
			var wasChanged = false;

			currentVisuals.DropLineTopOffset = ProcessVisual(currentVisuals.DropLineTopOffset, targetVisuals.DropLineTopOffset, delta, ref wasChanged, force);

			// done
			currentVisuals.DropLineThickness = ProcessVisual(currentVisuals.DropLineThickness, targetVisuals.DropLineThickness, delta, ref wasChanged, force, ApplyDropLineThickness);

			currentVisuals.DropLineBaseOpacity = ProcessVisual(currentVisuals.DropLineBaseOpacity, targetVisuals.DropLineBaseOpacity, delta, ref wasChanged, force);
			currentVisuals.BaseDistanceThickness = ProcessVisual(currentVisuals.BaseDistanceThickness, targetVisuals.BaseDistanceThickness, delta, ref wasChanged, force);

			//done
			currentVisuals.BaseDistanceOpacity = ProcessVisual(currentVisuals.BaseDistanceOpacity, targetVisuals.BaseDistanceOpacity, delta, ref wasChanged, force, ApplyBaseDistanceOpacity);
			currentVisuals.DetailsOpacity = ProcessVisual(currentVisuals.DetailsOpacity, targetVisuals.DetailsOpacity, delta, ref wasChanged, force, ApplyDetailsOpacity);
			currentVisuals.ConfirmOpacity = ProcessVisual(currentVisuals.ConfirmOpacity, targetVisuals.ConfirmOpacity, delta, ref wasChanged, force, ApplyConfirmOpacity);

			currentVisuals.AnalysisOpacity = ProcessVisual(currentVisuals.AnalysisOpacity, targetVisuals.AnalysisOpacity, delta, ref wasChanged, force);

			// done
			currentVisuals.SelectedOpacity = ProcessVisual(currentVisuals.SelectedOpacity, targetVisuals.SelectedOpacity, delta, ref wasChanged, force, ApplySelectedOpacity);

			// done
			currentVisuals.IconColorProgress = ProcessVisual(currentVisuals.IconColorProgress, targetVisuals.IconColorProgress, delta, ref wasChanged, force, ApplyIconColorProgress);

			currentVisuals.Height = ProcessVisual(currentVisuals.Height, targetVisuals.Height, delta, ref wasChanged, force);
			currentVisuals.Dimming = ProcessVisual(currentVisuals.Dimming, targetVisuals.Dimming, delta, ref wasChanged, force);

			return wasChanged;
		}

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

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{
			var radiusNormal = RadiusNormal(dropLine.transform.position);
			dropLine.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstant.Alpha, Opacity * dropLineRadiusOpacity.Evaluate(radiusNormal));
			var inBounds = radiusNormal < 1f;
			interactableGroup.interactable = inBounds;
			interactableGroup.blocksRaycasts = inBounds;
		}

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
		#endregion

		#region Children
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
				//dropLine.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, value * );
				SetMeshAlpha(dropLine.material, ShaderConstants.HoloDistanceFieldColorConstant.Alpha, dropLineRadiusOpacity.Evaluate(RadiusNormal(dropLine.transform.position)));
			}
		}

		void SetMeshAlpha(Material material, string fieldName, float alpha)
		{
			material.SetFloat(fieldName, Opacity * alpha);
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
			Celestial.TravelStates travelState = Celestial.TravelStates.Unknown,
			bool instant = false
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