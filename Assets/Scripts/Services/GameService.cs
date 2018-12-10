using System;
using System.Linq;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class GameService
	{
		static class Defaults
		{
			public static class CreateGameBlock
			{
				public const string GalaxyId = "bed1e465-32ad-4eae-8135-d01eac75a089"; // Milkyway
				public const string GalaxyTargetId = "a6603c5e-f151-45aa-96bb-30905e781573"; // Andromeda
			}

			public const float MinimumTravelRange = 1f; // In universe units.
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
			game.GalaxyId = StringExtensions.GetNonNullOrEmpty(info.GalaxyId, Defaults.CreateGameBlock.GalaxyId);
			game.GalaxyTargetId = StringExtensions.GetNonNullOrEmpty(info.GalaxyTargetId, Defaults.CreateGameBlock.GalaxyTargetId);
			game.Universe = universeService.CreateUniverse(info);

			game.FocusTransform.Value = FocusTransform.Default;

			// Ship ---
			var ship = new ShipModel();
			ship.SetMinimumTravelRange(Defaults.MinimumTravelRange);

			game.Ship.Value = ship;
			// --------

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