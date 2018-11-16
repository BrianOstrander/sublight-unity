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

		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public CelestialSystemPresenter(
			GameModel model,
			UniverseScales scale,
			UniversePosition positionInUniverse // Temp...
		) : base(model, scale)
		{
			this.positionInUniverse = positionInUniverse;

			App.Callbacks.Click += OnGlobalClick;

			Model.CelestialSystemState.Changed += OnCelestialSystemState;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.Click -= OnGlobalClick;

			Model.CelestialSystemState.Changed -= OnCelestialSystemState;

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
		void OnGlobalClick(Click click)
		{
			if (!click.ClickedNothing) return;

			switch(selectedState)
			{
				case Celestial.SelectedStates.Selected:
					Model.CelestialSystemState.Value = CelestialSystemStateBlock.UnSelect(positionInUniverse);
					break;
			}
		}

		protected override void OnShowView()
		{
			View.WorldOrigin = ScaleModel.Transform.Value.UnityOrigin;
			View.WorldRadius = ScaleModel.Transform.Value.UnityRadius;

			View.Enter = OnEnter;
			View.Exit = OnExit;
			View.Click = OnClick;

			highlightState = Celestial.HighlightStates.Idle;
			selectedState = Celestial.SelectedStates.NotSelected;

			// TODO: derive these values from a model...
			visitState = Model.Ship.Value.Position.Value.Equals(positionInUniverse) ? Celestial.VisitStates.Current : Celestial.VisitStates.NotVisited;
			rangeState = Celestial.RangeStates.InRange;
			travelState = Celestial.TravelStates.NotTraveling;

			ApplyStates(true);

		}

		void OnEnter()
		{
			Model.CelestialSystemState.Value = CelestialSystemStateBlock.Highlight(positionInUniverse);
		}

		void OnExit()
		{
			Model.CelestialSystemState.Value = CelestialSystemStateBlock.Idle;
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
		#endregion
	}
}