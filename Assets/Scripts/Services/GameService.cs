using System;
using System.Linq;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class GameService
	{
		public void CreateGame(Action<RequestStatus, GameModel> done)
		{
			if (done == null) throw new ArgumentNullException("done");

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
			ship.Inventory.Resources.Rations.Value = rations;
			ship.Inventory.Resources.Fuel.Value = fuel;
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

			// Probe generation, eventually will be done somewhere else...
			var multiProbe = new OrbitalProbeInventoryModel();
			multiProbe.Name.Value = "MultiProbe";
			multiProbe.Description.Value = "A multi probe, neat!";
			multiProbe.InventoryId.Value = "1";
			multiProbe.InstanceId.Value = "A";
			multiProbe.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Star, BodyTypes.Terrestrial };

			var terrestrialProbe = new OrbitalProbeInventoryModel();
			terrestrialProbe.Name.Value = "TerrestrialProbe";
			terrestrialProbe.Description.Value = "Time to hit the surface";
			terrestrialProbe.InventoryId.Value = "2";
			terrestrialProbe.InstanceId.Value = "B";
			terrestrialProbe.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Terrestrial };

			var starProbe = new OrbitalProbeInventoryModel();
			starProbe.Name.Value = "StarProbe";
			starProbe.Description.Value = "Stars are cool";
			starProbe.InventoryId.Value = "3";
			starProbe.InstanceId.Value = "C";
			starProbe.SupportedBodies.Value = new BodyTypes[] { BodyTypes.Star };

			game.Ship.Value.Inventory.All.Value = new InventoryModel[] { multiProbe, terrestrialProbe, starProbe };

			// Uncomment this to make the game easy.
			//game.EndSystem.Value = game.Universe.Value.GetSector(startSystem.Position).Systems.Value.OrderBy(s => UniversePosition.Distance(startSystem.Position, s.Position)).ElementAt(1).Position;

			App.SaveLoadService.Save(game, result => OnSaveGame(result, done));
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