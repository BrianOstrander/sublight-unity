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
			Model.Ship.Value.CurrentSystem.Changed += OnShipCurrentSystem;
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
			Model.Ship.Value.CurrentSystem.Changed -= OnShipCurrentSystem;
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

		void UpdateStates(SystemModel system)
		{
			if (system == null) throw new ArgumentNullException("system");

			if (Model.Ship.Value.CurrentSystem.Value == system) visitState = Celestial.VisitStates.Current;
			else visitState = system.Visited.Value ? Celestial.VisitStates.Visited : Celestial.VisitStates.NotVisited;

			if (UniversePosition.Distance(system.Position.Value, Model.Ship.Value.Position.Value) <= Model.Ship.Value.Range.Value.Total)
			{
				rangeState = Celestial.RangeStates.InRange;
			}
			else rangeState = Celestial.RangeStates.OutOfRange;

			switch (Model.CelestialSystemStateLastSelected.Value.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					selectedState = Model.CelestialSystemStateLastSelected.Value.Position.Equals(system.Position.Value) ? Celestial.SelectedStates.Selected : Celestial.SelectedStates.OtherSelected;
					break;
				default:
					selectedState = Celestial.SelectedStates.NotSelected;
					break;
			}

			switch (Model.TransitState.Value.State)
			{
				case TransitState.States.Complete:
					travelState = Celestial.TravelStates.NotTraveling;
					break;
				default:
					highlightState = Celestial.HighlightStates.Idle;
					travelState = Celestial.TravelStates.Traveling;
					break;
			}
		}

		void UpdateStates(
			SystemModel system,
			out bool highlightChanged,
			out bool visitChanged,
			out bool rangeChanged,
			out bool selectedChanged,
			out bool travelChanged
		)
		{
			var oldHighlightState = highlightState;
			var oldVisitState = visitState;
			var oldRangeState = rangeState;
			var oldSelectedState = selectedState;
			var oldTravelState = travelState;

			UpdateStates(system);

			highlightChanged = highlightState != oldHighlightState;
			visitChanged = visitState != oldVisitState;
			rangeChanged = rangeState != oldRangeState;
			selectedChanged = selectedState != oldSelectedState;
			travelChanged = travelState != oldTravelState;
		}

		void UpdateStates(SystemModel system, out bool anyChanges)
		{
			bool highlightChanged;
			bool visitChanged;
			bool rangeChanged;
			bool selectedChanged;
			bool travelChanged;

			UpdateStates(
				system,
				out highlightChanged,
				out visitChanged,
				out rangeChanged,
				out selectedChanged,
				out travelChanged
			);

			anyChanges = highlightChanged || visitChanged || rangeChanged || selectedChanged || travelChanged;
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

			UpdateStates(activeSystem);

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
							switch (visitState)
							{
								case Celestial.VisitStates.Current: break;
								default:
									Model.TransitStateRequest.Value = TransitStateRequest.Create(Model.Ship.Value.CurrentSystem.Value, instanceModel.ActiveSystem.Value);
									break;
							}
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

			bool anyChange;
			UpdateStates(instanceModel.ActiveSystem.Value, out anyChange);

			if (anyChange) ApplyStates(true);
		}

		void OnShipCurrentSystem(SystemModel system)
		{
			if (!instanceModel.HasSystem || !View.Visible) return;

			bool anyChange;
			UpdateStates(instanceModel.ActiveSystem.Value, out anyChange);

			if (anyChange) ApplyStates(true);
		}
		#endregion
	}
}