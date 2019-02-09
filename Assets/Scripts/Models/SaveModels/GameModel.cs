using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	/// <summary>
	/// All data that is serialized about the game.
	/// </summary>
	public class GameModel : SaveModel
	{
		#region Dynamic Serialized Values
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

		[JsonProperty] ShipModel ship;
		/// <summary>
		/// The game ship.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<ShipModel> Ship;

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore] public KeyValueListModel KeyValues { get { return keyValues; } }

		[JsonProperty] ToolbarSelections toolbarSelection;
		[JsonIgnore] public readonly ListenerProperty<ToolbarSelections> ToolbarSelection;

		[JsonProperty] bool toolbarLocking;
		[JsonIgnore] public readonly ListenerProperty<bool> ToolbarLocking;

		// TODO: Rethink if this should be serialized... actually I really really think it should not be...
		[JsonProperty] FocusTransform focusTransform;
		[JsonIgnore] public readonly ListenerProperty<FocusTransform> FocusTransform;
		#endregion

		#region Non-Dynamic Serialized Values
		[JsonProperty] UniverseScaleModel scaleSystem = UniverseScaleModel.Create(UniverseScales.System);
		[JsonProperty] UniverseScaleModel scaleLocal = UniverseScaleModel.Create(UniverseScales.Local);
		[JsonProperty] UniverseScaleModel scaleStellar = UniverseScaleModel.Create(UniverseScales.Stellar);
		[JsonProperty] UniverseScaleModel scaleQuadrant = UniverseScaleModel.Create(UniverseScales.Quadrant);
		[JsonProperty] UniverseScaleModel scaleGalactic = UniverseScaleModel.Create(UniverseScales.Galactic);
		[JsonProperty] UniverseScaleModel scaleCluster = UniverseScaleModel.Create(UniverseScales.Cluster);
		public UniverseScaleModel GetScale(UniverseScales scale)
		{
			switch (scale)
			{
				case UniverseScales.System: return scaleSystem;
				case UniverseScales.Local: return scaleLocal;
				case UniverseScales.Stellar: return scaleStellar;
				case UniverseScales.Quadrant: return scaleQuadrant;
				case UniverseScales.Galactic: return scaleGalactic;
				case UniverseScales.Cluster: return scaleCluster;
				default:
					Debug.LogError("Unrecognized scale: " + scale);
					return null;
			}
		}

		[JsonProperty] EncounterStateModel encounterState = new EncounterStateModel();
		[JsonIgnore] public EncounterStateModel EncounterState { get { return encounterState; } }

		[JsonProperty] WaypointCollectionModel waypointCollection = new WaypointCollectionModel();
		[JsonIgnore] public WaypointCollectionModel WaypointCollection { get { return waypointCollection; } }

		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonIgnore] public EncyclopediaListModel Encyclopedia { get { return encyclopedia; } }

		[JsonProperty] string galaxyId;
		[JsonIgnore] public string GalaxyId { get { return galaxyId; } set { galaxyId = value; } }

		[JsonProperty] string galaxyTargetId;
		[JsonIgnore] public string GalaxyTargetId { get { return galaxyTargetId; } set { galaxyTargetId = value; } }

		[JsonProperty] UniverseModel universe;
		[JsonIgnore] public UniverseModel Universe { get { return universe; } set { universe = value; } }
		#endregion

		#region NonSerialized
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
			Ship = new ListenerProperty<ShipModel>(value => ship = value, () => ship);
			ToolbarSelection = new ListenerProperty<ToolbarSelections>(value => toolbarSelection = value, () => toolbarSelection);
			ToolbarLocking = new ListenerProperty<bool>(value => toolbarLocking = value, () => toolbarLocking);
			FocusTransform = new ListenerProperty<FocusTransform>(value => focusTransform = value, () => focusTransform);

			Context = new GameContextModel(this);
		}
	}
}