using System;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipCameraPresenter : Presenter<IShipCameraView>
	{
		bool isDragging;

		public ShipCameraPresenter()
		{
			App.Callbacks.StateChange += OnStateChange;
			App.Callbacks.BeginGesture += OnBeginGesture;
			App.Callbacks.CurrentGesture += OnCurrentGesture;
			App.Callbacks.EndGesture += OnEndGesture;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			App.Callbacks.BeginGesture -= OnBeginGesture;
			App.Callbacks.CurrentGesture -= OnCurrentGesture;
			App.Callbacks.EndGesture -= OnEndGesture;
		}

		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnBeginGesture(Gesture gesture)
		{
			isDragging = true;
		}

		void OnCurrentGesture(Gesture gesture)
		{
			if (!isDragging) return;
			App.Log(gesture.Delta);
		}

		void OnEndGesture(Gesture gesture)
		{
			isDragging = false;
		}
		#endregion
	}
}