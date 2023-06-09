﻿using System;
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
		
		CameraTransformRequest cameraTransformAbsolute = CameraTransformRequest.Default;
		readonly ListenerProperty<CameraTransformRequest> cameraTransformAbsoluteListener;
		public readonly ReadonlyProperty<CameraTransformRequest> CameraTransformAbsolute;

		CameraTransformRequest cameraTransform = CameraTransformRequest.Default;
		public readonly ListenerProperty<CameraTransformRequest> CameraTransform;

		GridInputRequest gridInput = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Input);
		public readonly ListenerProperty<GridInputRequest> GridInput;

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

		CelestialSystemStateBlock celestialSystemState = CelestialSystemStateBlock.Default;
		ListenerProperty<CelestialSystemStateBlock> celestialSystemStateListener;
		public readonly ReadonlyProperty<CelestialSystemStateBlock> CelestialSystemState;

		KeyValueListModel transitKeyValues = new KeyValueListModel();
		ListenerProperty<KeyValueListModel> transitKeyValuesListener;
		public readonly ReadonlyProperty<KeyValueListModel> TransitKeyValues;

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

		//float transitHistoryDistance;
		///// <summary>
		///// The transit history distance in universe units.
		///// </summary>
		//public readonly ListenerProperty<float> TransitHistoryDistance;
		float transitHistoryLineDistance;
		/// <summary>
		/// The transit history distance in universe units.
		/// </summary>
		public readonly ListenerProperty<float> TransitHistoryLineDistance;
		//float transitHistoryDistance;
		///// <summary>
		///// The transit history distance in universe units.
		///// </summary>
		//public readonly ListenerProperty<float> TransitHistoryDistance;
		int transitHistoryLineCount;
		public readonly ListenerProperty<int> TransitHistoryLineCount;

		float transitTimeNormal;
		/// <summary>
		/// The fraction component of TransitTimeElapsed.
		/// </summary>
		public readonly ListenerProperty<float> TransitTimeNormal;

		float transitTimeElapsed;
		/// <summary>
		/// The player's game speeds up when in transit, this keeps track of the
		/// total elapsed time.
		/// </summary>
		/// <remarks>
		/// No idea what happens when this rolls over, so don't use it for
		/// mission critical stuff, lol.
		/// </remarks>
		public readonly ListenerProperty<float> TransitTimeElapsed;

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

		StackListModel<DeveloperViews> developerViewsEnabled = new StackListModel<DeveloperViews>();
		public StackListModel<DeveloperViews> DeveloperViewsEnabled { get { return developerViewsEnabled; } }

		GridModel grid = new GridModel();
		public GridModel Grid { get { return grid; } }
		#endregion

		#region Models
		public GamemodeInfoModel Gamemode { get; set; }

		public GalaxyInfoModel Galaxy { get; set; }
		public GalaxyInfoModel GalaxyTarget { get; set; }

		public KeyValueListener KeyValueListener { get; set; }
		public KeyValueListener CelestialSystemKeyValueListener { get; private set; }
		#endregion

		#region Events
		public Action<SystemModel> NavigationSelectionOutOfRange = ActionExtensions.GetEmpty<SystemModel>();
  		#endregion

        #region Services
        public IModuleService ModuleService { get; set; }
        #endregion
        
		public GameContextModel(GameModel model)
		{
			this.model = model;

			CameraTransformAbsolute = new ReadonlyProperty<CameraTransformRequest>(
				value => cameraTransformAbsolute = value,
				() => cameraTransformAbsolute,
				out cameraTransformAbsoluteListener
			);
			CameraTransform = new ListenerProperty<CameraTransformRequest>(
				value => cameraTransform = value,
				() => cameraTransform,
				OnCameraTransform
			);

			GridInput = new ListenerProperty<GridInputRequest>(value => gridInput = value, () => gridInput);

			ScaleLabelSystem = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelSystem = value, () => scaleLabelSystem);
			ScaleLabelLocal = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelLocal = value, () => scaleLabelLocal);
			ScaleLabelStellar = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelStellar = value, () => scaleLabelStellar);
			ScaleLabelQuadrant = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelQuadrant = value, () => scaleLabelQuadrant);
			ScaleLabelGalactic = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelGalactic = value, () => scaleLabelGalactic);
			ScaleLabelCluster = new ListenerProperty<UniverseScaleLabelBlock>(value => scaleLabelCluster = value, () => scaleLabelCluster);
			GridScaleOpacity = new ListenerProperty<float>(value => gridScaleOpacity = value, () => gridScaleOpacity);

			CelestialSystemStateLastSelected = new ListenerProperty<CelestialSystemStateBlock>(value => celestialSystemStateLastSelected = value, () => celestialSystemStateLastSelected);
			CelestialSystemState = new ReadonlyProperty<CelestialSystemStateBlock>(
				value => celestialSystemState = value,
				() => celestialSystemState,
				out celestialSystemStateListener
			);

			TransitKeyValues = new ReadonlyProperty<KeyValueListModel>(
				value => transitKeyValues = value,
				() => transitKeyValues,
				out transitKeyValuesListener
			);

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

			TransitHistoryLineDistance = new ListenerProperty<float>(value => transitHistoryLineDistance = value, () => transitHistoryLineDistance);
			TransitHistoryLineCount = new ListenerProperty<int>(value => transitHistoryLineCount = value, () => transitHistoryLineCount);

			TransitTimeNormal = new ListenerProperty<float>(value => transitTimeNormal = value, () => transitTimeNormal);
			TransitTimeElapsed = new ListenerProperty<float>(value => transitTimeElapsed = value, () => transitTimeElapsed);
		}

		#region Events
		void OnCameraTransform(CameraTransformRequest request)
		{
			cameraTransformAbsoluteListener.Value = new CameraTransformRequest(
				request.State,
				request.Transform,
				request.Yaw ?? cameraTransformAbsoluteListener.Value.Yaw,
				request.Pitch ?? cameraTransformAbsoluteListener.Value.Pitch,
				request.Radius ?? cameraTransformAbsoluteListener.Value.Radius,
				request.Duration,
				request.Done
			);
		}

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
		public void SetCelestialSystemState(CelestialSystemStateBlock block)
		{
			celestialSystemStateListener.Value = block;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
				case CelestialSystemStateBlock.States.Selected:
					CelestialSystemStateLastSelected.Value = block;
					model.KeyValues.Set(
						KeyDefines.Game.NavigationSelectionName,
						block.State == CelestialSystemStateBlock.States.Selected ? block.System.Name.Value : null
					);
					model.KeyValues.Set(
						KeyDefines.Game.NavigationSelection,
						block.State == CelestialSystemStateBlock.States.Selected ? block.System.ShrunkPosition : null
					);
					break;
			}

			if (block.State == CelestialSystemStateBlock.States.Selected)
			{
				// TODO: I don't think I need to do this...
				transitKeyValuesListener.Value = model.KeyValues.Duplicate;
			}
		}

		public void SetCurrentSystem(SystemModel system)
		{
			currentSystemListener.Value = system;
			model.Ship.SystemIndex.Value = system == null ? -1 : system.Index.Value;

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