using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

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

				public const ToolbarSelections ToolbarSelection = ToolbarSelections.System;
			}

			public const float TransitRangeMinimum = 1f; // In universe units.
			public const float TransitVelocityMinimum = 0.3f * UniversePosition.LightYearToUniverseScalar; // In universe units. Shouldn't be greater than 1 lightyear...
		}

		struct LoadInstructions
		{
			public bool IsFirstLoad;
			public DateTime CurrentTime;
			public DeveloperViews[] DeveloperViewsEnabled;

			public LoadInstructions ApplyDefaults()
			{
				CurrentTime = DateTime.Now;
				DeveloperViewsEnabled = EnumExtensions.GetValues(DeveloperViews.Unknown);

				return this;
			}
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

		#region Exposed Utilities
		/// <summary>
		/// Creates a new game using the specified info.
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="done">Done.</param>
		public void CreateGame(CreateGameBlock info, Action<RequestResult, GameModel> done)
		{
			if (done == null) throw new ArgumentNullException("done");

			var model = modelMediator.Create<GameModel>();

			model.Seed.Value = info.GameSeed;
			model.GalaxyId = StringExtensions.GetNonNullOrEmpty(info.GalaxyId, Defaults.CreateGameBlock.GalaxyId);
			model.GalaxyTargetId = StringExtensions.GetNonNullOrEmpty(info.GalaxyTargetId, Defaults.CreateGameBlock.GalaxyTargetId);
			model.Universe = universeService.CreateUniverse(info);

			var initialTime = DayTime.Zero;
			model.RelativeDayTime.Value = new RelativeDayTime(
				initialTime,
				initialTime
			);

			// Ship ---
			// TODO: Some of these values should be based on... like... the ship's inventory.
			model.Ship.SetRangeMinimum(Defaults.TransitRangeMinimum);
			//model.Ship.SetVelocityMinimum(Defaults.TransitVelocityMinimum);
			//model.Ship.SetVelocityMultiplierMaximum(7);
			//model.Ship.SetVelocityMultiplierEnabledMaximum(5);
			// --------

			model.ToolbarSelection.Value = info.ToolbarSelection == ToolbarSelections.Unknown ? Defaults.CreateGameBlock.ToolbarSelection : info.ToolbarSelection;

			OnInitializeGame(
				new LoadInstructions
				{
					IsFirstLoad = true
				}.ApplyDefaults(),
				model,
				done
			);
		}

		/// <summary>
		/// Loads the specified game and populates the context with required
		/// values.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="done">Done.</param>
		public void LoadGame(GameModel model, Action<RequestResult, GameModel> done)
		{
			if (model == null) throw new ArgumentNullException("model");
			if (done == null) throw new ArgumentNullException("done");

			OnInitializeGame(
				new LoadInstructions
				{
					// Nothing to do here...
				}.ApplyDefaults(),
				model,
				done
			);
		}

		/// <summary>
		/// Retrieves the most recent continuable game, if one exists, otherwise null.
		/// </summary>
		/// <remarks>
		/// A sucessful result will still return null if no continuable games are available.
		/// </remarks>
		/// <param name="done">Done.</param>
		public void ContinueGame(Action<RequestResult, GameModel> done)
		{
			if (done == null) throw new ArgumentNullException("done");

			App.M.List<GameModel>(result => OnContinueGameList(result, done));
		}
		#endregion

		#region Continue Game
		void OnContinueGameList(SaveLoadArrayRequest<SaveModel> result, Action<RequestResult, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load a list of saved games, error: "+result.Error);
				done(RequestResult.Failure(result.Error), null);
				return;
			}

			var continueGame = result.Models.Where(s => s.SupportedVersion.Value).OrderByDescending(s => s.Modified.Value).FirstOrDefault(m => m.GetMetaKey(MetaKeyConstants.Game.IsCompleted) != MetaKeyConstants.Values.True);

			if (continueGame == null)
			{
				done(RequestResult.Success(), null);
				return;
			}

			App.M.Load<GameModel>(continueGame, loadResult => OnContinueGameLoad(loadResult, done));
		}

		void OnContinueGameLoad(SaveLoadRequest<GameModel> result, Action<RequestResult, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Continue game load status " + result.Status + " error: " + result.Error);
				done(RequestResult.Failure(result.Error), null);
				return;
			}

			LoadGame(result.TypedModel, done);
		}
		#endregion

		#region Initialization
		void OnInitializeGame(LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			model.Context.FocusTransform.Value = FocusTransform.Default;

			if (string.IsNullOrEmpty(model.GalaxyId))
			{
				done(RequestResult.Failure("No GalaxyId to load").Log(), null);
				return;
			}
			App.M.Load<GalaxyInfoModel>(model.GalaxyId, result => OnLoadGalaxy(result, instructions, model, done));

		}

		void OnLoadGalaxy(SaveLoadRequest<GalaxyInfoModel> result, LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				done(RequestResult.Failure("Unable to load galaxy, resulted in " + result.Status + " and error: " + result.Error).Log(), null);
				return;
			}
			model.Context.Galaxy = result.TypedModel;

			if (string.IsNullOrEmpty(model.GalaxyTargetId))
			{
				done(RequestResult.Failure("No GalaxyTargetId to load").Log(), null);
				return;
			}

			App.M.Load<GalaxyInfoModel>(model.GalaxyTargetId, targetResult => OnLoadGalaxyTarget(targetResult, instructions, model, done));
		}

		void OnLoadGalaxyTarget(SaveLoadRequest<GalaxyInfoModel> result, LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				done(RequestResult.Failure("Unable to load galaxy target, resulted in " + result.Status + " and error: " + result.Error).Log(), null);
				return;
			}
			model.Context.GalaxyTarget = result.TypedModel;

			if (instructions.IsFirstLoad) OnInitializeFirstLoad(instructions, model, done);
			else SetContext(instructions, model, done);
		}

		void OnInitializeFirstLoad(LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			// By this point the galaxy and target galaxy should already be set.

			var beginFound = false;
			SectorModel beginSector;
			SystemModel beginSystem;
			var begin = model.Context.Galaxy.GetPlayerBegin(out beginFound, out beginSector, out beginSystem);
			if (!beginFound)
			{
				done(RequestResult.Failure("Provided galaxy has no player begin defined").Log(), null);
				return;
			}

			var endFound = false;
			SectorModel endSector;
			SystemModel endSystem;
			var end = model.Context.Galaxy.GetPlayerEnd(out endFound, out endSector, out endSystem);
			if (!endFound)
			{
				done(RequestResult.Failure("Provided galaxy has no player end defined").Log(), null);
				return;
			}

			model.Ship.Position.Value = begin;
			model.Ship.SystemIndex.Value = beginSystem.Index.Value;

			var transitHistoryBegin = TransitHistoryEntry.Begin(instructions.CurrentTime, beginSystem);

			model.TransitHistory.Push(transitHistoryBegin);
			model.SaveDetails.Value = new GameSaveDetails(
				transitHistoryBegin.Id,
				transitHistoryBegin.EnterTime,
				transitHistoryBegin.ElapsedTime
			);

			var shipWaypoint = new WaypointModel();
			shipWaypoint.SetLocation(begin);
			shipWaypoint.WaypointId.Value = WaypointIds.Ship;
			shipWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Visible;
			shipWaypoint.VisitState.Value = WaypointModel.VisitStates.Current;
			shipWaypoint.RangeState.Value = WaypointModel.RangeStates.InRange;
			shipWaypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, begin);

			model.Waypoints.AddWaypoint(shipWaypoint);

			var beginWaypoint = new WaypointModel();
			beginWaypoint.SetLocation(beginSystem);
			beginWaypoint.WaypointId.Value = WaypointIds.BeginSystem;
			beginWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Hidden;
			beginWaypoint.VisitState.Value = WaypointModel.VisitStates.Visited;
			beginWaypoint.RangeState.Value = WaypointModel.RangeStates.InRange;
			beginWaypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, begin);

			model.Waypoints.AddWaypoint(beginWaypoint);

			var endWaypoint = new WaypointModel();
			endWaypoint.SetLocation(endSystem);
			endWaypoint.WaypointId.Value = WaypointIds.EndSystem;
			endWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Visible;
			endWaypoint.VisitState.Value = WaypointModel.VisitStates.NotVisited;
			endWaypoint.RangeState.Value = WaypointModel.RangeStates.OutOfRange;
			endWaypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, end);

			model.Waypoints.AddWaypoint(endWaypoint);

			model.Universe.Sectors.Value = model.Context.Galaxy.GetSpecifiedSectors();

			SetContext(instructions, model, done);
		}

		void SetContext(LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			// By this point the galaxy and target galaxy should already be set.
			// Additionally, begin, end, specified sectors, and waypoints should be defined.

			model.Context.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(model.ToolbarSelection.Value, false, ToolbarSelectionRequest.Sources.Player);

			foreach (var developerView in instructions.DeveloperViewsEnabled) model.Context.DeveloperViewsEnabled.Push(developerView);

			model.Context.SetCurrentSystem(universeService.GetSystem(model.Context.Galaxy, model.Universe, model.Ship.Position.Value, model.Ship.SystemIndex.Value));

			if (instructions.IsFirstLoad || model.TransitHistory.Count() == 1)
			{
				model.Context.TransitState.Value = TransitState.Default(model.Context.CurrentSystem, model.Context.CurrentSystem);
			}
			else
			{
				var previousSystem = model.TransitHistory.Peek(1);
				model.Context.TransitState.Value = TransitState.Default(
					universeService.GetSystem(model.Context.Galaxy, model.Universe, previousSystem.SystemPosition, previousSystem.SystemIndex),
					model.Context.CurrentSystem
				);
			}

			model.Context.SetCurrentSystem(
				App.Universe.GetSystem(
					model.Context.Galaxy,
					model.Universe,
					model.Ship.Position,
					model.Ship.SystemIndex
				)
			);

			if (model.Context.CurrentSystem.Value == null)
			{
				done(RequestResult.Failure("Unable to load current system at " + model.Ship.Position.Value + " and index " + model.Ship.SystemIndex.Value).Log(), null);
				return;
			}

			foreach (var waypoint in model.Waypoints.Waypoints.Value)
			{
				switch (waypoint.WaypointId.Value)
				{
					case WaypointIds.Ship:
						waypoint.Name.Value = "Ark";
						break;
					case WaypointIds.BeginSystem:
						waypoint.Name.Value = "Origin";
						break;
					case WaypointIds.EndSystem:
						waypoint.Name.Value = "Cygnus X-1";
						break;
				}

				if (!waypoint.Location.Value.IsSystem) continue;

				var currWaypointSystem = App.Universe.GetSystem(
					model.Context.Galaxy,
					model.Universe,
					waypoint.Location.Value.Position,
					waypoint.Location.Value.SystemIndex
				);

				if (currWaypointSystem == null)
				{
					done(
						RequestResult.Failure(
							"Unable to load waypoint system ( WaypointId: " + waypoint.WaypointId.Value + " , Name: " + waypoint.Name.Value + " ) at\n" + waypoint.Location.Value.Position + " and index " + waypoint.Location.Value.SystemIndex
						).Log(),
						null
					);
					return;
				}
				waypoint.SetLocation(currWaypointSystem);
			}

			modelMediator.Save(model, result => OnSaveGame(result, instructions, model, done));
		}

		void OnSaveGame(
			SaveLoadRequest<GameModel> result,
			LoadInstructions instructions,
			GameModel model,
			Action<RequestResult, GameModel> done
		)
		{
			if (result.Status != RequestStatus.Success)
			{
				done(RequestResult.Failure(result.Error).Log(), null);
				return;
			}

			// Return the passed model rather than the save result, since we're keeping the Context data.
			done(RequestResult.Success(), model);

			//done(RequestResult.Failure("Some fake error"), null);
		}
		#endregion
	}
}