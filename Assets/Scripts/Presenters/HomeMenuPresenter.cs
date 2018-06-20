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
			foreach (var save in saves)
			{
				entries.Add(new LabelButtonBlock(save.SaveType+": "+save.Meta, () => OnLoadGameClick(save)));
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
					//var payload = new GamePayload();
					//App.SM.RequestState(payload);
					break;
			}
		}

		void OnLoadGameClick(SaveModel model)
		{
			Debug.Log("Loading " + model.Meta);
			App.SaveLoadService.Load<GameSaveModel>(model, OnLoadedGame);
		}

		void OnLoadedGame(SaveLoadRequest<GameSaveModel> result)
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
			var gameSave = App.SaveLoadService.Create<GameSaveModel>();
			gameSave.Game.Value = new GameModel();
			var game = gameSave.Game.Value;
			game.Seed.Value = DemonUtility.NextInteger;
			game.Universe.Value = App.UniverseService.CreateUniverse(1);
			game.FocusedSector.Value = new UniversePosition(Vector3.negativeInfinity, Vector3.negativeInfinity);
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
			//var travelRadiusChange = new TravelRadiusChange(startPosition, speed, rationConsumption, rations);

			//var travelRequest = new TravelRequest(
			//	TravelRequest.States.Complete,
			//	startSystem.Position.Value,
			//	startSystem,
			//	startSystem,
			//	DayTime.Zero,
			//	DayTime.Zero,
			//	1f
			//);

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem.Position;
			ship.Position.Value = startPosition;
			ship.Speed.Value = speed;
			ship.RationConsumption.Value = rationConsumption;
			ship.Rations.Value = rations;

			game.Ship.Value = ship;

			App.SaveLoadService.Save(gameSave, OnSaveGame);
		}

		void OnSaveGame(SaveLoadRequest<GameSaveModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(result.Error));
				return;
			}
			OnStartGame(result.TypedModel);
		}

		void OnStartGame(GameSaveModel model)
		{
			var payload = new GamePayload();
			payload.GameSave = model;
			payload.Game = model.Game;
			App.SM.RequestState(payload);
		}
		#endregion

	}
}