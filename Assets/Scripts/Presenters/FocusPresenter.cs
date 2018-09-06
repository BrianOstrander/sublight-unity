using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public abstract class FocusPresenter<V, D> : Presenter<V>
		where V : class, IView
		where D : SetFocusDetails<D>, new()
	{
		protected abstract SetFocusLayers FocusLayer { get; }
		protected Transform ViewParent;

		public FocusPresenter() : this(null) {}

		public FocusPresenter(Transform viewParent, string overrideName = null)
		{
			ViewParent = viewParent;
			if (!string.IsNullOrEmpty(overrideName)) View.InstanceName = overrideName;

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
				if (View.TransitionState == TransitionStates.Closed) ShowInstant();
			}
			else if (request.LastActive)
			{
				if (View.TransitionState == TransitionStates.Shown) CloseInstant();
			}

			if (transition.Start.Layer != transition.End.Layer)
			{
				Debug.LogError("Mismatch between details, cannot begin with " + transition.Start.Layer + " and end with " + transition.End.Layer);
				return;
			}
			if (transition.Start.Layer != FocusLayer)
			{
				Debug.LogError("Mismatch between start and end details of layer " + transition.Start.Layer + ", and this presenter's layer, " + FocusLayer);
				return;
			}

			OnTransitionActive(request, transition, transition.Start.Details as D, transition.End.Details as D);
		}

		void ShowInstant()
		{
			View.Reset();

			OnShowInstant();

			ShowView(ViewParent, true);
		}

		void CloseInstant()
		{
			OnCloseInstant();

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
		protected virtual void OnShowInstant() { }
		protected virtual void OnCloseInstant() { }

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