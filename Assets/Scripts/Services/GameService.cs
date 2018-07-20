using System;
using System.Linq;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class GameService
	{
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
			var maximumNavigationTime = 10f;

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem.Position;
			ship.Position.Value = startPosition;
			ship.Speed.Value = speed;
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

			// Setting the request to Complete for consistency, since that's how
			// the game will normally be opened from a save.
			game.FocusRequest.Value = new SystemsFocusRequest(
				startSystem.Position.Value.SystemZero,
				startSystem.Position,
				FocusRequest.States.Complete
			);

			var endSector = game.Universe.Value.GetSector(startSystem.Position + new UniversePosition(new Vector3(0f, 0f, 1f), Vector3.zero));
			game.EndSystem.Value = endSector.Systems.Value.First().Position;

			// Generating inventory, eventually will be done somewhere else...
			var rootModule = new ModuleInventoryModel();
			rootModule.Name.Value = "RootModule";
			rootModule.InstanceId.Value = "rootid";
			rootModule.IsRoot.Value = true;

			var crewModule0 = new CrewModuleSlotModel();
			crewModule0.SlotId.Value = "crewslot0";
			crewModule0.ValidInventoryTypes.Value = crewModule0.ValidInventoryTypes.Value.Append(InventoryTypes.OrbitalCrew).ToArray();
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(crewModule0).ToArray();

			var crewModule1 = new CrewModuleSlotModel();
			crewModule1.SlotId.Value = "crewslot1";
			crewModule1.ValidInventoryTypes.Value = crewModule1.ValidInventoryTypes.Value.Append(InventoryTypes.OrbitalCrew).ToArray();
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(crewModule1).ToArray();

			var modulePlug = new ModuleModuleSlotModel();
			modulePlug.SlotId.Value = "plug0";
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(modulePlug).ToArray();

			var modulePlug1 = new ModuleModuleSlotModel();
			modulePlug1.SlotId.Value = "plug1";
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(modulePlug1).ToArray();

			var resourceModule = new ModuleInventoryModel();
			resourceModule.Name.Value = "SomeResourceModule";
			resourceModule.InstanceId.Value = "recid";

			var resourceMod = new ResourceModuleSlotModel();
			resourceMod.SlotId.Value = "resourceMod0";
			resourceMod.MaximumResources.Assign(new ResourceInventoryModel(1f, 2f));
			resourceMod.MaximumLogisticsResources.Assign(new ResourceInventoryModel(0.5f, 0.5f));
			resourceMod.RefillLogisticsResources.Assign(new ResourceInventoryModel(0.1f, 0.1f));
			resourceModule.Slots.All.Value = resourceModule.Slots.All.Value.Append(resourceMod).ToArray();

			game.Ship.Value.Inventory.Connect(modulePlug, resourceModule);

			var crewHabModule = new ModuleInventoryModel();
			crewHabModule.Name.Value = "SomeCrewHabModule";
			crewHabModule.InstanceId.Value = "crewhab";

			var crewEatingMod = new ResourceModuleSlotModel();
			crewEatingMod.SlotId.Value = "crewEatingMod0";
			crewEatingMod.RefillResources.Assign(new ResourceInventoryModel(-rationConsumption, 0f));
			crewHabModule.Slots.All.Value = crewHabModule.Slots.All.Value.Append(crewEatingMod).ToArray();

			game.Ship.Value.Inventory.Connect(modulePlug1, crewHabModule);

			// Crew generation, eventually will be done somewhere else...
			var multiCrew = new OrbitalCrewInventoryModel();
			multiCrew.Name.Value = "MultiCrew";
			multiCrew.Description.Value = "A multi crew, neat!";
			multiCrew.InventoryId.Value = "10";
			multiCrew.InstanceId.Value = "A0";
			multiCrew.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Star, BodyTypes.Terrestrial };
			game.Ship.Value.Inventory.Connect(crewModule0, multiCrew);

			var terrestrialCrew = new OrbitalCrewInventoryModel();
			terrestrialCrew.Name.Value = "TerrestrialCrew";
			terrestrialCrew.Description.Value = "Time to hit the surface, with a crew!";
			terrestrialCrew.InventoryId.Value = "20";
			terrestrialCrew.InstanceId.Value = "B0";
			terrestrialCrew.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Terrestrial };
			game.Ship.Value.Inventory.Connect(crewModule1, terrestrialCrew);

			var starCrew = new OrbitalCrewInventoryModel();
			starCrew.Name.Value = "StarCrew";
			starCrew.Description.Value = "Stars are cool, your crew won't be tho";
			starCrew.InventoryId.Value = "30";
			starCrew.InstanceId.Value = "C0";
			starCrew.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Star };

			game.Ship.Value.Inventory.All.Value = new InventoryModel[] {
				rootModule,
				resourceModule,
				crewHabModule,
				multiCrew,
				terrestrialCrew,
				starCrew
			};

			// Uncomment this to make the game easy.
			//game.EndSystem.Value = game.Universe.Value.GetSector(startSystem.Position).Systems.Value.OrderBy(s => UniversePosition.Distance(startSystem.Position, s.Position)).ElementAt(1).Position;

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