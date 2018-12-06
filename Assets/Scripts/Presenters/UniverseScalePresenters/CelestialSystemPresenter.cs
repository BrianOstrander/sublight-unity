using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CelestialSystemPresenter : UniverseScalePresenter<ICelestialSystemView>
	{
		Celestial.HighlightStates highlightState;
		Celestial.VisitStates visitState;
		Celestial.RangeStates rangeState;
		Celestial.SelectedStates selectedState;
		Celestial.TravelStates travelState;

		UniversePosition positionInUniverse;

		protected override bool CanShow { get { return instanceModel.HasSystem; } }

		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		SystemInstanceModel instanceModel;
		CelestialSystemLanguageBlock language;
		UniverseScales lastZoomToScale;

		public CelestialSystemPresenter(
			GameModel model,
			UniverseScales scale,
			SystemInstanceModel instanceModel,
			CelestialSystemLanguageBlock language
		) : base(model, scale)
		{
			this.instanceModel = instanceModel;
			this.language = language;

			instanceModel.ActiveSystem.Changed += OnActiveSystem;

			App.Callbacks.Click += OnGlobalClick;

			Model.CelestialSystemState.Changed += OnCelestialSystemState;
			Model.CameraTransform.Changed += OnCameraTransform;
			Model.GridInput.Changed += OnGridInput;
			Model.FocusTransform.Changed += OnFocusTransform;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			instanceModel.ActiveSystem.Changed -= OnActiveSystem;

			App.Callbacks.Click -= OnGlobalClick;

			Model.CelestialSystemState.Changed -= OnCelestialSystemState;
			Model.CameraTransform.Changed -= OnCameraTransform;
			Model.GridInput.Changed -= OnGridInput;
			Model.FocusTransform.Changed -= OnFocusTransform;

			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		void ApplyStates(bool instant = false)
		{
			View.SetStates(
				highlightState,
				visitState,
				rangeState,
				selectedState,
				travelState,
				instant
			);
		}

		#region Events
		void OnActiveSystem(SystemModel activeSystem)
		{
			if (activeSystem == null || !ScaleModel.IsVisible)
			{
				if (View.Visible) CloseViewInstant();
				return;
			}

			if (View.Visible)
			{
				View.Reset();
				ForceApplyScaleTransform();
				OnShowView();
			}
			else
			{
				View.Reset();
				ForceApplyScaleTransform();
				OnShowView();
				ShowView(instant: true);
			}
		}

		void OnGlobalClick(Click click)
		{
			if (!click.ClickedNothing) return;
			if (App.V.CameraHasMoved) return;
			if (!Mathf.Approximately(1f, ScaleModel.Opacity.Value)) return;

			switch(selectedState)
			{
				case Celestial.SelectedStates.Selected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.UnSelect(positionInUniverse);
					break;
			}
		}

		protected override void OnShowView()
		{
			var activeSystem = instanceModel.ActiveSystem.Value;

			positionInUniverse = activeSystem.Position.Value;

			SetGrid(ScaleModel.Transform.Value.UnityOrigin, ScaleModel.Transform.Value.UnityRadius);

			View.Enter = OnEnter;
			View.Exit = OnExit;
			View.Click = OnClick;

			highlightState = Celestial.HighlightStates.Idle;

			if (activeSystem.Position.Value.Equals(Model.Ship.Value.Position.Value)) visitState = Celestial.VisitStates.Current;
			else visitState = activeSystem.Visited.Value ? Celestial.VisitStates.Visited : Celestial.VisitStates.NotVisited;

			rangeState = Celestial.RangeStates.InRange;

			switch (Model.CelestialSystemStateLastSelected.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					selectedState = Model.CelestialSystemStateLastSelected.Position.Equals(activeSystem.Position.Value) ? Celestial.SelectedStates.Selected : Celestial.SelectedStates.OtherSelected;
					break;
				default:
					selectedState = Celestial.SelectedStates.NotSelected;
					break;
			}

			travelState = Celestial.TravelStates.NotTraveling;

			View.DetailsName = activeSystem.Name.Value;
			View.DetailsDescription = language.PrimaryClassifications[activeSystem.PrimaryClassification.Value].Value.Value + " - " + activeSystem.SecondaryClassification.Value;
			View.Confirm = language.Confirm.Value;
			View.ConfirmDescription = language.ConfirmDescription.Value;

			var lightyearDistance = UniversePosition.ToLightYearDistance(UniversePosition.Distance(Model.Ship.Value.Position.Value, activeSystem.Position.Value));
			var lightyearText = lightyearDistance < 10f ? lightyearDistance.ToString("N1") : Mathf.RoundToInt(lightyearDistance).ToString("N0");

			View.Distance = lightyearText;
			View.DistanceUnit = language.DistanceUnit.Value;

			View.IconColor = activeSystem.IconColor.Value;
			View.IconScale = activeSystem.IconScale.Value;

			ApplyStates(true);
		}

		void OnEnter()
		{
			Model.CelestialSystemState.Value = CelestialSystemStateBlock.Highlight(positionInUniverse);
		}

		void OnExit()
		{
			switch (selectedState)
			{
				case Celestial.SelectedStates.NotSelected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle(Model.Ship.Value.Position);
					break;
				case Celestial.SelectedStates.Selected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle(positionInUniverse);
					break;
				case Celestial.SelectedStates.OtherSelected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle(Model.CelestialSystemStateLastSelected.Position);
					break;
			}

		}

		void OnClick()
		{
			switch (selectedState)
			{
				case Celestial.SelectedStates.OtherSelected:
				case Celestial.SelectedStates.NotSelected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Select(positionInUniverse);
					break;
			}
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			if (OnCelestialSystemStateProcess(block.Position.Equals(positionInUniverse), block))
			{
				if (View.Visible) ApplyStates();
			}
		}

		bool OnCelestialSystemStateProcess(bool isCurrent, CelestialSystemStateBlock block)
		{
			switch (block.State)
			{
				case CelestialSystemStateBlock.States.Idle:
					highlightState = Celestial.HighlightStates.Idle;
					return true;
				case CelestialSystemStateBlock.States.Highlighted:
					if (isCurrent) highlightState = Celestial.HighlightStates.Highlighted;
					else highlightState = Celestial.HighlightStates.OtherHighlighted;
					return true;
				case CelestialSystemStateBlock.States.Selected:
					if (isCurrent)
					{
						highlightState = Celestial.HighlightStates.Highlighted;
						selectedState = Celestial.SelectedStates.Selected;
					}
					else
					{
						highlightState = Celestial.HighlightStates.OtherHighlighted;
						selectedState = Celestial.SelectedStates.OtherSelected;
					}
					return true;
				case CelestialSystemStateBlock.States.UnSelected:
					selectedState = Celestial.SelectedStates.NotSelected;
					return true;
				default:
					Debug.Log("Unrecognized state: " + block.State);
					return false;
			}
		}

		void OnCameraTransform(CameraTransformRequest transform)
		{
			if (!View.Visible) return;
			ProcessInterectable(transform, Model.GridInput.Value);
		}

		void OnGridInput(GridInputRequest gridInput)
		{
			if (!View.Visible) return;
			ProcessInterectable(Model.CameraTransform.Value, gridInput);
		}

		void ProcessInterectable(CameraTransformRequest transform, GridInputRequest gridInput)
		{
			View.Interactable = transform.State == CameraTransformRequest.States.Complete && gridInput.State == GridInputRequest.States.Complete;
		}

		void OnFocusTransform(FocusTransform transform)
		{
			if (!View.Visible || lastZoomToScale == transform.ToScale)
			{
				lastZoomToScale = transform.ToScale;
				return;
			}

			lastZoomToScale = transform.ToScale;
			if (lastZoomToScale == Scale) return;

			if (View.Visible && !View.IsInBounds) CloseViewInstant();
		}
		#endregion
	}
}