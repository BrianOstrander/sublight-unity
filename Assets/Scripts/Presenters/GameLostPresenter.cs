using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class GameLostPresenter : Presenter<IGameLostView>
	{
		GameModel model;
		bool hasPoppedEscape;

		public GameLostPresenter(GameModel model)
		{
			this.model = model;

			model.Ship.Value.Inventory.AllResources.Rations.Changed += OnRations;
			model.DestructionRadius.Changed += OnDestructionRadius;
		}

		protected override void OnUnBind()
		{
			model.Ship.Value.Inventory.AllResources.Rations.Changed -= OnRations;
			model.DestructionRadius.Changed -= OnDestructionRadius;
		}

		void Show(string reason)
		{
			if (View.Visible) return;

			App.Callbacks.ShadeRequest(ShadeRequest.Shade);
			App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.Obscure);
			hasPoppedEscape = false;

			View.Reset();

			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(OnEscape));

			View.Reason = reason;
			View.MainMenuClick = OnMainMenuClick;
			ShowView(App.OverlayCanvasRoot, true);
		}

		#region Events
		void OnEscape()
		{
			hasPoppedEscape = true;
			OnClose();
		}

		void OnRations(float rations)
		{
			if (!Mathf.Approximately(0f, rations)) return;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			Show(Strings.OutOfRations);
		}

		void OnDestructionRadius(float radius)
		{
			if (radius < UniversePosition.Distance(UniversePosition.Zero, model.Ship.Value.Position)) return;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			Show(Strings.DestroyedByVoid);
		}

		void OnMainMenuClick()
		{
			switch(View.TransitionState)
			{
				case TransitionStates.Shown:
					OnClose();
					break;
			}
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
			var payload = new HomePayload();
			App.SM.RequestState(payload);
		}
		#endregion

	}
}