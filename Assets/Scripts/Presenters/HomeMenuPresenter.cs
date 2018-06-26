using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
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
				entries.Add(new LabelButtonBlock(text, click));
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
			App.Callbacks.DialogRequest(DialogRequest.Alert("This save file is no longer supported."));
		}

		void OnLoadGameClick(SaveModel model)
		{
			App.SaveLoadService.Load<GameModel>(model, OnLoadedGame);
		}

		void OnLoadedGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(result.Error));
				return;
			}
			App.SaveLoadService.Save(result.TypedModel, OnSaveGame);
		}

		void OnNewGame()
		{
			var game = App.SaveLoadService.Create<GameModel>();
			game.Seed.Value = DemonUtility.NextInteger;
			game.Universe.Value = App.UniverseService.CreateUniverse(1);
			game.FocusedSector.Value = UniversePosition.Zero;
			game.FocusedSectors.Value = new UniversePosition[0];
			game.DestructionSpeed.Value = 0.01f;

			var startSystem = game.Universe.Value.Sectors.Value.First().Systems.Value.First();
			var lastDistance = UniversePosition.Distance(UniversePosition.Zero, startSystem.Position);
			foreach (var system in game.Universe.Value.Sectors.Value.First().Systems.Value)
			{
				var distance = UniversePosition.Distance(UniversePosition.Zero, system.Position);
				if (lastDistance < distance) continue;
				lastDistance = distance;
				startSystem = system;
			}

			startSystem.Visited.Value = true;
			var startPosition = startSystem.Position;
			var rations = 0.3f;
			var speed = 0.003f;
			var rationConsumption = 0.02f;

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem.Position;
			ship.Position.Value = startPosition;
			ship.Speed.Value = speed;
			ship.RationConsumption.Value = rationConsumption;
			ship.Rations.Value = rations;

			game.Ship.Value = ship;

			game.TravelRequest.Value = new TravelRequest(
				TravelRequest.States.Complete,
				startSystem.Position,
				startSystem.Position,
				startSystem.Position,
				DayTime.Zero,
				DayTime.Zero,
				1f
			);

			App.SaveLoadService.Save(game, OnSaveGame);
		}

		void OnSaveGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(result.Error));
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