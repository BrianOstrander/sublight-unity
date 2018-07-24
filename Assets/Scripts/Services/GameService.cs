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

			// - Root
			var rootModule = new ModuleInventoryModel();
			rootModule.Name.Value = "Root Module";
			rootModule.InstanceId.Value = "rootModule";
			rootModule.IsRoot.Value = true;

			var orbitalBerthNode0 = new CrewModuleSlotModel();
			orbitalBerthNode0.SlotId.Value = "orbitalBerthNode0";
			orbitalBerthNode0.ValidInventoryTypes.Value = orbitalBerthNode0.ValidInventoryTypes.Value.Append(InventoryTypes.OrbitalCrew).ToArray();
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(orbitalBerthNode0).ToArray();

			var orbitalBerthNode1 = new CrewModuleSlotModel();
			orbitalBerthNode1.SlotId.Value = "orbitalBerthNode1";
			orbitalBerthNode1.ValidInventoryTypes.Value = orbitalBerthNode1.ValidInventoryTypes.Value.Append(InventoryTypes.OrbitalCrew).ToArray();
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(orbitalBerthNode1).ToArray();

			var moduleNode0 = new ModuleModuleSlotModel();
			moduleNode0.SlotId.Value = "moduleNode0";
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(moduleNode0).ToArray();

			var moduleNode1 = new ModuleModuleSlotModel();
			moduleNode1.SlotId.Value = "moduleNode1";
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(moduleNode1).ToArray();

			var crewResourceNode = new ResourceModuleSlotModel();
			crewResourceNode.SlotId.Value = "crewResourceNode";
			crewResourceNode.RefillResources.Assign(new ResourceInventoryModel(-rationConsumption, 0f));
			rootModule.Slots.All.Value = rootModule.Slots.All.Value.Append(crewResourceNode).ToArray();

			// - Resource
			var resourceModule = new ModuleInventoryModel();
			resourceModule.Name.Value = "Basic Resource Module";
			resourceModule.InstanceId.Value = "resourceModule";

			var resourceNode0 = new ResourceModuleSlotModel();
			resourceNode0.SlotId.Value = "resourceNode0";
			resourceNode0.MaximumResources.Assign(new ResourceInventoryModel(2f, 2f));
			resourceNode0.MaximumLogisticsResources.Assign(new ResourceInventoryModel(1f, 1f));
			resourceNode0.RefillLogisticsResources.Assign(new ResourceInventoryModel(0.1f, 0.1f));
			resourceModule.Slots.All.Value = resourceModule.Slots.All.Value.Append(resourceNode0).ToArray();

			game.Ship.Value.Inventory.Connect(moduleNode0, resourceModule);

			// Crew generation, eventually will be done somewhere else...
			var orbitalShuttle0 = new OrbitalCrewInventoryModel();
			orbitalShuttle0.Name.Value = "Advanced Orbiter";
			orbitalShuttle0.Description.Value = "A multi crew, neat!";
			orbitalShuttle0.InventoryId.Value = "orbitalShuttle0";
			orbitalShuttle0.InstanceId.Value = "orbitalShuttle0_0";
			orbitalShuttle0.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Star, BodyTypes.Terrestrial };
			game.Ship.Value.Inventory.Connect(orbitalBerthNode0, orbitalShuttle0);

			var orbitalShuttle1 = new OrbitalCrewInventoryModel();
			orbitalShuttle1.Name.Value = "Terrestrial Orbiter";
			orbitalShuttle1.Description.Value = "Time to hit the surface, with a crew!";
			orbitalShuttle1.InventoryId.Value = "orbitalShuttle1";
			orbitalShuttle1.InstanceId.Value = "orbitalShuttle1_0";
			orbitalShuttle1.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Terrestrial };
			game.Ship.Value.Inventory.Connect(orbitalBerthNode1, orbitalShuttle1);

			var orbitalShuttle2 = new OrbitalCrewInventoryModel();
			orbitalShuttle2.Name.Value = "Stellar Orbiter";
			orbitalShuttle2.Description.Value = "Stars are cool, your crew won't be tho";
			orbitalShuttle2.InventoryId.Value = "orbitalShuttle2";
			orbitalShuttle2.InstanceId.Value = "orbitalShuttle2_0";
			orbitalShuttle2.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Star };

			game.Ship.Value.Inventory.All.Value = new InventoryModel[] {
				rootModule,
				resourceModule,
				orbitalShuttle0,
				orbitalShuttle1,
				orbitalShuttle2
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