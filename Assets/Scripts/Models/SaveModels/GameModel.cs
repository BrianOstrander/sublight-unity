using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	/// <summary>
	/// All data that is serialized about the game.
	/// </summary>
	public class GameModel : SaveModel
	{
		#region Serialized Values
		[JsonProperty] string name;
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] int seed;
		/// <summary>
		/// The game seed.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<int> Seed;

		[JsonProperty] RelativeDayTime relativeDayTime;
		/// <summary>
		/// The day time.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<RelativeDayTime> RelativeDayTime;

		[JsonProperty] ToolbarSelections toolbarSelection;
		[JsonIgnore] public readonly ListenerProperty<ToolbarSelections> ToolbarSelection;

		[JsonProperty] bool toolbarLocking;
		[JsonIgnore] public readonly ListenerProperty<bool> ToolbarLocking;

		[JsonProperty] EncounterResume encounterResume = SubLight.EncounterResume.Default;
		[JsonIgnore] public readonly ListenerProperty<EncounterResume> EncounterResume;

		[JsonProperty] EncounterTriggers[] encounterTriggers = new EncounterTriggers[0];
		[JsonIgnore] public readonly ListenerProperty<EncounterTriggers[]> EncounterTriggers;

		[JsonProperty] TimeSpan elapsedTime;
		[JsonIgnore] public readonly ListenerProperty<TimeSpan> ElapsedTime;

		[JsonProperty] GameSaveDetails saveDetails;
		[JsonIgnore] public readonly ListenerProperty<GameSaveDetails> SaveDetails;
		#endregion

		#region Serialized Models
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore] public KeyValueListModel KeyValues { get { return keyValues; } }

		[JsonProperty] ShipModel ship = new ShipModel();
		[JsonIgnore] public ShipModel Ship { get { return ship; } }

		[JsonProperty] WaypointListModel waypoints = new WaypointListModel();
		[JsonIgnore] public WaypointListModel Waypoints { get { return waypoints; } }

		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonIgnore] public EncyclopediaListModel Encyclopedia { get { return encyclopedia; } }

		[JsonProperty] TransitHistoryModel transitHistory = new TransitHistoryModel();
		[JsonIgnore] public TransitHistoryModel TransitHistory { get { return transitHistory; } }

		[JsonProperty] EncounterStatusListModel encounterStatuses = new EncounterStatusListModel();
		[JsonIgnore] public EncounterStatusListModel EncounterStatuses { get { return encounterStatuses; } }

		[JsonProperty] UniverseModel universe;
		/// <summary>
		/// Gets or sets the universe.
		/// </summary>
		/// <remarks>
		/// This should only be set upon creation of a new game by the
		/// UniverseService.
		/// </remarks>
		/// <value>The universe.</value>
		[JsonIgnore] public UniverseModel Universe { get { return universe; } set { universe = value; } }
		#endregion

		#region Serialized Model Ids
		[JsonProperty] string galaxyId;
		[JsonIgnore] public string GalaxyId { get { return galaxyId; } set { galaxyId = value; } }

		[JsonProperty] string galaxyTargetId;
		[JsonIgnore] public string GalaxyTargetId { get { return galaxyTargetId; } set { galaxyTargetId = value; } }

		[JsonProperty] string gamemodeId;
		[JsonIgnore] public string GamemodeId { get { return gamemodeId; } set { gamemodeId = value; } }
		#endregion

		#region Non Serialized Models
		/// <summary>
		/// Gets the context data, non-serialized information relating to game
		/// data.
		/// </summary>
		/// <remarks>
		/// This should be the only non-serialized data in this model, anything
		/// else should be inside the context.
		/// </remarks>
		/// <value>The context.</value>
		[JsonIgnore] public readonly GameContextModel Context;
		#endregion

		public GameModel()
		{
			SaveType = SaveTypes.Game;

			Name = new ListenerProperty<string>(value => name = value, () => name);
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			RelativeDayTime = new ListenerProperty<RelativeDayTime>(value => relativeDayTime = value, () => relativeDayTime);
			ToolbarSelection = new ListenerProperty<ToolbarSelections>(value => toolbarSelection = value, () => toolbarSelection);
			ToolbarLocking = new ListenerProperty<bool>(value => toolbarLocking = value, () => toolbarLocking);
			EncounterResume = new ListenerProperty<EncounterResume>(value => encounterResume = value, () => encounterResume);
			EncounterTriggers = new ListenerProperty<EncounterTriggers[]>(value => encounterTriggers = value, () => encounterTriggers);
			ElapsedTime = new ListenerProperty<TimeSpan>(value => elapsedTime = value, () => elapsedTime);
			SaveDetails = new ListenerProperty<GameSaveDetails>(value => saveDetails = value, () => saveDetails);

			Context = new GameContextModel(this, Ship);
		}
	}
}