using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class FocusLipPresenter : Presenter<ILipView>, IPresenterCloseShowOptions
	{
		enum RevealStates
		{
			Unknown = 0,
			Revealing = 10,
			Hiding = 20,
			Switching = 30
		}

		RevealStates revealing;
		bool hasSetLayer;
		SetFocusLayers currentLayer;
		int currentOrder;
		SetFocusLayers[] layers = new SetFocusLayers[0];

		public FocusLipPresenter(params SetFocusLayers[] layers)
		{
			this.layers = layers;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.HoloColor = App.Callbacks.LastHoloColorRequest.Color;

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}

		#region Events
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			switch (request.State)
			{
				case TransitionFocusRequest.States.Request:
					hasSetLayer = false;
					SetFocusLayers closestLayer = SetFocusLayers.Unknown;
					var order = int.MaxValue;
					foreach (var transition in request.Transitions)
					{
						if (layers.Contains(transition.Layer) && transition.End.Enabled)
						{
							if (transition.End.Enabled && transition.End.Order < order) closestLayer = transition.Layer;
						}
					}
					if (currentLayer == closestLayer)
					{
						revealing = RevealStates.Unknown;
						return;
					}

					var wasCurrent = currentLayer;
					currentLayer = closestLayer;

					if (wasCurrent == SetFocusLayers.Unknown)
					{
						// We're revealing.
						revealing = RevealStates.Revealing;
						break;
					}
					if (closestLayer == SetFocusLayers.Unknown)
					{
						// We're hiding.
						revealing = RevealStates.Hiding;
						break;
					}
					// We're going from one show to another...
					revealing = RevealStates.Switching;
					break;
				case TransitionFocusRequest.States.Active:
					if (revealing != RevealStates.Unknown) OnTransitionActive(request);
					break;
				default:
					return;
			}

			if (request.State == TransitionFocusRequest.States.Request && revealing == RevealStates.Revealing && !View.Visible)
			{
				View.Reset();

				View.HoloColor = App.Callbacks.LastHoloColorRequest.Color;

				View.SetLayer(LayerConstants.Get(currentLayer));
				hasSetLayer = true;

				ShowView(instant: true);

				View.SetLips(1f, false);
			}
		}

		void OnTransitionActive(TransitionFocusRequest request)
		{
			var firstStage = request.Progress <= 0.5f;
			var remappedProgress = firstStage ? request.Progress / 0.5f : (request.Progress - 0.5f) / 0.5f;

			switch (revealing)
			{
				case RevealStates.Revealing:
					if (firstStage) break;
					View.SetLips(remappedProgress, true);
					break;
				case RevealStates.Hiding:
					if (!firstStage) break;
					View.SetLips(remappedProgress, false);
					break;
				case RevealStates.Switching:
					if (firstStage) View.SetLips(remappedProgress, false);
					else
					{
						if (!hasSetLayer)
						{
							hasSetLayer = true;
							View.SetLayer(LayerConstants.Get(currentLayer));
						}
						View.SetLips(remappedProgress, true);
					}
					break;
			}

			if (revealing == RevealStates.Hiding && Mathf.Approximately(1f, request.Progress)) CloseView(true);
		}
		#endregion
	}
}