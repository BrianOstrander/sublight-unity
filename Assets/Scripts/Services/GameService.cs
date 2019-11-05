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
				public const ToolbarSelections ToolbarSelection = ToolbarSelections.Communication;
			}

			public const float TransitRangeMinimum = 1f; // In universe units.

			public const float TransitHistoryLineDistance = 8f; // In universe units.
			public const int TransitHistoryLineCount = 32;
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
			this.modelMediator = modelMediator ?? throw new ArgumentNullException(nameof(modelMediator));
			this.universeService = universeService ?? throw new ArgumentNullException(nameof(universeService));
		}

		#region Exposed Utilities
		/// <summary>
		/// Creates a new game using the specified info.
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="done">Done.</param>
		public void CreateGame(CreateGameBlock info, Action<RequestResult, GameModel> done)
		{
			if (done == null) throw new ArgumentNullException(nameof(done));

			var model = modelMediator.Create<GameModel>(App.M.CreateUniqueId());

			model.Name.Value = Guid.NewGuid().ToString();
			model.Id.Value = model.Name.Value;

			model.Seed.Value = info.GameSeed;
			model.GamemodeId = StringExtensions.GetNonNullOrEmpty(info.GamemodeId, App.BuildPreferences.DefaultGamemodeId);
			model.GalaxyId = StringExtensions.GetNonNullOrEmpty(info.GalaxyId, App.BuildPreferences.DefaultGalaxyId);
			model.GalaxyTargetId = StringExtensions.GetNonNullOrEmpty(info.GalaxyTargetId, App.BuildPreferences.DefaultGalaxyTargetId);
			model.Universe = universeService.CreateUniverse(info);

			var initialTime = DayTime.Zero;
			model.RelativeDayTime.Value = new RelativeDayTime(
				initialTime,
				initialTime
			);

			// Ship ---
			// TODO: Should this set minimum range be removed? It should just be set by the rules encounter, no?
			//model.Ship.SetRangeMinimum(Defaults.TransitRangeMinimum);
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
				new LoadInstructions().ApplyDefaults(),
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

			App.M.Index<GameModel>(result => OnContinueGameIndex(result, done));
		}
		#endregion

		#region Continue Game
		void OnContinueGameIndex(ModelIndexResult<SaveModel> result, Action<RequestResult, GameModel> done)
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

		void OnContinueGameLoad(ModelResult<GameModel> result, Action<RequestResult, GameModel> done)
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
		void OnInitializeGame(
			LoadInstructions instructions,
			GameModel model,
			Action<RequestResult, GameModel> done
		)
		{
			model.Context.FocusTransform.Value = FocusTransform.Default;

			if (string.IsNullOrEmpty(model.GamemodeId))
			{
				done(RequestResult.Failure("No GamemodeId to load").Log(), null);
				return;
			}
			App.M.Load<GamemodeInfoModel>(model.GamemodeId, result => OnLoadGamemode(result, instructions, model, done));
		}

		void OnLoadGamemode(
			ModelResult<GamemodeInfoModel> result,
			LoadInstructions instructions,
			GameModel model,
			Action<RequestResult, GameModel> done
		)
		{
			if (result.Status != RequestStatus.Success)
			{
				done(RequestResult.Failure("Unable to load gamemode, resulted in " + result.Status + " and error: " + result.Error).Log(), null);
				return;
			}
			model.Context.Gamemode = result.TypedModel;
			model.KeyValues.Set(KeyDefines.Game.GamemodeId, model.Context.Gamemode.Id.Value);

			if (string.IsNullOrEmpty(model.GalaxyId))
			{
				done(RequestResult.Failure("No GalaxyId to load").Log(), null);
				return;
			}
			App.M.Load<GalaxyInfoModel>(model.GalaxyId, galaxyResult => OnLoadGalaxy(galaxyResult, instructions, model, done));
		}

		void OnLoadGalaxy(
			ModelResult<GalaxyInfoModel> result,
			LoadInstructions instructions,
			GameModel model,
			Action<RequestResult, GameModel> done
		)
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

		void OnLoadGalaxyTarget(ModelResult<GalaxyInfoModel> result, LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				done(RequestResult.Failure("Unable to load galaxy target, resulted in " + result.Status + " and error: " + result.Error).Log(), null);
				return;
			}
			model.Context.GalaxyTarget = result.TypedModel;

			model.Context.ModuleService = new FudgedModuleService(modelMediator); 
			model.Context.ModuleService.Initialize(
				initializeModuleServiceResult => OnInitializeModuleService(
					initializeModuleServiceResult,
					instructions,
					model,
					done
				)
			);
		}
		
		void OnInitializeModuleService(
			Result<IModuleService> result,
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

			if (instructions.IsFirstLoad) OnInitializeFirstLoad(instructions, model, done);
			else SetContext(instructions, model, done);
		}
		
		void OnInitializeFirstLoad(
			LoadInstructions instructions,
			GameModel model,
			Action<RequestResult, GameModel> done
		)
		{
			// By this point the galaxy and target galaxy should already be set. ModuleService should be initialized.

			var begin = model.Context.Galaxy.GetPlayerBegin(out var beginFound, out _, out var beginSystem);
			if (!beginFound)
			{
				done(RequestResult.Failure("Provided galaxy has no player begin defined").Log(), null);
				return;
			}

			var end = model.Context.Galaxy.GetPlayerEnd(out var endFound, out _, out var endSystem);
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
			shipWaypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, begin);

			// --- May be deprecated eventually...
			model.Waypoints.AddWaypoint(shipWaypoint);

			var beginWaypoint = new WaypointModel();
			beginWaypoint.SetLocation(beginSystem);
			beginWaypoint.WaypointId.Value = WaypointIds.BeginSystem;
			beginWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Hidden;
			beginWaypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, begin);

			model.Waypoints.AddWaypoint(beginWaypoint);

			var endWaypoint = new WaypointModel();
			endWaypoint.SetLocation(endSystem);
			endWaypoint.WaypointId.Value = WaypointIds.EndSystem;
			endWaypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Hidden;
			endWaypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, end);

			model.Waypoints.AddWaypoint(endWaypoint);
			// --- 

			// --- This will require more steps once specified sector placement gets more complicated...
			var specifiedSectorInstances = model.Context.Galaxy.GetSpecifiedSectors();

			var allSectorPositions = new List<KeyValuePair<string, Vector3Int>>();

			foreach (var sectorInstance in specifiedSectorInstances)
			{
				switch (sectorInstance.SpecifiedPlacement.Value)
				{
					case SectorModel.SpecifiedPlacements.Position:
						allSectorPositions.Add(new KeyValuePair<string, Vector3Int>(sectorInstance.Name.Value, sectorInstance.Position.Value.SectorInteger));
						break;
					case SectorModel.SpecifiedPlacements.PositionList:
						foreach (var position in sectorInstance.PositionList.Value) allSectorPositions.Add(new KeyValuePair<string, Vector3Int>(sectorInstance.Name.Value, position.SectorInteger));
						sectorInstance.Position.Value = sectorInstance.PositionList.Value.Random();
						break;
					default:
						Debug.LogError("Unrecognized Placement: " + sectorInstance.SpecifiedPlacement.Value + " in specified sector \"" + sectorInstance.Name.Value + "\", attempting to skip...");
						break;
				}
			}

			var uniqueSectorPositions = new List<Vector3Int>();

			foreach (var entry in allSectorPositions)
			{
				var position = entry.Value;
				if (uniqueSectorPositions.Any(p => p.x == position.x && p.y == position.y && p.z == position.z))
				{
					done(
						RequestResult.Failure(
							"Provided galaxy has multiple sector positions specified for ( " + position + " ) at least one is on specified sector \"" + entry.Key + "\""
						).Log(),
						null
					);
					return;
				}
				uniqueSectorPositions.Add(position);
			}

			// TODO: clone specified sector instances, set any positions, etc...
			model.Universe.Sectors.Value = specifiedSectorInstances;
			// ---

			foreach (var systemInstance in specifiedSectorInstances.SelectMany(s => s.Systems.Value))
			{
				if (!systemInstance.KeyValues.Get(KeyDefines.CelestialSystem.HasSpecifiedWaypoint)) continue;

				var waypoint = new WaypointModel();
				waypoint.SetLocation(systemInstance);
				waypoint.WaypointId.Value = systemInstance.KeyValues.Get(KeyDefines.CelestialSystem.SpecifiedWaypointId);
				waypoint.VisibilityState.Value = WaypointModel.VisibilityStates.Hidden;
				waypoint.Distance.Value = UniversePosition.Distance(model.Ship.Position.Value, systemInstance.Position.Value);

				waypoint.Name.Value = systemInstance.KeyValues.Get(KeyDefines.CelestialSystem.SpecifiedWaypointName);

				model.Waypoints.AddWaypoint(waypoint);
			}

			// -- Module Generation
			var moduleTypes = EnumExtensions.GetValues(ModuleTypes.Unknown);

			var defaultPowerProduction = (float) moduleTypes.Length;
			var defaultPowerConsumption = 1f;

			var moduleConstraints = new List<ModuleService.ModuleConstraint>();

			foreach (var moduleType in moduleTypes)
			{
				var current = ModuleService.ModuleConstraint.Default;
				current.ValidTypes = new[] {moduleType};

				switch (moduleType)
				{
					case ModuleTypes.PowerProduction:
						current.PowerProduction = FloatRange.Constant(defaultPowerProduction);
						break;
					default:
						current.PowerConsumption = FloatRange.Constant(defaultPowerConsumption);
						break;
				}

				moduleConstraints.Add(current);
			}

			model.Context.ModuleService.GenerateModules(
				moduleConstraints.ToArray(),
				generateModulesResult =>
				{
					OnGenerateModules(
						generateModulesResult,
						instructions,
						model,
						done
					);
				}
			);
		}

		void OnGenerateModules(
			ResultArray<ModuleModel> result,
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

			model.Ship.Modules.Value = new ModuleStatistics(result.Payloads);

			SetContext(instructions, model, done);
		}

		void SetContext(LoadInstructions instructions, GameModel model, Action<RequestResult, GameModel> done)
		{
			// By this point the galaxy and target galaxy should already be set.
			// Additionally, begin, end, specified sectors, and waypoints should be defined.

			model.Context.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(model.ToolbarSelection.Value, false, ToolbarSelectionRequest.Sources.Player);

			foreach (var developerView in instructions.DeveloperViewsEnabled) model.Context.DeveloperViewsEnabled.Push(developerView);

			model.Context.SetCurrentSystem(universeService.GetSystem(model.Context.Galaxy, model.Universe, model.Ship.Position.Value, model.Ship.SystemIndex.Value));

			if (instructions.IsFirstLoad || model.TransitHistory.Count == 1)
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
						// TODO: This shouldn't be stored here... Maybe it should be done by the rules encounter? Or from the galaxy data?
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

			model.Context.TransitHistoryLineDistance.Value = Defaults.TransitHistoryLineDistance;
			model.Context.TransitHistoryLineCount.Value = Defaults.TransitHistoryLineCount;

			modelMediator.Save(model, saveResult => OnGameSaved(saveResult, instructions, model, done));
		}
		
		void OnGameSaved(
			ModelResult<GameModel> result,
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
		}
		#endregion
	}
}