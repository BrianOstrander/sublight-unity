using System;

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
			Model.Ship.Value.Position.Changed += OnShipPosition;
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
			Model.Ship.Value.Position.Changed -= OnShipPosition;
		}

		void ApplyStates(bool instant = false)
		{
			switch (rangeState)
			{
				case Celestial.RangeStates.InRange:
					View.Confirm = language.Confirm.Value;
					View.ConfirmDescription = language.ConfirmDescription.Value;
					break;
				case Celestial.RangeStates.OutOfRange:
					View.Confirm = language.OutOfRange.Value;
					View.ConfirmDescription = language.OutOfRangeDescription.Value;
					break;
			}

			View.SetStates(
				highlightState,
				visitState,
				rangeState,
				selectedState,
				travelState,
				instant
			);
		}

		Celestial.RangeStates GetRangeState(SystemModel system)
		{
			if (system == null) throw new ArgumentNullException("system");

			var result = Celestial.RangeStates.OutOfRange;

			if (UniversePosition.Distance(system.Position.Value, Model.Ship.Value.Position.Value) <= Model.Ship.Value.Range.Value.Total)
			{
				result = Celestial.RangeStates.InRange;
			}
			return result;
		}

		#region Events
		void OnActiveSystem(SystemModel activeSystem)
		{
			if (activeSystem == null)
			{
				if (View.Visible) CloseViewInstant();
				return;
			}

			ShowViewInstant(View.Visible);
		}

		void OnGlobalClick(Click click)
		{
			if (!click.ClickedNothing) return;
			if (Model.TransitState.Value.State != TransitState.States.Complete) return;
			if (App.V.CameraHasMoved) return;
			if (!Mathf.Approximately(1f, ScaleModel.Opacity.Value)) return;

			switch(selectedState)
			{
				case Celestial.SelectedStates.Selected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.UnSelect(positionInUniverse, instanceModel.ActiveSystem.Value);
					break;
			}
		}

		protected override void OnShowView()
		{
			var activeSystem = instanceModel.ActiveSystem.Value;

			positionInUniverse = activeSystem.Position.Value;

			//SetGrid(ScaleModel.Transform.Value.UnityOrigin, ScaleModel.Transform.Value.UnityRadius);

			View.Enter = OnEnter;
			View.Exit = OnExit;
			View.Click = OnClick;

			highlightState = Celestial.HighlightStates.Idle;

			if (activeSystem.Position.Value.Equals(Model.Ship.Value.Position.Value)) visitState = Celestial.VisitStates.Current;
			else visitState = activeSystem.Visited.Value ? Celestial.VisitStates.Visited : Celestial.VisitStates.NotVisited;

			rangeState = GetRangeState(activeSystem);

			switch (Model.CelestialSystemStateLastSelected.Value.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					selectedState = Model.CelestialSystemStateLastSelected.Value.Position.Equals(activeSystem.Position.Value) ? Celestial.SelectedStates.Selected : Celestial.SelectedStates.OtherSelected;
					break;
				default:
					selectedState = Celestial.SelectedStates.NotSelected;
					break;
			}

			travelState = Celestial.TravelStates.NotTraveling;

			View.DetailsName = activeSystem.Name.Value;
			View.DetailsDescription = language.PrimaryClassifications[activeSystem.PrimaryClassification.Value].Value.Value + " - " + activeSystem.SecondaryClassification.Value;

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
			Model.CelestialSystemState.Value = CelestialSystemStateBlock.Highlight(positionInUniverse, instanceModel.ActiveSystem.Value);
		}

		void OnExit()
		{
			switch (selectedState)
			{
				case Celestial.SelectedStates.NotSelected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle(Model.Ship.Value.Position, null);
					break;
				case Celestial.SelectedStates.Selected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle(positionInUniverse, instanceModel.ActiveSystem.Value);
					break;
				case Celestial.SelectedStates.OtherSelected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle(Model.CelestialSystemStateLastSelected.Value.Position, Model.CelestialSystemStateLastSelected.Value.System);
					break;
			}

		}

		void OnClick()
		{
			switch (rangeState)
			{
				case Celestial.RangeStates.InRange:
					switch (selectedState)
					{
						case Celestial.SelectedStates.OtherSelected:
						case Celestial.SelectedStates.NotSelected:
							Model.CelestialSystemState.Value = CelestialSystemStateBlock.Select(positionInUniverse, instanceModel.ActiveSystem.Value);
							break;
						case Celestial.SelectedStates.Selected:
							Model.TransitState.Value = TransitState.Request(false, Model.Ship.Value.CurrentSystem.Value, instanceModel.ActiveSystem.Value);
							break;
					}
					break;
			}
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

		void OnShipPosition(UniversePosition shipPosition)
		{
			if (!instanceModel.HasSystem || !View.Visible) return;

			var wasRangeState = rangeState;
			rangeState = GetRangeState(instanceModel.ActiveSystem.Value);
			if (wasRangeState != rangeState) ApplyStates(true);
		}
		#endregion
	}
}