using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class EnterSystemPresenter : Presenter<IEnterSystemView>
	{
		GameModel model;
		bool hasPoppedEscape;
		SystemModel destination;

		public EnterSystemPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.TravelRequest += OnTravelRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.TravelRequest -= OnTravelRequest;
		}

		void Show()
		{
			if (View.Visible) return;

			App.Callbacks.ShadeRequest(ShadeRequest.Shade);
			App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.Obscure);
			hasPoppedEscape = false;

			View.Reset();

			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(OnEscape, false, false));

			View.Title = Strings.ArrivedIn(destination.Name.Value);
			View.Details = Strings.ArrivedDetails(destination.Rations.Value);
			View.OkayClick = OnOkayClick;
			ShowView(App.OverlayCanvasRoot);
		}

		#region Events
		void OnTravelRequest(TravelRequest travelRequest)
		{
			switch(travelRequest.State)
			{
				case TravelRequest.States.Complete:
					// Don't pop up on end system.
					if (travelRequest.Destination == model.EndSystem.Value) return;

					var travelDestination = model.Universe.Value.GetSystem(travelRequest.Destination);
					if (travelDestination == null) Debug.LogError("Travel destination null, may cause strange side effects.");
					else if (!travelDestination.Visited)
					{
						destination = travelDestination;
						Show();
					}
					break;
			}
		}

		void OnEscape()
		{
			hasPoppedEscape = true;
			OnClose();
		}

		void OnOkayClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose();
		}

		void OnClose()
		{
			if (!hasPoppedEscape)
			{
				App.Callbacks.PopEscape();
				App.Callbacks.ShadeRequest(ShadeRequest.UnShade);
				App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.UnObscure);
			}
			View.Closed += OnClosed;
			CloseView();
		}

		void OnClosed()
		{
			destination.Visited.Value = true;
			model.Ship.Value.Rations.Value += destination.Rations;
		}
		#endregion

	}
}