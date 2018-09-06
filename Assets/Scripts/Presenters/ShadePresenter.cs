using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ShadePresenter : Presenter<IShadeView>
	{
		ShadeRequest lastShade;

		public ShadePresenter()
		{
			App.Callbacks.ShadeRequest += OnShadeRequest;
			App.Heartbeat.LateUpdate += OnLateUpdate;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.ShadeRequest -= OnShadeRequest;
			App.Heartbeat.LateUpdate -= OnLateUpdate;
		}

		void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Click = OnClick;

			ShowView(App.OverlayCanvasRoot, true);
		}

		#region Events
		void OnShadeRequest(ShadeRequest request)
		{
			switch(request.State)
			{
				case ShadeRequest.States.Request:
					lastShade = request;
					break;
			}
		}

		void OnLateUpdate(float delta)
		{
			if (lastShade.State != ShadeRequest.States.Request) return;
			if (lastShade.IsShaded)
			{
				switch(View.TransitionState)
				{
					case TransitionStates.Closed:
						Show();
						break;
				}
			}
			else
			{
				switch (View.TransitionState)
				{
					case TransitionStates.Shown:
						CloseView();
						break;
				}
			}

			App.Callbacks.ShadeRequest(lastShade = lastShade.Duplicate(ShadeRequest.States.Complete));
		}

		void OnClick()
		{
			// TODO: Is this needed?
		}
		#endregion

	}
}