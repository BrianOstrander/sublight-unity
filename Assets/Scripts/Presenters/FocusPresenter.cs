using UnityEngine;

namespace LunraGames.SubLight.Presenters
{
	public abstract class FocusPresenter<V, D> : Presenter<V>
		where V : class, IView
		where D : SetFocusDetails<D>, new()
	{
		SetFocusLayers focusLayer;
		protected SetFocusLayers FocusLayer
		{
			get { return focusLayer == SetFocusLayers.Unknown ? (focusLayer = SetFocusDetailsBase.GetLayer<D>()) : focusLayer; }
		}

		protected Transform ViewParent;

		public FocusPresenter() : this(null) {}

		public FocusPresenter(Transform viewParent, string overrideName = null, SetFocusLayers layer = SetFocusLayers.Unknown)
		{
			ViewParent = viewParent;
			if (!string.IsNullOrEmpty(overrideName)) View.InstanceName = overrideName;
			if (layer != SetFocusLayers.Unknown) View.SetLayer(LayerConstants.Get(layer));

			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
		}

		void TransitionActive(TransitionFocusRequest request, SetFocusTransition transition)
		{
			if (transition.End.Enabled)
			{
				if (request.FirstActive)
				{
					// We check if they're the same, because if we're showing the priority layer we don't want to run logic for already visible stuff...
					if (transition.Start.Enabled != transition.End.Enabled)
					{
						if (View.TransitionState == TransitionStates.Closed) ShowInstant();
						else OnUpdateEnabled();
					}
				}
			}
			else if (request.LastActive)
			{
				if (View.TransitionState == TransitionStates.Shown) CloseInstant();
				else OnUpdateDisabled();
			}

			if (transition.Start.Layer != transition.End.Layer)
			{
				Debug.LogError("Mismatch between details, cannot begin with " + transition.Start.Layer + " and end with " + transition.End.Layer, View.gameObject);
				return;
			}
			if (transition.Start.Layer != FocusLayer)
			{
				Debug.LogError("Mismatch between start and end details of layer " + transition.Start.Layer + ", and this presenter's layer, " + FocusLayer, View.gameObject);
				return;
			}

			OnTransitionActive(request, transition, transition.Start.Details as D, transition.End.Details as D);
		}

		void ShowInstant()
		{
			if (!CanShow()) return;

			if (CanReset()) View.Reset();

			OnUpdateEnabled();

			ShowView(ViewParent, true);
		}

		void CloseInstant()
		{
			if (!CanClose()) return;

			OnUpdateDisabled();

			CloseView(true);
		}

		#region Events
		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			switch (request.State)
			{
				case TransitionFocusRequest.States.Active:
					break;
				default:
					return;
			}
			SetFocusTransition transition;
			if (request.GetTransition(FocusLayer, out transition)) TransitionActive(request, transition);
		}
		#endregion

		#region Overridable Events
		protected virtual bool CanReset() { return true; }
		protected virtual bool CanShow() { return true; }
		protected virtual bool CanClose() { return true; }

		protected virtual void OnUpdateEnabled() { }
		protected virtual void OnUpdateDisabled() { }

		protected virtual void OnTransitionActive(
			TransitionFocusRequest request,
			SetFocusTransition transition,
			D startDetails,
			D endDetails
		)
		{ }
		#endregion
	}
}