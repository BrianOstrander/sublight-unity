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

		// TODO: Rethink if this should be serialized... actually I really really think it should not be...
		[JsonProperty] FocusTransform focusTransform;
		[JsonIgnore] public readonly ListenerProperty<FocusTransform> FocusTransform;
		#endregion

		#region Serialized Models
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

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore] public KeyValueListModel KeyValues { get { return keyValues; } }

		[JsonProperty] ShipModel ship = new ShipModel();
		[JsonIgnore] public ShipModel Ship { get { return ship; } }

		[JsonProperty] WaypointCollectionModel waypointCollection = new WaypointCollectionModel();
		[JsonIgnore] public WaypointCollectionModel WaypointCollection { get { return waypointCollection; } }

		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonIgnore] public EncyclopediaListModel Encyclopedia { get { return encyclopedia; } }

		[JsonProperty] TransitHistoryModel transitHistory = new TransitHistoryModel();
		[JsonIgnore] public TransitHistoryModel TransitHistory { get { return transitHistory; } }

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
			FocusTransform = new ListenerProperty<FocusTransform>(value => focusTransform = value, () => focusTransform);

			Context = new GameContextModel(this, Ship);
		}
	}
}