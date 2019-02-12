using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	/// <summary>
	/// All data that is serialized about the game.
	/// </summary>
	public class GameModel : SaveModel
	{
		#region Serialized Values
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

		[JsonProperty] string galaxyId;
		[JsonIgnore] public string GalaxyId { get { return galaxyId; } set { galaxyId = value; } }

		[JsonProperty] string galaxyTargetId;
		[JsonIgnore] public string GalaxyTargetId { get { return galaxyTargetId; } set { galaxyTargetId = value; } }

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

			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			RelativeDayTime = new ListenerProperty<RelativeDayTime>(value => relativeDayTime = value, () => relativeDayTime);
			ToolbarSelection = new ListenerProperty<ToolbarSelections>(value => toolbarSelection = value, () => toolbarSelection);
			ToolbarLocking = new ListenerProperty<bool>(value => toolbarLocking = value, () => toolbarLocking);

			Context = new GameContextModel(this, Ship);
		}
	}
}