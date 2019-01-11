using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class GameModel : SaveModel
	{
		#region Serialized
		[JsonProperty] int seed;
		[JsonProperty] RelativeDayTime relativeDayTime;
		[JsonProperty] ShipModel ship;
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonProperty] ToolbarSelections toolbarSelection;

		[JsonProperty] FocusTransform focusTransform;

		[JsonProperty] string galaxyId;
		[JsonProperty] string galaxyTargetId;
		[JsonProperty] UniverseModel universe;

		[JsonProperty] UniverseScaleModel scaleSystem = UniverseScaleModel.Create(UniverseScales.System);
		[JsonProperty] UniverseScaleModel scaleLocal = UniverseScaleModel.Create(UniverseScales.Local);
		[JsonProperty] UniverseScaleModel scaleStellar = UniverseScaleModel.Create(UniverseScales.Stellar);
		[JsonProperty] UniverseScaleModel scaleQuadrant = UniverseScaleModel.Create(UniverseScales.Quadrant);
		[JsonProperty] UniverseScaleModel scaleGalactic = UniverseScaleModel.Create(UniverseScales.Galactic);
		[JsonProperty] UniverseScaleModel scaleCluster = UniverseScaleModel.Create(UniverseScales.Cluster);

		[JsonProperty] EncounterStateModel encounterState = new EncounterStateModel();

		/// <summary>
		/// The game seed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		/// <summary>
		/// The day time.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<RelativeDayTime> RelativeDayTime;
		/// <summary>
		/// The game ship.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<ShipModel> Ship;

		[JsonIgnore]
		public readonly ListenerProperty<ToolbarSelections> ToolbarSelection;

		[JsonIgnore]
		public readonly ListenerProperty<FocusTransform> FocusTransform;

		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		[JsonIgnore]
		public UniverseModel Universe
		{
			get { return universe; }
			set { universe = value; }
		}

		[JsonIgnore]
		public string GalaxyId
		{
			get { return galaxyId; }
			set { galaxyId = value; }
		}

		[JsonIgnore]
		public string GalaxyTargetId
		{
			get { return galaxyTargetId; }
			set { galaxyTargetId = value; }
		}

		[JsonIgnore]
		public GalaxyInfoModel Galaxy { get; set; }
		[JsonIgnore]
		public GalaxyInfoModel GalaxyTarget { get; set; }

		[JsonIgnore]
		public EncounterStateModel EncounterState { get { return encounterState; } }
		#endregion

		#region NonSerialized
		SaveStateBlock saveState = SaveStateBlock.Savable();
		CameraTransformRequest cameraTransform = CameraTransformRequest.Default;
		GridInputRequest gridInput = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Input);
		CelestialSystemStateBlock celestialSystemState = CelestialSystemStateBlock.Default;
		UniverseScaleLabelBlock scaleLabelSystem = UniverseScaleLabelBlock.Default;
		UniverseScaleLabelBlock scaleLabelLocal = UniverseScaleLabelBlock.Default;
		UniverseScaleLabelBlock scaleLabelStellar = UniverseScaleLabelBlock.Default;
		UniverseScaleLabelBlock scaleLabelQuadrant = UniverseScaleLabelBlock.Default;
		UniverseScaleLabelBlock scaleLabelGalactic = UniverseScaleLabelBlock.Default;
		UniverseScaleLabelBlock scaleLabelCluster = UniverseScaleLabelBlock.Default;
		float gridScaleOpacity;

		[JsonIgnore]
		public readonly ListenerProperty<SaveStateBlock> SaveState;
		[JsonIgnore]
		public readonly ListenerProperty<CameraTransformRequest> CameraTransform;
		[JsonIgnore]
		public readonly ListenerProperty<GridInputRequest> GridInput;
		[JsonIgnore]
		public readonly ListenerProperty<CelestialSystemStateBlock> CelestialSystemState;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelLocal;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelStellar;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelQuadrant;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelGalactic;
		[JsonIgnore]
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelCluster;
		[JsonIgnore]
		public readonly ListenerProperty<float> GridScaleOpacity;

		UniverseScaleModel activeScale;
		ListenerProperty<UniverseScaleModel> activeScaleListener;
		[JsonIgnore]
		public readonly ReadonlyProperty<UniverseScaleModel> ActiveScale;

		CelestialSystemStateBlock celestialSystemStateLastSelected = CelestialSystemStateBlock.Default;
		[JsonIgnore]
		public ListenerProperty<CelestialSystemStateBlock> CelestialSystemStateLastSelected;

		TransitStateRequest transitStateRequest;
		[JsonIgnore]
		public ListenerProperty<TransitStateRequest> TransitStateRequest;
		TransitState transitState;
		[JsonIgnore]
		public ListenerProperty<TransitState> TransitState;
		#endregion

		public GameModel()
		{
			SaveType = SaveTypes.Game;
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			RelativeDayTime = new ListenerProperty<RelativeDayTime>(value => relativeDayTime = value, () => relativeDayTime);
			Ship = new ListenerProperty<ShipModel>(value => ship = value, () => ship);
			ToolbarSelection = new ListenerProperty<ToolbarSelections>(value => toolbarSelection = value, () => toolbarSelection);
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
		#endregion

		#region Utility
		[JsonIgnore]
		public EncyclopediaListModel Encyclopedia { get { return encyclopedia; } }

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
		#endregion

		#region Events
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