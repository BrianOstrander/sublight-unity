using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class HomeMenuPresenter : Presenter<IHomeMenuView>
	{
		SaveModel[] saves;

		public HomeMenuPresenter(params SaveModel[] saves)
		{
			this.saves = saves;
		}

		public void Show(Action done)
		{
			if (View.Visible) return;

			View.Reset();

			var entries = new List<LabelButtonBlock>();
			var index = 0;
			foreach (var save in saves)
			{
				var text = "Save Game #" + index;
				Action click = () => OnLoadGameClick(save);
				if (!save.SupportedVersion)
				{
					text += " <obsolete>";
					click = OnClickObsoleteGame;
				}
				entries.Add(new LabelButtonBlock(LanguageStringModel.Override(text), click));
				index++;
			}

			View.LoadEntries = entries.ToArray();
			View.StartClick = OnStartClick;
			View.Shown += done;
			ShowView(App.GameCanvasRoot, true);
		}

		#region Events
		void OnStartClick()
		{
			switch (View.TransitionState)
			{
				case TransitionStates.Shown:
					CloseView(true);
					OnNewGame();
					break;
			}
		}

		void OnClickObsoleteGame()
		{
			App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override("This save file is no longer supported.")));
		}

		void OnLoadGameClick(SaveModel model)
		{
			App.M.Load<GameModel>(model, OnLoadedGame);
		}

		void OnLoadedGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override(result.Error)));
				return;
			}
			App.M.Save(result.TypedModel, OnSaveGame);
		}

		void OnNewGame()
		{
			App.GameService.CreateGame(OnNewGameCreated);
		}

		void OnNewGameCreated(RequestStatus result, GameModel model)
		{
			if (result != RequestStatus.Success)
			{
				App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override("Creating new game returned with result "+result)));
				return;
			}
			OnStartGame(model);
		}

		void OnSaveGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override(result.Error)));
				return;
			}
			OnStartGame(result.TypedModel);
		}

		void OnStartGame(GameModel model)
		{
			var payload = new GamePayload();
			payload.Game = model;
			App.SM.RequestState(payload);
		}
		#endregion

	}
}