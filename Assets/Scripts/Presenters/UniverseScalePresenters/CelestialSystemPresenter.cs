﻿using System;
using System.Linq;

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
			App.Callbacks.EncounterRequest += OnEncounterRequest;

			Model.Context.CelestialSystemState.Changed += OnCelestialSystemState;
			Model.Context.CameraTransformAbsolute.Changed += OnCameraTransform;
			Model.Context.GridInput.Changed += OnGridInput;
			Model.Context.FocusTransform.Changed += OnFocusTransform;
			Model.Ship.Position.Changed += OnShipPosition;
			Model.Ship.Statistics.Changed += OnShipStatistics;
			Model.Context.CurrentSystem.Changed += OnShipCurrentSystem;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			instanceModel.ActiveSystem.Changed -= OnActiveSystem;

			App.Callbacks.Click -= OnGlobalClick;
			App.Callbacks.EncounterRequest -= OnEncounterRequest;

			Model.Context.CelestialSystemState.Changed -= OnCelestialSystemState;
			Model.Context.CameraTransformAbsolute.Changed -= OnCameraTransform;
			Model.Context.GridInput.Changed -= OnGridInput;
			Model.Context.FocusTransform.Changed -= OnFocusTransform;
			Model.Ship.Position.Changed -= OnShipPosition;
			Model.Ship.Statistics.Changed -= OnShipStatistics;
			Model.Context.CurrentSystem.Changed -= OnShipCurrentSystem;
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

			if (Model.Context.CurrentSystem.Value == system) visitState = Celestial.VisitStates.Current;
			else visitState = system.Visited.Value ? Celestial.VisitStates.Visited : Celestial.VisitStates.NotVisited;

			var range = Model.Ship.Statistics.Value.TransitRange;

			if (UniversePosition.Distance(system.Position.Value, Model.Ship.Position.Value) <= range)
			{
				rangeState = Celestial.RangeStates.InRange;
			}
			else rangeState = Celestial.RangeStates.OutOfRange;

			switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					selectedState = Model.Context.CelestialSystemStateLastSelected.Value.Position.Equals(system.Position.Value) ? Celestial.SelectedStates.Selected : Celestial.SelectedStates.OtherSelected;
					break;
				default:
					selectedState = Celestial.SelectedStates.NotSelected;
					break;
			}

			switch (Model.Context.TransitState.Value.State)
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

		void CheckForAnyUpdates()
		{
			if (!instanceModel.HasSystem || !View.Visible) return;

			bool anyChange;
			UpdateStates(instanceModel.ActiveSystem.Value, out anyChange);

			if (anyChange) ApplyStates(true);
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
			if (Model.Context.TransitState.Value.State != TransitState.States.Complete) return;
			if (App.V.CameraHasMoved) return;
			if (!Mathf.Approximately(1f, ScaleModel.Opacity.Value)) return;

			switch(selectedState)
			{
				case Celestial.SelectedStates.Selected:
					Model.Context.SetCelestialSystemState(CelestialSystemStateBlock.UnSelect(positionInUniverse, instanceModel.ActiveSystem.Value));
					break;
			}
		}

		void OnEncounterRequest(EncounterRequest request)
		{
			if (instanceModel.ActiveSystem.Value == null ||
				!(
					Model.Ship.Position.Value.SectorEquals(instanceModel.ActiveSystem.Value.Position) &&
					Model.Ship.SystemIndex.Value == instanceModel.ActiveSystem.Value.Index
			   	)
		   	)
			{
				return;
			}

			switch (request.State)
			{
				case EncounterRequest.States.Handle:
					request.TryHandle<EncounterEventHandlerModel>(OnHandleEvent);
					break;
			}
		}

		public void OnHandleEvent(EncounterEventHandlerModel handler)
		{
			// They should already be filtered at this point...
			var refreshSystem = handler.Events.Value.FirstOrDefault(e => e.EncounterEvent.Value == EncounterEvents.Types.RefreshSystem);
			if (refreshSystem == null) return;

			RefreshFromKeyValues();
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

			var lightyearDistance = UniversePosition.ToLightYearDistance(UniversePosition.Distance(Model.Ship.Position.Value, activeSystem.Position.Value));
			var lightyearText = lightyearDistance < 10f ? lightyearDistance.ToString("N1") : Mathf.RoundToInt(lightyearDistance).ToString("N0");

			View.Distance = lightyearText;
			View.DistanceUnit = language.DistanceUnit.Value;

			RefreshFromKeyValues();

			ApplyStates(true);
		}

		void RefreshFromKeyValues()
		{
			var activeSystem = instanceModel.ActiveSystem.Value;

			View.DetailsName = activeSystem.Name.Value;
			View.DetailsDescription = language.PrimaryClassifications[activeSystem.PrimaryClassification.Value].Value.Value + " - " + activeSystem.SecondaryClassification.Value;

			View.IconColor = activeSystem.IconColor.Value;
			View.IconScale = activeSystem.IconScale.Value;

			View.IconPings(activeSystem.KeyValues.Get(KeyDefines.CelestialSystem.IconPings));
		}

		void OnEnter()
		{
			Model.Context.SetCelestialSystemState(CelestialSystemStateBlock.Highlight(positionInUniverse, instanceModel.ActiveSystem.Value));
		}

		void OnExit()
		{
			switch (selectedState)
			{
				case Celestial.SelectedStates.NotSelected:
					Model.Context.SetCelestialSystemState(CelestialSystemStateBlock.Idle(Model.Ship.Position, null));
					break;
				case Celestial.SelectedStates.Selected:
					Model.Context.SetCelestialSystemState(CelestialSystemStateBlock.Idle(positionInUniverse, instanceModel.ActiveSystem.Value));
					break;
				case Celestial.SelectedStates.OtherSelected:
					Model.Context.SetCelestialSystemState(CelestialSystemStateBlock.Idle(Model.Context.CelestialSystemStateLastSelected.Value.Position, Model.Context.CelestialSystemStateLastSelected.Value.System));
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
							Model.Context.SetCelestialSystemState(CelestialSystemStateBlock.Select(positionInUniverse, instanceModel.ActiveSystem.Value));
							break;
						case Celestial.SelectedStates.Selected:
							switch (visitState)
							{
								case Celestial.VisitStates.Current: break;
								default:
									Model.Context.TransitStateRequest.Value = TransitStateRequest.Create(Model.Context.CurrentSystem.Value, instanceModel.ActiveSystem.Value);
									break;
							}
							break;
					}
					break;
				case Celestial.RangeStates.OutOfRange:
					Model.Context.NavigationSelectionOutOfRange(instanceModel.ActiveSystem.Value);
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
			ProcessInterectable(transform, Model.Context.GridInput.Value);
		}

		void OnGridInput(GridInputRequest gridInput)
		{
			if (!View.Visible) return;
			ProcessInterectable(Model.Context.CameraTransformAbsolute.Value, gridInput);
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
			CheckForAnyUpdates();
		}

		void OnShipStatistics(ShipStatistics shipStatistics)
		{
			CheckForAnyUpdates();
		}

		void OnShipCurrentSystem(SystemModel system)
		{
			CheckForAnyUpdates();
		}
		#endregion
	}
}