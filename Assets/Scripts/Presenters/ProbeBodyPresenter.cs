using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ProbeBodyPresenter : Presenter<IProbeBodyView>
	{
		GameModel model;
		bool hasPoppedEscape;

		public ProbeBodyPresenter(GameModel model)
		{
			this.model = model;
		}

		protected override void UnBind()
		{
			base.UnBind();
		}

		public void Show()
		{
			if (View.Visible) return;

			App.Callbacks.ShadeRequest(ShadeRequest.Shade);
			App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.Obscure);
			hasPoppedEscape = false;

			View.Reset();

			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(OnEscape, false, false));

			ShowView(App.OverlayCanvasRoot);
		}

		#region Events
		void OnEscape()
		{
			hasPoppedEscape = true;
			OnClose();
		}

		void OnBackClick()
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
			// TODO: Remove this?
		}
		#endregion

	}
}