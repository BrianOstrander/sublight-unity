using System;
using System.Linq;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class GameService
	{
		static class DefaultShip
		{
			public const string StockRoot = "eccdddb4-553f-4f7a-be7e-b68799839bc8";
			public const string StockOrbiterBay = "23c48a72-e35a-44d6-bd7e-29e600a76046";
			public const string StockStorage = "ba230c9e-33a9-4a26-ba2c-de25cc3a0b27";
			public const string StockLogistics = "90a2c3b4-7b41-449f-b23f-86bc201a1729";
			public const string StockEngine = "88d5280d-7802-401a-bb25-24b1e4ee46ca";
			public const string StockTerrestrialOrbiter = "84af3d23-d1c6-4bfd-bec1-c17b11c15269";
			public const string StockStellarOrbiter = "290702f5-d9a7-43e4-907f-23dfdb579eb9";
			public const string StockMultiOrbiter = "3442e5dd-00ae-4a1a-bd1c-234e652901d7";
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

		public void CreateGame(Action<RequestStatus, GameModel> done)
		{
			if (done == null) throw new ArgumentNullException("done");

			var game = modelMediator.Create<GameModel>();
			game.Seed.Value = DemonUtility.NextInteger;
			game.Universe.Value = universeService.CreateUniverse(1);
			game.FocusedSector.Value = UniversePosition.Zero;
			game.DestructionSpeed.Value = 0.004f;
			game.DestructionSpeedIncrement.Value = 0.0025f;

			game.Zoom.Value = 1f;

			var startSystem = game.Universe.Value.Sectors.Value.First().Systems.Value.First();
			var lastDistance = UniversePosition.Distance(UniversePosition.Zero, startSystem.Position);
			foreach (var system in game.Universe.Value.Sectors.Value.First().Systems.Value)
			{
				var distance = UniversePosition.Distance(UniversePosition.Zero, system.Position);
				if (lastDistance < distance) continue;
				lastDistance = distance;
				startSystem = system;
			}

			var startPosition = startSystem.Position;
			var rations = 0.3f;
			var fuel = 1f;
			var fuelConsumption = 1f;
			var resourceDetection = 0.5f;
			var maximumNavigationTime = 10f;

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem.Position;
			ship.Position.Value = startPosition;
			ship.Inventory.AllResources.Rations.Value = rations;
			ship.Inventory.AllResources.Fuel.Value = fuel;
			ship.FuelConsumption.Value = fuelConsumption;
			ship.ResourceDetection.Value = resourceDetection;
			ship.MaximumNavigationTime.Value = new DayTime(maximumNavigationTime);

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

			game.ToolbarSelection.Value = ToolbarSelections.System;

			var endSector = game.Universe.Value.GetSector(startSystem.Position + new UniversePosition(new Vector3(0f, 0f, 1f), Vector3.zero));
			game.EndSystem.Value = endSector.Systems.Value.First().Position;

			App.InventoryReferences.CreateInstance<ModuleInventoryModel>(
				DefaultShip.StockRoot,
				InventoryReferenceContext.Default,
				instanceResult => OnStockRootLoaded(instanceResult, game, done)
			);
		}

		void OnStockRootLoaded(InventoryReferenceRequest<ModuleInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<ModuleInventoryModel>(
				DefaultShip.StockOrbiterBay,
				InventoryReferenceContext.Default,
				instanceResult => OnStockOrbiterBayLoaded(instanceResult, game, done)
			);
		}

		void OnStockOrbiterBayLoaded(InventoryReferenceRequest<ModuleInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<ModuleInventoryModel>(
				DefaultShip.StockStorage,
				InventoryReferenceContext.Default,
				instanceResult => OnStockStorageLoaded(instanceResult, game, done)
			);
		}

		void OnStockStorageLoaded(InventoryReferenceRequest<ModuleInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<ModuleInventoryModel>(
				DefaultShip.StockLogistics,
				InventoryReferenceContext.Default,
				instanceResult => OnStockLogisticsLoaded(instanceResult, game, done)
			);
		}

		void OnStockLogisticsLoaded(InventoryReferenceRequest<ModuleInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<ModuleInventoryModel>(
				DefaultShip.StockEngine,
				InventoryReferenceContext.Default,
				instanceResult => OnStockEngineLoaded(instanceResult, game, done)
			);
		}

		void OnStockEngineLoaded(InventoryReferenceRequest<ModuleInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<OrbitalCrewInventoryModel>(
				DefaultShip.StockTerrestrialOrbiter,
				InventoryReferenceContext.Default,
				instanceResult => OnStockTerrestrialOrbiterLoaded(instanceResult, game, done)
			);
		}

		void OnStockTerrestrialOrbiterLoaded(InventoryReferenceRequest<OrbitalCrewInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<OrbitalCrewInventoryModel>(
				DefaultShip.StockStellarOrbiter,
				InventoryReferenceContext.Default,
				instanceResult => OnStockStellarOrbiterLoaded(instanceResult, game, done)
			);
		}

		void OnStockStellarOrbiterLoaded(InventoryReferenceRequest<OrbitalCrewInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			game.Ship.Value.Inventory.Add(result.Instance);

			App.InventoryReferences.CreateInstance<OrbitalCrewInventoryModel>(
				DefaultShip.StockMultiOrbiter,
				InventoryReferenceContext.Default,
				instanceResult => OnStockMultiOrbiterLoaded(instanceResult, game, done)
			);
		}

		void OnStockMultiOrbiterLoaded(InventoryReferenceRequest<OrbitalCrewInventoryModel> result, GameModel game, Action<RequestStatus, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(result.Status, null);
				return;
			}

			//game.Ship.Value.Inventory.Add(result.Instance);

			OnConnectShip(game, done);
		}

		void OnConnectShip(GameModel game, Action<RequestStatus, GameModel> done)
		{
			var inventory = game.Ship.Value.Inventory;

			var stockRoot = inventory.GetInventoryFirstOrDefault<ModuleInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockRoot);
			var stockOrbiterBay = inventory.GetInventoryFirstOrDefault<ModuleInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockOrbiterBay);
			var stockStorage = inventory.GetInventoryFirstOrDefault<ModuleInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockStorage);
			var stockLogistics = inventory.GetInventoryFirstOrDefault<ModuleInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockLogistics);
			var stockEngine = inventory.GetInventoryFirstOrDefault<ModuleInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockEngine);

			var stockTerrestrialOrbiter = inventory.GetInventoryFirstOrDefault<OrbitalCrewInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockTerrestrialOrbiter);
			var stockStellarOrbiter = inventory.GetInventoryFirstOrDefault<OrbitalCrewInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockStellarOrbiter);
			//var stockMultiOrbiter = inventory.GetInventoryFirstOrDefault<OrbitalCrewInventoryModel>(i => i.InventoryId.Value == DefaultShip.StockMultiOrbiter);

			var rootModule0 = stockRoot.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("module_0");
			var rootModule1 = stockRoot.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("module_1");
			var rootModule2 = stockRoot.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("module_2");
			var rootModule3 = stockRoot.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("module_3");
			//var rootModule4 = stockRoot.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("module_4");

			var orbitalBayOrbital0 = stockOrbiterBay.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("orbital_0");
			var orbitalBayOrbital1 = stockOrbiterBay.Slots.GetSlotFirstOrDefault<ModuleSlotModel>("orbital_1");

			inventory.Connect(rootModule0, stockOrbiterBay);
			inventory.Connect(rootModule1, stockStorage);
			inventory.Connect(rootModule2, stockLogistics);
			inventory.Connect(rootModule3, stockEngine);

			inventory.Connect(orbitalBayOrbital0, stockTerrestrialOrbiter);
			inventory.Connect(orbitalBayOrbital1, stockStellarOrbiter);

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