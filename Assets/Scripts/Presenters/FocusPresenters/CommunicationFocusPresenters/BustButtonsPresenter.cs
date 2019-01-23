using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class BustButtonsPresenter : CommunicationFocusPresenter<IBustButtonsView>
	{
		//GameModel model;

		public BustButtonsPresenter(GameModel model)
		{
			//this.model = model;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		protected override void OnUpdateEnabled()
		{
			View.SetButtons(
				View.DefaultTheme,
				new BustButtonBlock
				{
					Message = "First Button",
					Used = false,
					Interactable = true,
					Click = () => Debug.Log("Clicked first")
				},
				new BustButtonBlock
				{
					Message = "Second Button",
					Used = true,
					Interactable = true,
					Click = () => Debug.Log("Clicked second")
				},
				new BustButtonBlock
				{
					Message = "Third Button",
					Used = false,
					Interactable = false,
					Click = () => Debug.Log("Clicked third")
				},
				new BustButtonBlock
				{
					Message = "Fourth Button",
					Used = true,
					Interactable = false,
					Click = () => Debug.Log("Clicked fourth")
				}
			);
		}

		#region Events
		void OnEncounterRequest(EncounterRequest request)
		{
			if (request.State == EncounterRequest.States.Handle) request.TryHandle<ButtonHandlerModel>(OnHandleButtons);
		}

		void OnHandleButtons(ButtonHandlerModel handler)
		{
			if (handler.Log.Value.Style.Value != ButtonEncounterLogModel.Styles.Bust) return;

			Debug.Log("todo: handl buttons");
		}
		#endregion
	}
}