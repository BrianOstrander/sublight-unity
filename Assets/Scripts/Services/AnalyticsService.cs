using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class AnalyticsService
	{
		public static class ScreenNames
		{
			public const string MainMenu = "main_menu";
			public const string Preferences = "preferences";
			public const string LearnMore = "learn_more";
			public const string Changelog = "changelog";

			public const string Navigation = "navigation";
			public const string Logistics = "logistics";
			public const string Communication = "communication";
			public const string Encyclopedia = "encyclopedia";

			public const string PauseMenu = "pause_menu";
		}

		static class StateNames
		{
			public const string HomeState = "home";
			public const string GameState = "game";
		}

		static class GameFields
		{
			public const string Id = "game_id";
			public const string Seed = "seed";
			public const string GalaxyId = "galaxy_id";
			public const string GalaxyTargetId = "galaxy_target_id";
			public const string UniverseSeed = "universe_seed";
			public const string Condition = "complete_condition";
			public const string ConditionTitle = "complete_condition_title";

			public const string BeginPosition = "begin_position";
			public const string EndPosition = "end_position";
			public const string CurrentPosition = "current_position";
		}

		static class EncounterFields
		{
			public const string Name = "encounter_name";
			public const string Id = "encounter_id";
		}

		static class ScreenFields
		{
			public const string State = "state";
		}

		static class TransitFields
		{
			public const string GalaxySize = "galaxy_size";
			public const string Position = "position";
			public const string Velocity = "velocity";
			public const string ShipYears = "ship_years";
			public const string SystemName = "system_name";
		}

		CallbackService callbacks;

		StateMachine.States lastState;

		public AnalyticsService(
			CallbackService callbacks
		)
		{
			if (callbacks == null) throw new NullReferenceException("callbacks");

			this.callbacks = callbacks;
		}

		public void Initialize(
			Action<RequestStatus> done,
			StateMachine.States state
		)
		{
			lastState = state;

			callbacks.StateChange += OnStateChange;

			done(RequestStatus.Success);
		}

		#region Events
		void OnStateChange(StateChange stateChange)
		{
			switch (stateChange.Event)
			{
				case StateMachine.Events.Begin:
					StateChange(stateChange.State);
					break;
			}
		}
		#endregion

		#region Private Events
		void StateChange(StateMachine.States state)
		{
			AnalyticsEvent.Custom(
				"state_change",
				new Dictionary<string, object>
				{
					{ "state", state },
					{ "from_state",  lastState }
				}
			);

			lastState = state;
		}
		#endregion

		#region Public Events
		public void GameStart(
			GameModel gameModel
		)
		{
			var eventData = new Dictionary<string, object>
			{
				{ GameFields.Id, gameModel.GameId.Value},
				{ GameFields.Seed, gameModel.Seed.Value },
				{ GameFields.GalaxyId, gameModel.GalaxyId },
				{ GameFields.GalaxyTargetId, gameModel.GalaxyTargetId },
				{ GameFields.UniverseSeed, gameModel.Universe.Seed.Value }
			};

			AnalyticsEvent.GameStart(
				ApplyWaypoints(gameModel, eventData)
			);
		}

		public void GameOver(
			GameModel gameModel,
			EncounterEvents.GameComplete.Conditions condition,
			string title
		)
		{
			var eventData = new Dictionary<string, object>
			{
				{ GameFields.Id, gameModel.GameId.Value},

				{ GameFields.Condition, condition },
				{ GameFields.ConditionTitle, title }
			};

			AnalyticsEvent.GameOver(
				gameModel.Name.Value,
				ApplyWaypoints(gameModel, eventData)
			);
		}

		public void GameContinue(
			GameModel gameModel
		)
		{
			var eventData = new Dictionary<string, object>
			{
				{ GameFields.Id, gameModel.GameId.Value},
				{ GameFields.Seed, gameModel.Seed.Value },
				{ GameFields.GalaxyId, gameModel.GalaxyId },
				{ GameFields.GalaxyTargetId, gameModel.GalaxyTargetId },
				{ GameFields.UniverseSeed, gameModel.Universe.Seed.Value }
			};

			AnalyticsEvent.Custom(
				"game_continue",
				ApplyWaypoints(gameModel, eventData)
			);
		}

		public void EncounterBegin(
			EncounterInfoModel encounter
		)
		{
			AnalyticsEvent.Custom(
				"encounter_begin",
				new Dictionary<string, object>
				{
					{ EncounterFields.Id, encounter.EncounterId.Value },
					{ EncounterFields.Name, encounter.Name.Value }
				}
			);
		}

		public void EncounterEnd(
			EncounterInfoModel encounter
		)
		{
			AnalyticsEvent.Custom(
				"encounter_end",
				new Dictionary<string, object>
				{
					{ EncounterFields.Id, encounter.EncounterId.Value },
					{ EncounterFields.Name, encounter.Name.Value }
				}
			);
		}

		public void ScreenVisit(
			string screenName,
			Dictionary<string, object> properties = null
		)
		{
			if (properties == null)
			{
				properties = new Dictionary<string, object>
				{
					{ ScreenFields.State, lastState }
				};
			}
			else if (properties.ContainsKey(ScreenFields.State))
			{
				Debug.LogError("Cannot add ScreenVisit." + ScreenFields.State + " property, it was already defined");
			}
			else
			{
				properties.Add(ScreenFields.State, lastState);
			}

			AnalyticsEvent.ScreenVisit(
				screenName,
				properties
			);
		}

		public void Transit(
			GameModel gameModel,
			TransitState transit
		)
		{
			AnalyticsEvent.Custom(
				"transit",
				new Dictionary<string, object>
				{
					{ TransitFields.GalaxySize, gameModel.Context.Galaxy.GalaxySize.SectorInteger },
					{ TransitFields.Position, transit.EndSystem.Position.Value.Lossy },
					{ TransitFields.Velocity, transit.VelocityLightYearsMaximum },
					{ TransitFields.ShipYears, transit.RelativeTimeTotal.ShipTime.TotalYears },
					{ TransitFields.SystemName, transit.EndSystem.Name.Value }
				}
			);
		}
		#endregion

		#region Utility
		void GetWaypoints(
			GameModel gameModel,
			out WaypointModel begin,
			out WaypointModel end,
			out WaypointModel current
		)
		{
			begin = gameModel.Waypoints.Waypoints.Value.First(w => w.WaypointId.Value == WaypointIds.BeginSystem);
			end = gameModel.Waypoints.Waypoints.Value.First(w => w.WaypointId.Value == WaypointIds.EndSystem);
			current = gameModel.Waypoints.Waypoints.Value.First(w => w.WaypointId.Value == WaypointIds.Ship);
		}

		Dictionary<string, object> ApplyWaypoints(
			GameModel gameModel,
			Dictionary<string, object> result
		)
		{
			WaypointModel begin;
			WaypointModel end;
			WaypointModel current;

			GetWaypoints(gameModel, out begin, out end, out current);

			ApplyKey(result, GameFields.BeginPosition, begin.Location.Value.Position.Lossy);
			ApplyKey(result, GameFields.EndPosition, end.Location.Value.Position.Lossy);
			ApplyKey(result, GameFields.CurrentPosition, current.Location.Value.Position.Lossy);

			return result;
		}

		Dictionary<string, object> ApplyKey(
			Dictionary<string, object> result,
			string key,
			object value
		)
		{
			if (result.ContainsKey(key)) Debug.LogError("Cannot apply key, already an entry for " + key);
			else result.Add(key, value);

			return result;
		}
		#endregion
	}
}