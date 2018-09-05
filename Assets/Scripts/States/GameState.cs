using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Presenters;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class GamePayload : IStatePayload
	{
		public GameModel Game;

		public Dictionary<FocusRequest.Focuses, IPresenterCloseShow[]> Focuses = new Dictionary<FocusRequest.Focuses, IPresenterCloseShow[]>();
		public KeyValueListener KeyValueListener;
	}

	public partial class GameState : State<GamePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Game, SceneConstants.HoloRoom }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			//App.SM.PushBlocking(InitializeGame);
			App.SM.PushBlocking(InitializeFocusPresenters);
			App.SM.PushBlocking(InitializeFocus);
			App.SM.PushBreak();
		}

		void LoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.Load(result => done(), Scenes));
		}

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeCallbacks(Action done)
		{
			App.Callbacks.SaveRequest += OnSaveRequest;
			Payload.Game.TravelRequest.Changed += OnTravelRequest;
			App.Callbacks.FocusRequest += OnFocus;


			Payload.KeyValueListener = new KeyValueListener(KeyValueTargets.Game, Payload.Game.KeyValues, App.KeyValues);
			Payload.KeyValueListener.Register();

			done();
		}

		void InitializeGame(Action done)
		{
			//
			// --- Initial Callbacks --- 
			//
			// We do this so basic important values are cached in the 
			// CallbackService.

			var game = Payload.Game;

			game.FocusedSector.Changed += OnFocusedSector;

			var ship = game.Ship.Value;

			//UniversePosition
			App.Callbacks.DayTimeDelta(new DayTimeDelta(game.DayTime, game.DayTime));
			App.Callbacks.SystemHighlight(SystemHighlight.None);
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);

			//
			// --- Create Presenters --- 
			//
			// There may be some warnings from creating these without assigning,
			// but you can safely ignore those warnings since they register
			// themselves with the PresenterMediator.

			// System presenters
			new CameraSystemPresenter(game);

			Payload.Focuses.Add(
				FocusRequest.Focuses.Systems,
				new IPresenterCloseShow[] {
					new ShipRadiusPresenter(game),
					new DestructionOriginSystemPresenter(),
					new DestructionSystemPresenter(game),
					new EndDirectionSystemPresenter(game),
					new FuelSliderPresenter(game),
					new DestructionSpeedPresenter(game),
					new SpeedPresenter(game),
					new EndDistancePresenter(game),
					new ShipSystemPresenter(game),
					new EndSystemPresenter(game)
				}
			);
			new DetailSystemPresenter(game);
			new LineSystemPresenter(game);

			// System Bodies presenters
			new CameraSystemBodiesPresenter();
			new SystemBodyListPresenter(game);

			// Body presenters
			new CameraBodyPresenter();
			new BodyHookPresenter(game);

			// Encounter presenters
			new CameraEncounterPresenter();
			new ContainerEncounterLogPresenter(game);

			// Ship presenters
			new CameraShipPresenter();
			new ShipSlotsPresenter(game);

			// Encyclopedia presenters
			new CameraEncyclopediaPresenter();
			new EncyclopediaPresenter(game);

			// Global presenters
			new PauseMenuPresenter();
			new GameLostPresenter(game);

			done();
		}

		void InitializeFocusPresenters(Action done)
		{
			new HoloRoomFocusCameraPresenter();

			done();
		}

		void InitializeFocus(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Default(Focuses.Defaults, () => OnInializeFocusDefaults(done)));
		}

		void OnInializeFocusDefaults(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.System, done));
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			var focusedSector = Payload.Game.Ship.Value.Position.Value.SystemZero;
			var wasSectorFocused = Payload.Game.FocusedSector.Value == focusedSector;
			Payload.Game.FocusedSector.Value = focusedSector;
			if (wasSectorFocused) OnFocusedSector(focusedSector);

			if (Payload.Game.TravelRequest.Value.State == TravelRequest.States.Complete && !Payload.Game.Universe.Value.GetSystem(Payload.Game.TravelRequest.Value.Destination).Visited.Value)
			{
				OnTravelRequest(Payload.Game.TravelRequest);
			}
			else App.Callbacks.FocusRequest(Payload.Game.FocusRequest.Value.Duplicate(FocusRequest.States.Request));


		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);

			App.Callbacks.SaveRequest -= OnSaveRequest;
			Payload.Game.TravelRequest.Changed -= OnTravelRequest;
			App.Callbacks.FocusRequest -= OnFocus;
			Payload.Game.FocusedSector.Changed -= OnFocusedSector;
			App.Callbacks.ClearEscapables();

			Payload.KeyValueListener.UnRegister();

			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnLoadScenes);
		}

		void UnBind(Action done)
		{
			// All presenters will have their views closed and unbinded. Events
			// will also be unbinded.
			App.P.UnRegisterAll(done);
		}

		void UnLoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.UnLoad(result => done(), Scenes));
		}
		#endregion

		#region Events
		void OnFocus(FocusRequest focus)
		{
			Payload.Game.FocusRequest.Value = focus;

			switch (focus.State)
			{
				case FocusRequest.States.Request:
					App.Callbacks.FocusRequest(focus.Duplicate(FocusRequest.States.Active));
					return;
				case FocusRequest.States.Active:
					App.Callbacks.FocusRequest(focus.Duplicate(FocusRequest.States.Complete));
					return;
			}

			foreach (var key in Payload.Focuses.Keys)
			{
				var isShowing = key == focus.Focus;
				foreach (var value in Payload.Focuses[key])
				{
					if (isShowing) value.Show();
					else value.Close();
				}
			}

			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Systems:
					var systemFocus = focus as SystemsFocusRequest;
					Payload.Game.FocusedSector.Value = systemFocus.FocusedSector;
					break;
			}
		}

		void OnFocusedSector(UniversePosition position)
		{
			var oldSectors = Payload.Game.FocusedSectors.Value.ToList();
			var newSectors = new List<UniversePosition>();

			var minX = position.Sector.x - App.Preferences.SectorUnloadRadius;
			var maxX = position.Sector.x + App.Preferences.SectorUnloadRadius;
			var minZ = position.Sector.z - App.Preferences.SectorUnloadRadius;
			var maxZ = position.Sector.z + App.Preferences.SectorUnloadRadius;

			for (var x = minX; x <= maxX; x++)
			{
				for (var z = minZ; z <= maxZ; z++)
				{
					newSectors.Add(new UniversePosition(new Vector3(x, 0f, z), Vector3.zero));
				}
			}

			App.Callbacks.UniversePositionRequest(UniversePositionRequest.Request(position));
			Payload.Game.FocusedSectors.Value = newSectors.ToArray();

			foreach (var sectorPosition in newSectors.Where(s => !oldSectors.Contains(s)))
			{
				var sector = Payload.Game.Universe.Value.GetSector(sectorPosition);
				foreach (var system in sector.Systems.Value)
				{
					new SystemPresenter(Payload.Game, system).Show();
				}
			}
		}

		void OnTravelRequest(TravelRequest request)
		{
			Payload.Game.TravelRequest.Value = request;

			switch (request.State)
			{
				case TravelRequest.States.Complete:
					// Don't focus on end system.
					if (request.Destination == Payload.Game.EndSystem.Value) return;

					var travelDestination = Payload.Game.Universe.Value.GetSystem(request.Destination);

					if (!travelDestination.Visited) OnNonVisitedSystem(travelDestination);
					break;
			}
		}

		/// <summary>
		/// Called when we enter a system for the first time.
		/// </summary>
		/// <param name="system">System.</param>
		void OnNonVisitedSystem(SystemModel system)
		{
			system.Visited.Value = true;
			App.Encounters.AssignBestEncounter(OnAssignBestEncounter, Payload.Game, system);
		}

		void OnAssignBestEncounter(AssignBestEncounter result)
		{
			if (result.Status != RequestStatus.Success || !result.EncounterAssigned) return;

			if (result.Encounter.IsIntroduction && DevPrefs.SkipExplanation)
			{
				Debug.Log("Skipping Explanation");
				App.Callbacks.KeyValueRequest(KeyValueRequest.Set(KeyValueTargets.Game, "IntroductionShown", true));
				App.Callbacks.FocusRequest(Payload.Game.FocusRequest.Value.Duplicate(FocusRequest.States.Request));
				return;
			}

			switch (result.Encounter.Trigger.Value)
			{
				case EncounterTriggers.Automatic:
					OnAutomaticEncounter(result);
					break;
				case EncounterTriggers.BodyAlert:
					OnBodyAlertEncounter(result);
					break;
				default:
					Debug.LogError("Unrecognized Trigger: " + result.Encounter.Trigger.Value);
					break;
			}
		}

		void OnAutomaticEncounter(AssignBestEncounter result)
		{
			switch (Payload.Game.GetEncounterStatus(result.Encounter.EncounterId.Value).State)
			{
				case EncounterStatus.States.Seen:
				case EncounterStatus.States.NeverSeen:
					App.Callbacks.EncounterRequest(
						EncounterRequest.Request(
							Payload.Game,
							result.Encounter.EncounterId,
							result.System.Position
						)
					);
					break;
			}
		}

		void OnBodyAlertEncounter(AssignBestEncounter result)
		{
			// TODO: A more subtle alert, perhaps? something more fun?
			App.Callbacks.FocusRequest(
				new SystemBodiesFocusRequest(
					result.System.Position
				)
			);
		}

		void OnSaveRequest(SaveRequest request)
		{
			switch (request.State)
			{
				case SaveRequest.States.Request:
					if (!Payload.Game.SaveState.Value.CanSave)
					{
						App.Callbacks.SaveRequest(SaveRequest.Failure(request, Payload.Game.SaveState.Value.Reason));
						break;
					}
					Payload.Game.SaveState.Value = SaveStateBlock.NotSavable(Strings.CannotSaveReasons.CurrentlySaving);
					App.M.Save(Payload.Game, result => OnSave(result, request));
					break;
				case SaveRequest.States.Complete:
					if (request.Status != RequestStatus.Success) Debug.LogError("Unable to save game, request returned with status " + request.Status + " and error: " + request.Error);
					request.Done(request);
					break;
			}
		}

		void OnSave(SaveLoadRequest<GameModel> result, SaveRequest request)
		{
			Payload.Game.SaveState.Value = SaveStateBlock.Savable();

			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to save game, model mediator returned status "+result.Status+" and error: " + result.Error);
				App.Callbacks.SaveRequest(SaveRequest.Failure(request, result.Error));
				return;
			}
			App.Callbacks.SaveRequest(SaveRequest.Success(request));
		}
		#endregion
	}
}