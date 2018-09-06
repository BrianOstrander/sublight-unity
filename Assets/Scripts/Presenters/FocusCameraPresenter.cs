using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public abstract class FocusCameraPresenter<V, D> : Presenter<V>
		where V : class, IFocusCameraView
		where D : SetFocusDetails<D>, new()
	{
		protected abstract SetFocusLayers FocusLayer { get; }
		protected virtual bool IsGatherable { get { return true; } }
		protected Transform ViewParent;

		RenderTexture renderTexture;

		public FocusCameraPresenter(Transform viewParent, string overrideName = null)
		{
			ViewParent = viewParent;
			if (!string.IsNullOrEmpty(overrideName)) View.InstanceName = overrideName;

			App.Focus.RegisterLayer(FocusLayer);
			App.Callbacks.GatherFocusRequest += OnGatherFocusRequest;
			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		protected override void OnUnBind()
		{
			App.Focus.UnRegisterLayer(FocusLayer);
			App.Callbacks.GatherFocusRequest -= OnGatherFocusRequest;
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
		}

		void TransitionActive(TransitionFocusRequest request, SetFocusTransition transition)
		{
			Debug.Log(GetType().Name + ": From " + transition.Start.Enabled + " to " + transition.End.Enabled);
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

			View.Texture = IsGatherable ? renderTexture : null;

			OnShowInstant();

			ShowView(ViewParent, true);
		}
		
		void CloseInstant()
		{
			renderTexture = null;
			View.Texture = null;
			
			OnCloseInstant();

			CloseView(true);
		}

		#region Events
		void OnGatherFocusRequest(GatherFocusRequest request)
		{
			DeliverFocusBlock gather;
			if (!request.GetGather(FocusLayer, out gather)) return;

			if (!IsGatherable)
			{
				gather.Done(gather.Duplicate(Texture2D.blackTexture));
				return;
			}
			gather.Done(gather.Duplicate(renderTexture = new RenderTexture(Screen.width, Screen.height, 16)));
		}

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

		protected virtual void OnShowInstant() {}
		protected virtual void OnCloseInstant() {}

		protected virtual void OnTransitionActive(
			TransitionFocusRequest request,
			SetFocusTransition transition,
			D startDetails,
			D endDetails
		) {}
		#endregion
	}
}