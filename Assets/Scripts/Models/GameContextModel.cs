using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	/// <summary>
	/// Data relating to the GameModel, but shouldn't be serialized.
	/// </summary>
	/// <remarks>
	/// All data in this class should be initialized before a game is played.
	/// </remarks>
	public class GameContextModel : Model
	{
#pragma warning disable CS0414 // The private field is assigned but its value is never used.
		GameModel model;
#pragma warning restore CS0414 // The private field is assigned but its value is never used.
		ShipModel ship;

		CameraTransformRequest cameraTransform = CameraTransformRequest.Default;
		public readonly ListenerProperty<CameraTransformRequest> CameraTransform;

		GridInputRequest gridInput = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Input);
		public readonly ListenerProperty<GridInputRequest> GridInput;

		CelestialSystemStateBlock celestialSystemState = CelestialSystemStateBlock.Default;
		public readonly ListenerProperty<CelestialSystemStateBlock> CelestialSystemState;

		UniverseScaleLabelBlock scaleLabelSystem = UniverseScaleLabelBlock.Default;
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelSystem;

		UniverseScaleLabelBlock scaleLabelLocal = UniverseScaleLabelBlock.Default;
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelLocal;

		UniverseScaleLabelBlock scaleLabelStellar = UniverseScaleLabelBlock.Default;
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelStellar;

		UniverseScaleLabelBlock scaleLabelQuadrant = UniverseScaleLabelBlock.Default;
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelQuadrant;

		UniverseScaleLabelBlock scaleLabelGalactic = UniverseScaleLabelBlock.Default;
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelGalactic;

		UniverseScaleLabelBlock scaleLabelCluster = UniverseScaleLabelBlock.Default;
		public readonly ListenerProperty<UniverseScaleLabelBlock> ScaleLabelCluster;

		float gridScaleOpacity;
		public readonly ListenerProperty<float> GridScaleOpacity;

		CelestialSystemStateBlock celestialSystemStateLastSelected = CelestialSystemStateBlock.Default;
		public ListenerProperty<CelestialSystemStateBlock> CelestialSystemStateLastSelected;

		TransitStateRequest transitStateRequest;
		public ListenerProperty<TransitStateRequest> TransitStateRequest;

		TransitState transitState;
		public ListenerProperty<TransitState> TransitState;

		ToolbarSelectionRequest toolbarSelectionRequest;
		public ListenerProperty<ToolbarSelectionRequest> ToolbarSelectionRequest;

		FocusTransform focusTransform;
		public readonly ListenerProperty<FocusTransform> FocusTransform;

		int pauseMenuBlockers;
		public readonly ListenerProperty<int> PauseMenuBlockers;

		UniverseScaleModel scaleSystem = UniverseScaleModel.Create(UniverseScales.System);
		UniverseScaleModel scaleLocal = UniverseScaleModel.Create(UniverseScales.Local);
		UniverseScaleModel scaleStellar = UniverseScaleModel.Create(UniverseScales.Stellar);
		UniverseScaleModel scaleQuadrant = UniverseScaleModel.Create(UniverseScales.Quadrant);
		UniverseScaleModel scaleGalactic = UniverseScaleModel.Create(UniverseScales.Galactic);
		UniverseScaleModel scaleCluster = UniverseScaleModel.Create(UniverseScales.Cluster);
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

		#region Read Only Listeners
		UniverseScaleModel activeScale;
		ListenerProperty<UniverseScaleModel> activeScaleListener;
		public readonly ReadonlyProperty<UniverseScaleModel> ActiveScale;

		SystemModel currentSystem;
		readonly ListenerProperty<SystemModel> currentSystemListener;
		public ReadonlyProperty<SystemModel> CurrentSystem;
		#endregion

		#region Read Only Models
		EncounterStateModel encounterState = new EncounterStateModel();
		public EncounterStateModel EncounterState { get { return encounterState; } }

		SaveBlockerListModel saveBlockers = new SaveBlockerListModel();
		public SaveBlockerListModel SaveBlockers { get { return saveBlockers; } }

		StackListModel<string> elapsedTimeBlockers = new StackListModel<string>();
		public StackListModel<string> ElapsedTimeBlockers { get { return elapsedTimeBlockers; } }
		#endregion

		#region Models
		public GalaxyInfoModel Galaxy { get; set; }
		public GalaxyInfoModel GalaxyTarget { get; set; }

		public KeyValueListener KeyValueListener { get; set; }
		public KeyValueListener CelestialSystemKeyValueListener { get; private set; }
		#endregion

		public GameContextModel(
			GameModel model,
			ShipModel ship
		)
		{
			this.model = model;
			this.ship = ship;

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

			CelestialSystemStateLastSelected = new ListenerProperty<CelestialSystemStateBlock>(value => celestialSystemStateLastSelected = value, () => celestialSystemStateLastSelected);

			TransitStateRequest = new ListenerProperty<TransitStateRequest>(value => transitStateRequest = value, () => transitStateRequest);
			TransitState = new ListenerProperty<TransitState>(value => transitState = value, () => transitState);

			ToolbarSelectionRequest = new ListenerProperty<ToolbarSelectionRequest>(value => toolbarSelectionRequest = value, () => toolbarSelectionRequest);
			FocusTransform = new ListenerProperty<FocusTransform>(value => focusTransform = value, () => focusTransform);

			PauseMenuBlockers = new ListenerProperty<int>(value => pauseMenuBlockers = value, () => pauseMenuBlockers);

			ActiveScale = new ReadonlyProperty<UniverseScaleModel>(value => activeScale = value, () => activeScale, out activeScaleListener);
			foreach (var currScale in EnumExtensions.GetValues(UniverseScales.Unknown).Select(GetScale))
			{
				currScale.Opacity.Changed += OnScaleOpacity;
				if (activeScale == null || activeScale.Opacity.Value < currScale.Opacity.Value) activeScale = currScale;
			}

			CurrentSystem = new ReadonlyProperty<SystemModel>(value => currentSystem = value, () => currentSystem, out currentSystemListener);
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

		#region Utility
		public void SetCurrentSystem(SystemModel system)
		{
			currentSystemListener.Value = system;
			ship.SystemIndex.Value = system == null ? -1 : system.Index.Value;

			if (CelestialSystemKeyValueListener != null)
			{
				CelestialSystemKeyValueListener.UnRegister();
				CelestialSystemKeyValueListener = null;
			}

			if (system != null)
			{
				CelestialSystemKeyValueListener = new KeyValueListener(
					KeyValueTargets.CelestialSystem,
					system.KeyValues,
					App.KeyValues
				).Register();
			}
		}
		#endregion
	}
}