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
			game.DestructionSpeed.Value = 0.005f;
			game.DestructionSpeedIncrement.Value = 0.005f;

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
			var fuel = 1f;
			var fuelConsumption = 1f;
			var speed = 0.012f;
			var rationConsumption = 0.02f;
			var resourceDetection = 0.5f;

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem.Position;
			ship.Position.Value = startPosition;
			ship.Speed.Value = speed;
			ship.RationConsumption.Value = rationConsumption;
			ship.Rations.Value = rations;
			ship.Fuel.Value = fuel;
			ship.FuelConsumption.Value = fuelConsumption;
			ship.ResourceDetection.Value = resourceDetection;

			game.Ship.Value = ship;

			game.TravelRequest.Value = new TravelRequest(
				TravelRequest.States.Complete,
				startSystem.Position,
				startSystem.Position,
				startSystem.Position,
				DayTime.Zero,
				DayTime.Zero,
				0f,
				1f
			);

			// Setting the request to Complete for consistency, since that's how
			// the game will normally be opened from a save.
			game.FocusRequest.Value = new SystemsFocusRequest(
				startSystem.Position.Value.SystemZero,
				startSystem.Position,
				FocusRequest.States.Complete
			);

			var endSector = game.Universe.Value.GetSector(startSystem.Position + new UniversePosition(new Vector3(0f, 0f, 1f), Vector3.zero));
			game.EndSystem.Value = endSector.Systems.Value.First().Position;

			// Uncomment this to make the game easy.
			//game.EndSystem.Value = game.Universe.Value.GetSector(startSystem.Position).Systems.Value.OrderBy(s => UniversePosition.Distance(startSystem.Position, s.Position)).ElementAt(1).Position;



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