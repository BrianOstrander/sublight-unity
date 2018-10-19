using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ToolbarPresenter : Presenter<IToolbarView>, IPresenterCloseShowOptions
	{
		TransitionFocusRequest lastTransition;

		bool CanTransition { get { return lastTransition.State == TransitionFocusRequest.States.Complete; } }

		public ToolbarPresenter()
		{
			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.Selection = 0;
			View.Buttons = new ToolbarButtonBlock[] 
			{
				new ToolbarButtonBlock(LanguageStringModel.Override("Navigation"), View.GetIcon(SetFocusLayers.System), () => OnTransitionClick(SetFocusLayers.System)),
				new ToolbarButtonBlock(LanguageStringModel.Override("Logistics"), View.GetIcon(SetFocusLayers.Ship), () => OnTransitionClick(SetFocusLayers.Ship)),
				new ToolbarButtonBlock(LanguageStringModel.Override("Communications"), View.GetIcon(SetFocusLayers.Communications), () => OnTransitionClick(SetFocusLayers.Communications)),
				new ToolbarButtonBlock(LanguageStringModel.Override("Encyclopedia"), View.GetIcon(SetFocusLayers.Encyclopedia), () => OnTransitionClick(SetFocusLayers.Encyclopedia))
			};

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}

		#region Events
		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			lastTransition = request;

			View.Interactable = CanTransition;
		}

		void OnTransitionClick(SetFocusLayers layer)
		{
			if (!CanTransition) return;

			SetFocusTransition transition;
			if (lastTransition.GetTransition(layer, out transition))
			{
				Debug.Log("Layer " + layer + " is " + transition.End.Enabled);
			}
		}
		#endregion
	}
}