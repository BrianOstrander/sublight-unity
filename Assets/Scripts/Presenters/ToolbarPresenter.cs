using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ToolbarPresenter : Presenter<IToolbarView>, IPresenterCloseShowOptions
	{
		GameModel model;
		TransitionFocusRequest lastTransition;
		ToolbarSelections lastSelection;

		bool CanTransition { get { return lastTransition.State == TransitionFocusRequest.States.Complete; } }

		public ToolbarPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
			model.ToolbarSelection.Changed += OnToolbarSelection;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
			model.ToolbarSelection.Changed -= OnToolbarSelection;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			OnToolbarSelection(model.ToolbarSelection.Value);

			View.Buttons = new ToolbarButtonBlock[] 
			{
				new ToolbarButtonBlock(LanguageStringModel.Override("Navigation"), View.GetIcon(SetFocusLayers.System), () => OnTransitionClick(ToolbarSelections.System)),
				new ToolbarButtonBlock(LanguageStringModel.Override("Logistics"), View.GetIcon(SetFocusLayers.Ship), () => OnTransitionClick(ToolbarSelections.Ship)),
				new ToolbarButtonBlock(LanguageStringModel.Override("Communications"), View.GetIcon(SetFocusLayers.Communication), () => OnTransitionClick(ToolbarSelections.Communication)),
				new ToolbarButtonBlock(LanguageStringModel.Override("Encyclopedia"), View.GetIcon(SetFocusLayers.Encyclopedia), () => OnTransitionClick(ToolbarSelections.Encyclopedia))
			};

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);

			lastSelection = ToolbarSelections.Unknown;
		}

		#region Events
		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			lastTransition = request;
		}

		void OnToolbarSelection(ToolbarSelections selection)
		{
			if (lastSelection == selection) return;

			switch (selection)
			{
				case ToolbarSelections.System: View.Selection = 0; break;
				case ToolbarSelections.Ship: View.Selection = 1; break;
				case ToolbarSelections.Communication: View.Selection = 2; break;
				case ToolbarSelections.Encyclopedia: View.Selection = 3; break;
				default:
					Debug.LogError("Unrecognized selection: " + model.ToolbarSelection.Value);
					break;
			}
			lastSelection = selection;
		}

		void OnTransitionClick(ToolbarSelections selection)
		{
			if (!CanTransition || selection == model.ToolbarSelection.Value) return;

			OnToolbarSelection(selection);
			model.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(selection);
		}
		#endregion
	}
}