using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class GamePayload : IStatePayload
	{
		public GameModel Game;

		public Dictionary<FocusRequest.Focuses, IPresenterCloseShow[]> Focuses = new Dictionary<FocusRequest.Focuses, IPresenterCloseShow[]>();
		public KeyValueListener KeyValueListener;
	}

	public class GameState : State<GamePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Game }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(InitializeGame);
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
			new CameraSystemBodiesPresenter(game);
			new SystemBodyListPresenter(game);

			// Body presenters
			new CameraBodyPresenter(game);
			new BodyHookPresenter(game);

			// Encounter presenters
			new CameraEncounterPresenter(game);
			new ContainerEncounterLogPresenter(game);

			// Global presenters
			new PauseMenuPresenter();
			new GameLostPresenter(game);

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			var focusedSector = Payload.Game.Ship.Value.Position.Value.SystemZero;
			var wasSectorFocused = Payload.Game.FocusedSector.Value == focusedSector;
			Payload.Game.FocusedSector.Value = focusedSector;
			if (wasSectorFocused) OnFocusedSector(focusedSector);

			App.Callbacks.FocusRequest(Payload.Game.FocusRequest.Value.Duplicate(FocusRequest.States.Request));

			if (!DevPrefs.SkipExplanation)
			{
				App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.Explanation0, Strings.ExplanationTitle0, OnExplanation));
			}
		}

		void OnExplanation()
		{
			App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.Explanation1, Strings.ExplanationTitle1));
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
					Payload.Game.Ship.Value.Inventory.AllResources.Fuel.Value -= request.FuelConsumed;

					if (!travelDestination.Visited)
					{
						travelDestination.Visited.Value = true;
						App.Callbacks.FocusRequest(
							new SystemBodiesFocusRequest(
								travelDestination.Position
							)
						);
					}
					break;
			}
		}

		void OnSaveRequest(SaveRequest request)
		{
			switch (request.State)
			{
				case SaveRequest.States.Request:
					App.M.Save(Payload.Game, OnSave);
					break;
			}
		}

		void OnSave(SaveLoadRequest<GameModel> request)
		{
			if (request.Status != RequestStatus.Success) Debug.LogError("Error saving: " + request.Error);
			App.Callbacks.SaveRequest(new SaveRequest(SaveRequest.States.Complete));
		}
		#endregion
	}
}