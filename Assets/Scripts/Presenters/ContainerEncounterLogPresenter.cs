using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class ContainerEncounterLogPresenter : Presenter<IContainerEncounterLogView>
	{
		GameModel model;

		List<IEntryEncounterLogPresenter> entries = new List<IEntryEncounterLogPresenter>();

		public ContainerEncounterLogPresenter(GameModel model)
		{
			this.model = model;

			//App.Callbacks.FocusRequest += OnFocus;
			App.Callbacks.EncounterRequest += OnEncounter;
		}

		protected override void OnUnBind()
		{
			//App.Callbacks.FocusRequest -= OnFocus;
			App.Callbacks.EncounterRequest -= OnEncounter;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = "Encounter";
			View.DoneClick = OnDoneClick;
			View.NextClick = OnNextClick;

			View.PrepareClose += OnPrepareClose;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		/* TODO: Support the new focus system.
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Encounter:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var encounterFocus = focus as EncounterFocusRequest;
					entries.Clear();

					Show();
					break;
				default:
					if (focus.State == FocusRequest.States.Active)
					{
						if (View.TransitionState == TransitionStates.Shown) CloseView();
					}
					break;
			}
		}
		*/

		void OnPrepareClose()
		{
			// TODO: Make this nicer and not need to close instantly???
			foreach (var entry in entries)
			{
				entry.Close();
				App.P.UnRegister(entry);
			}
			entries.Clear();
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			CloseView();
			App.Callbacks.EncounterRequest(EncounterRequest.PrepareComplete());
		}

		void OnNextClick()
		{
			App.Callbacks.EncounterRequest(EncounterRequest.Next());
		}

		void OnEncounter(EncounterRequest request)
		{
			switch (request.State)
			{
				case EncounterRequest.States.Handle:
					if (request.TryHandle<ButtonHandlerModel>(OnButtonHandler)) break;
					if (request.TryHandle<TextHandlerModel>(OnTextHandler)) break;
					Debug.LogError("Unrecognized request model type: " + request.ModelType.FullName);
					break;
				case EncounterRequest.States.Controls:
					View.NextEnabled = request.NextControl;
					View.DoneEnabled = request.PrepareCompleteControl;
					break;
			}
		}

		void OnButtonHandler(ButtonHandlerModel handlerModel)
		{
			var current = new ButtonEncounterLogPresenter(model, handlerModel.Log, handlerModel.Buttons);
			current.Show(View.EntryArea, OnShownLog);
			entries.Add(current);
		}

		void OnTextHandler(TextHandlerModel handlerModel)
		{
			var current = new TextEncounterLogPresenter(model, handlerModel.Log);
			current.Show(View.EntryArea, OnShownLog);
			entries.Add(current);
		}

		void OnShownLog()
		{
			View.Scroll = 0f;
		}
		#endregion
	}
}