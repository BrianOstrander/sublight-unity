using System.Linq;

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
		GameModel model;
		ShipModel ship;

		SaveStateBlock saveState = SaveStateBlock.Savable();
		public readonly ListenerProperty<SaveStateBlock> SaveState;

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

		#region Read Only Listeners
		UniverseScaleModel activeScale;
		ListenerProperty<UniverseScaleModel> activeScaleListener;
		public readonly ReadonlyProperty<UniverseScaleModel> ActiveScale;

		SystemModel currentSystem;
		readonly ListenerProperty<SystemModel> currentSystemListener;
		public ReadonlyProperty<SystemModel> CurrentSystem;
		#endregion

		#region Models
		public GalaxyInfoModel Galaxy { get; set; }
		public GalaxyInfoModel GalaxyTarget { get; set; }
		#endregion

		public GameContextModel(
			GameModel model,
			ShipModel ship
		)
		{
			this.model = model;
			this.ship = ship;

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

			CelestialSystemStateLastSelected = new ListenerProperty<CelestialSystemStateBlock>(value => celestialSystemStateLastSelected = value, () => celestialSystemStateLastSelected);

			TransitStateRequest = new ListenerProperty<TransitStateRequest>(value => transitStateRequest = value, () => transitStateRequest);
			TransitState = new ListenerProperty<TransitState>(value => transitState = value, () => transitState);

			ToolbarSelectionRequest = new ListenerProperty<ToolbarSelectionRequest>(value => toolbarSelectionRequest = value, () => toolbarSelectionRequest);

			ActiveScale = new ReadonlyProperty<UniverseScaleModel>(value => activeScale = value, () => activeScale, out activeScaleListener);
			foreach (var currScale in EnumExtensions.GetValues(UniverseScales.Unknown).Select(model.GetScale))
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
			foreach (var currScale in EnumExtensions.GetValues(UniverseScales.Unknown).Select(model.GetScale))
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
		}
		#endregion
	}
}