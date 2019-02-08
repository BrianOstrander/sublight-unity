using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class GameModel : SaveModel
	{
		#region Serialized
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

		[JsonProperty] FocusTransform focusTransform;
		[JsonIgnore] public readonly ListenerProperty<FocusTransform> FocusTransform;

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

		[JsonIgnore] public GalaxyInfoModel Galaxy { get; set; }
		[JsonIgnore] public GalaxyInfoModel GalaxyTarget { get; set; }
		#endregion

		#region NonSerialized
		// TODO: Figure out if this all should be moved to the payload, or some other model...
		SaveStateBlock saveState = SaveStateBlock.Savable();
		[JsonIgnore] public readonly ListenerProperty<SaveStateBlock> SaveState;

		CameraTransformRequest cameraTransform = CameraTransformRequest.Default;
		[JsonIgnore] public readonly ListenerProperty<CameraTransformRequest> CameraTransform;

		GridInputRequest gridInput = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Input);
		[JsonIgnore] public readonly ListenerProperty<GridInputRequest> GridInput;

		CelestialSystemStateBlock celestialSystemState = CelestialSystemStateBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<CelestialSystemStateBlock> CelestialSystemState;

		UniverseScaleLabelBlock scaleLabelSystem = UniverseScaleLabelBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelSystem;

		UniverseScaleLabelBlock scaleLabelLocal = UniverseScaleLabelBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelLocal;

		UniverseScaleLabelBlock scaleLabelStellar = UniverseScaleLabelBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelStellar;

		UniverseScaleLabelBlock scaleLabelQuadrant = UniverseScaleLabelBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelQuadrant;

		UniverseScaleLabelBlock scaleLabelGalactic = UniverseScaleLabelBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelGalactic;

		UniverseScaleLabelBlock scaleLabelCluster = UniverseScaleLabelBlock.Default;
		[JsonIgnore] public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelCluster;

		float gridScaleOpacity;
		[JsonIgnore] public readonly ListenerProperty<float> GridScaleOpacity;

		UniverseScaleModel activeScale;
		ListenerProperty<UniverseScaleModel> activeScaleListener;
		[JsonIgnore] public readonly ReadonlyProperty<UniverseScaleModel> ActiveScale;

		CelestialSystemStateBlock celestialSystemStateLastSelected = CelestialSystemStateBlock.Default;
		[JsonIgnore] public ListenerProperty<CelestialSystemStateBlock> CelestialSystemStateLastSelected;

		TransitStateRequest transitStateRequest;
		[JsonIgnore] public ListenerProperty<TransitStateRequest> TransitStateRequest;

		TransitState transitState;
		[JsonIgnore] public ListenerProperty<TransitState> TransitState;

		ToolbarSelectionRequest toolbarSelectionRequest;
		[JsonIgnore] public ListenerProperty<ToolbarSelectionRequest> ToolbarSelectionRequest;
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

			SaveState = new ListenerProperty<SaveStateBlock>(value => saveState = value, () => saveState);
			CameraTransform = new ListenerProperty<CameraTransformRequest>(value => cameraTransform = value, () => cameraTransform);
			GridInput = new ListenerProperty<GridInputRequest>(value => gridInput = value, () => gridInput);
			CelestialSystemState = new ListenerProperty<CelestialSystemStateBlock>(value => celestialSystemState = value, () => celestialSystemState, OnCelestialSystemState);

			ScaleLabelSystem = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelSystem = value, () => scaleLabelSystem);
			ScaleLabelLocal = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelLocal = value, () => scaleLabelLocal);
			ScaleLabelStellar = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelStellar = value, () => scaleLabelStellar);
			ScaleLabelQuadrant = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelQuadrant = value, () => scaleLabelQuadrant);
			ScaleLabelGalactic = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelGalactic = value, () => scaleLabelGalactic);
			ScaleLabelCluster = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelCluster = value, () => scaleLabelCluster);
			GridScaleOpacity = new ListenerProperty<float>(value => gridScaleOpacity = value, () => gridScaleOpacity);

			ActiveScale = new ReadonlyProperty<UniverseScaleModel>(value => activeScale = value, () => activeScale, out activeScaleListener);
			foreach (var currScale in EnumExtensions.GetValues(UniverseScales.Unknown).Select(GetScale))
			{
				currScale.Opacity.Changed += OnScaleOpacity;
				if (activeScale == null || activeScale.Opacity.Value < currScale.Opacity.Value) activeScale = currScale;
			}

			CelestialSystemStateLastSelected = new ListenerProperty<CelestialSystemStateBlock>(value => celestialSystemStateLastSelected = value, () => celestialSystemStateLastSelected);

			TransitStateRequest = new ListenerProperty<TransitStateRequest>(value => transitStateRequest = value, () => transitStateRequest);
			TransitState = new ListenerProperty<TransitState>(value => transitState = value, () => transitState);

			ToolbarSelectionRequest = new ListenerProperty<ToolbarSelectionRequest>(value => toolbarSelectionRequest = value, () => toolbarSelectionRequest);
		}

		#region Events
		void OnScaleOpacity(float opacity)
		{
			var newHighestOpacityScale = activeScale;
			foreach (var currScale in EnumExtensions.GetValues(UniverseScales.Unknown).Select(GetScale))
			{
				if (newHighestOpacityScale.Opacity.Value < currScale.Opacity.Value) newHighestOpacityScale = currScale;
			}
			activeScaleListener.Value = newHighestOpacityScale;
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
				case CelestialSystemStateBlock.States.Selected:
					CelestialSystemStateLastSelected.Value = block;
					break;
			}
		}
		#endregion
	}
}