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

			public const float TransitRangeMinimum = 1f; // In universe units.
			public const float TransitVelocityMinimum = 0.3f * UniversePosition.LightYearToUniverseScalar; // In universe units. Shouldn't be greater than 1 lightyear...
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

			var initialTime = new DayTime((365 * 1057) + (30 * 2) + 5);
			game.RelativeDayTime.Value = new RelativeDayTime(
				initialTime,
				initialTime
			);

			// Ship ---
			var ship = new ShipModel();
			ship.SetRangeMinimum(Defaults.TransitRangeMinimum);
			ship.SetVelocityMinimum(Defaults.TransitVelocityMinimum);
			ship.SetVelocityMultiplierMaximum(7);
			ship.SetVelocityMultiplierEnabledMaximum(5);

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

			var beginFound = false;
			SectorModel beginSector;
			SystemModel beginSystem;
			var begin = galaxy.GetPlayerBegin(out beginFound, out beginSector, out beginSystem);
			if (!beginFound)
			{
				Debug.LogError("Provided galaxy has no player begin defined");
				done(RequestStatus.Failure, null);
				return;
			}

			var endFound = false;
			SectorModel endSector;
			SystemModel endSystem;
			var end = galaxy.GetPlayerEnd(out endFound, out endSector, out endSystem);
			if (!endFound)
			{
				Debug.LogError("Provided galaxy has no player end defined");
				done(RequestStatus.Failure, null);
				return;
			}

			game.Ship.Value.Position.Value = begin;
			game.Ship.Value.SetCurrentSystem(beginSystem);
			game.TransitState.Value = TransitState.Default(beginSystem, beginSystem);

			Debug.Log("reenable begin waypoint here");
			//var beginWaypoint = new WaypointModel();
			//beginWaypoint.SetLocation(beginSystem);
			//beginWaypoint.WaypointId.Value = WaypointIds.BeginSystem;
			//beginWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Visible;
			//beginWaypoint.VisitState.Value = WaypointModel.VisitStates.Current;
			//beginWaypoint.RangeState.Value = WaypointModel.RangeStates.InRange;
			//beginWaypoint.Distance.Value = UniversePosition.Distance(game.Ship.Value.Position.Value, begin);

			//game.WaypointCollection.AddWaypoint(beginWaypoint);

			var endWaypoint = new WaypointModel();
			endWaypoint.SetLocation(endSystem);
			endWaypoint.WaypointId.Value = WaypointIds.EndSystem;
			endWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Visible;
			endWaypoint.VisitState.Value = WaypointModel.VisitStates.NotVisited;
			endWaypoint.RangeState.Value = WaypointModel.RangeStates.OutOfRange;
			endWaypoint.Distance.Value = UniversePosition.Distance(game.Ship.Value.Position.Value, end);

			game.WaypointCollection.AddWaypoint(endWaypoint);

			game.Universe.Sectors.Value = galaxy.GetSpecifiedSectors();

			OnShipReady(game, done);
		}

		void OnShipReady(GameModel game, Action<RequestStatus, GameModel> done)
		{
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