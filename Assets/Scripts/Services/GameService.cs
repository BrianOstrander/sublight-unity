using System;
using System.Linq;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class GameService
	{
		static class DefaultGameBlock
		{
			public const string GalaxyId = "bed1e465-32ad-4eae-8135-d01eac75a089"; // Milkyway
			public const string GalaxyTargetId = "a6603c5e-f151-45aa-96bb-30905e781573"; // Andromeda
		}

		static class DefaultShip
		{
			//public const string StockRoot = "eccdddb4-553f-4f7a-be7e-b68799839bc8";
			//public const string StockOrbiterBay = "23c48a72-e35a-44d6-bd7e-29e600a76046";
			//public const string StockStorage = "ba230c9e-33a9-4a26-ba2c-de25cc3a0b27";
			//public const string StockLogistics = "90a2c3b4-7b41-449f-b23f-86bc201a1729";
			//public const string StockEngine = "88d5280d-7802-401a-bb25-24b1e4ee46ca";
			//public const string StockTerrestrialOrbiter = "84af3d23-d1c6-4bfd-bec1-c17b11c15269";
			//public const string StockStellarOrbiter = "290702f5-d9a7-43e4-907f-23dfdb579eb9";
			//public const string StockMultiOrbiter = "3442e5dd-00ae-4a1a-bd1c-234e652901d7";
		}

		IModelMediator modelMediator;
		IUniverseService universeService;

		public GameService(IModelMediator modelMediator, IUniverseService universeService)
		{
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (universeService == null) throw new ArgumentNullException("universeService");

			this.modelMediator = modelMediator;
			this.universeService = universeService;
		}

		public void CreateGame(CreateGameBlock info, Action<RequestStatus, GameModel> done)
		{
			if (done == null) throw new ArgumentNullException("done");

			var game = modelMediator.Create<GameModel>();

			game.Seed.Value = info.GameSeed;
			game.GalaxyId = StringExtensions.GetNonNullOrEmpty(info.GalaxyId, DefaultGameBlock.GalaxyId);
			game.GalaxyTargetId = StringExtensions.GetNonNullOrEmpty(info.GalaxyTargetId, DefaultGameBlock.GalaxyTargetId);
			game.Universe = universeService.CreateUniverse(info);

			game.FocusTransform.Value = FocusTransform.Default;

			var ship = new ShipModel();

			game.Ship.Value = ship;

			game.ToolbarSelection.Value = ToolbarSelections.System;

			App.M.Load<GalaxyInfoModel>(game.GalaxyId, result => OnGalaxyLoaded(result, game, done));
		}

		void OnGalaxyLoaded(SaveLoadRequest<GalaxyInfoModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			var galaxy = result.TypedModel;

			var foundBegin = false;
			var begin = galaxy.GetPlayerBegin(out foundBegin);
			if (!foundBegin)
			{
				Debug.LogError("Provided galaxy has no player begin defined");
				done(RequestStatus.Failure, null);
				return;
			}

			game.Ship.Value.Position.Value = begin;

			game.Universe.Sectors.Value = galaxy.GetSpecifiedSectors();

			OnShipReady(game, done);
		}

		void OnShipReady(GameModel game, Action<RequestStatus, GameModel> done)
		{
			// Uncomment this to make the goal closer.
			//game.EndSystem.Value = game.Universe.Value.GetSector(startSystem.Position).Systems.Value.OrderBy(s => UniversePosition.Distance(startSystem.Position, s.Position)).ElementAt(1).Position;

			// Uncomment this to make the void never expand
			//game.DestructionSpeed.Value = 0f;
			//game.DestructionSpeedIncrement.Value = 0f;

			modelMediator.Save(game, result => OnSaveGame(result, done));
		}

		void OnSaveGame(SaveLoadRequest<GameModel> result, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}
			done(result.Status, result.TypedModel);
		}
	}
}