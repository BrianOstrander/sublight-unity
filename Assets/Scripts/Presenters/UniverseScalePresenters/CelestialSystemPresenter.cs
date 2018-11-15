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

			Model.CelestialSystemState.Changed += OnCelestialSystemState;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

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
			visitState = Celestial.VisitStates.Visited;
			ApplyStates();
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.Idle:
					highlightState = Celestial.HighlightStates.Idle;
					selectedState = Celestial.SelectedStates.NotSelected;
					ApplyStates();
					break;
				case CelestialSystemStateBlock.States.Highlighted:
					var isCurrent = block.Position.Equals(positionInUniverse);
					highlightState = isCurrent ? Celestial.HighlightStates.Highlighted : Celestial.HighlightStates.OtherHighlighted;
					ApplyStates();
					break;
			}
			//if ()
			//{

			//}
		}
		#endregion
	}
}