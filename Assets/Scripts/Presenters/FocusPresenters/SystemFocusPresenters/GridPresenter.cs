﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridPresenter : SystemFocusPresenter<IGridView>
	{
		const float Tiling = 10f;

		enum UniverseFocuses
		{
			Unknown = 0,
			None = 10,
			Ship = 20,
			GalacticOrigin = 30,
			ClusterOrigin = 40
		}

		enum TweenStates
		{
			Unknown = 0,
			Complete = 10,
			Zooming = 20,
			Nudging = 30
		}

		struct UnitMap
		{
			public float ZoomBegin;
			public float LightYearsPrevious;
			/// <summary>
			/// The light years per tile.
			/// </summary>
			public float LightYears;
			public float LightYearsNext;
			public UniverseScales Scale;
			public UniverseFocuses FocusFromZoomDown;
			public UniverseFocuses Focus;
			public UniverseFocuses FocusFromZoomUp;

			public float ZoomUpBase;
			public float ZoomUpDelta;
			public float ZoomDownBase;
			public float ZoomDownDelta;

			public UnitMap(
				float zoomBegin,
				UniverseScales scale,
				float lightYearsPrevious,
				float lightYears,
				float lightYearsNext,
				UniverseFocuses focusFromZoomDown,
				UniverseFocuses focus,
				UniverseFocuses focusFromZoomUp
			)
			{
				ZoomBegin = zoomBegin;
				Scale = scale;
				LightYearsPrevious = lightYearsPrevious;
				LightYearsNext = lightYearsNext;
				LightYears = lightYears;
				FocusFromZoomDown = focusFromZoomDown;
				Focus = focus;
				FocusFromZoomUp = focusFromZoomUp;

				ZoomUpBase = lightYearsPrevious / lightYears;
				ZoomUpDelta = 1f - ZoomUpBase;
				ZoomDownBase = lightYearsNext / lightYears;
				ZoomDownDelta = -(ZoomDownBase - 1f);
			}
		}

		GameModel model;
		GridInfoBlock info;
		UnitMap[] unitMaps;

		TweenStates tweenState;

		float animationDuration;
		float animationRemaining;
		Dictionary<UniverseScales, UniverseTransform> scaleTransformsOnBeginAnimation = new Dictionary<UniverseScales, UniverseTransform>();

		float scrollSignWhenLastAnimated;
		float scrollWhenLastAnimated;
		float scrollCooldownRemaining;

		bool wasDragging;
		bool isDragging;
		Gesture lastgesture;
		Vector3 unityPosOnDragBegin;
		Vector3 unityPosOnDragLast;
		UniverseTransform transformOnBeginDrag;

		UniversePosition universeOriginOnTransitPrepareInitialize;

		UniversePosition ShipPositionOnPlane
		{
			get
			{
				var position = model.Ship.Position.Value;
				return new UniversePosition(position.Sector, position.Local.NewY(0f));
			}
		}

		public GridPresenter(
			GameModel model,
			GridInfoBlock info
		)
		{
			this.model = model;
			this.info = info;

			var systemScale = 0.1f;
			var localScale = 125f;
			var stellarScale = 800f;
			var quadrantScale = 3200f;
			var galacticScale = 9600f;
			var clusterScale = 28800f;

			unitMaps = new UnitMap[]
			{
				new UnitMap(
					0f,
					UniverseScales.System,
					systemScale, systemScale, localScale,
					UniverseFocuses.Ship, UniverseFocuses.Ship, UniverseFocuses.Ship
				),
				new UnitMap(
					1f,
					UniverseScales.Local,
					systemScale, localScale, stellarScale,
					UniverseFocuses.Ship, UniverseFocuses.Ship, UniverseFocuses.Ship
				),
				new UnitMap(
					2f,
					UniverseScales.Stellar,
					localScale, stellarScale, quadrantScale,
					UniverseFocuses.Ship, UniverseFocuses.Ship, UniverseFocuses.Ship
				),
				new UnitMap(
					3f,
					UniverseScales.Quadrant,
					stellarScale, quadrantScale, galacticScale,
					UniverseFocuses.Ship, UniverseFocuses.Ship, UniverseFocuses.GalacticOrigin
				),
				new UnitMap(
					4f,
					UniverseScales.Galactic,
					quadrantScale, galacticScale, clusterScale,
					UniverseFocuses.Ship, UniverseFocuses.GalacticOrigin, UniverseFocuses.ClusterOrigin
				),
				new UnitMap(
					5f,
					UniverseScales.Cluster,
					galacticScale, clusterScale, clusterScale,
					UniverseFocuses.GalacticOrigin, UniverseFocuses.ClusterOrigin, UniverseFocuses.ClusterOrigin
				)
			};

			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.CurrentScrollGesture += OnCurrentScrollGesture;
			App.Callbacks.CurrentGesture += OnCurrentGesture;

			foreach (var unitMap in unitMaps)
			{
				var currTransform = model.Context.GetScale(unitMap.Scale);
				currTransform.TransformDefault.Value = DefineTransform(unitMap, UniversePosition.Zero, 1f);

			}

			model.Context.Grid.HazardOffset.Changed += OnGridHazardOffset;

			model.Ship.Position.Changed += OnShipPosition;
			model.Ship.Statistics.Changed += OnShipStatistics;
			OnShipPosition(model.Ship.Position.Value);

			model.Context.TransitKeyValues.Changed += OnTransitKeyValues;
			model.Context.CelestialSystemState.Changed += OnCelestialSystemState;

			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.CurrentScrollGesture -= OnCurrentScrollGesture;
			App.Callbacks.CurrentGesture -= OnCurrentGesture;

			model.Context.Grid.HazardOffset.Changed -= OnGridHazardOffset;

			model.Ship.Position.Changed -= OnShipPosition;
			model.Ship.Statistics.Changed -= OnShipStatistics;
			model.Context.TransitKeyValues.Changed -= OnTransitKeyValues;
			model.Context.CelestialSystemState.Changed -= OnCelestialSystemState;

			model.Context.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{
			View.Dragging = OnDragging;
			View.Click = OnClick;
			View.SetGridSelected(
				model.Context.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected ? GridStates.Selected : GridStates.Idle,
				true
			);

			View.GridHazardOffset = model.Context.Grid.HazardOffset.Value;
		}

		void SetGrid()
		{
			var activeScale = model.Context.ActiveScale.Value.Scale.Value;
			var result = new GridView.Grid[unitMaps.Length];

			for (var i = 0; i < unitMaps.Length; i++)
			{
				var curr = unitMaps[i];
				var isTarget = activeScale == curr.Scale;

				result[i] = AnimateGrid(
					curr,
					1f,
					isTarget,
					isTarget,
					false
				);
			}

			View.Grids = result;
		}

		TweenBlock<float> AnimateZoom(float progress, TweenBlock<float> tween)
		{
			if (!tween.IsPingPong)
			{
				var result = new GridView.Grid[unitMaps.Length];

				for (var i = 0; i < unitMaps.Length; i++)
				{
					var curr = unitMaps[i];
					var isTarget = Mathf.Approximately(curr.ZoomBegin, tween.End);

					result[i] = AnimateGrid(
						curr,
						progress,
						isTarget,
						isTarget || (Mathf.Approximately(curr.ZoomBegin, tween.Begin) && !Mathf.Approximately(progress, 1f)),
						true,
						tween.Transition == TweenTransitions.ToHigher
					);
				}

				View.Grids = result;
			}

			return tween.Duplicate(tween.Begin + ((tween.End - tween.Begin) * progress), progress);
		}

		GridView.Grid AnimateGrid(
			UnitMap unitMap,
			float progress,
			bool isTarget,
			bool isActive,
			bool isZooming = false,
			bool zoomingUp = false
		)
		{
			var scale = model.Context.GetScale(unitMap.Scale);
			var scaleTransform = scale.Transform.Value;

			if (isZooming)
			{
				var beginPosition = scaleTransformsOnBeginAnimation[scale.Scale.Value].UniverseOrigin;
				var endPosition = beginPosition;

				var focus = UniverseFocuses.Unknown;

				if (isTarget) focus = unitMap.Focus;
				else
				{
					focus = zoomingUp ? unitMap.FocusFromZoomUp : unitMap.FocusFromZoomDown;
				}

				switch (focus)
				{
					case UniverseFocuses.None: break;
					case UniverseFocuses.Ship: endPosition = ShipPositionOnPlane; break;
					case UniverseFocuses.GalacticOrigin: endPosition = model.Context.Galaxy.GalaxyOrigin; break;
					case UniverseFocuses.ClusterOrigin: endPosition = model.Context.Galaxy.ClusterOrigin; break;
					default:
						Debug.LogError("unrecognized focus: " + focus);
						break;
				}

				var currPosition = UniversePosition.Lerp(View.GetPositionCurve(zoomingUp).Evaluate(progress), beginPosition, endPosition);
				scaleTransform = scale.Transform.Value.Duplicate(currPosition);
			}

			var grid = new GridView.Grid();
			grid.ZoomingUp = zoomingUp;
			grid.IsTarget = isTarget;
			grid.IsActive = isActive;
			grid.Progress = progress;
			grid.RangeOrigin = scaleTransform.GetUnityPosition(model.Ship.Position.Value);
			grid.RangeRadius = scaleTransform.GetUnityScale(model.Ship.Statistics.Value.TransitRange);

			CelestialSystemStateBlock? targetBlock = null;
			var transitRangeNormal = 0f;
			switch (model.Context.TransitState.Value.State)
			{
				case TransitState.States.Complete:
					switch (model.Context.CelestialSystemStateLastSelected.Value.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							targetBlock = model.Context.CelestialSystemStateLastSelected.Value;
							break;
					}
					break;
				default:
					targetBlock = model.Context.CelestialSystemStateLastSelected.Value;
					var transitStep = model.Context.TransitState.Value.CurrentStep;
					switch (transitStep.Step)
					{
						case TransitState.Steps.Prepare: transitRangeNormal = 0f; break;
						case TransitState.Steps.Finalize: transitRangeNormal = 1f; break;
						case TransitState.Steps.Transit:
							transitRangeNormal = transitStep.Progress;
							break;
					}
					break;
			}

			if (targetBlock.HasValue)
			{
				grid.TargetRangeVisible = true;
				grid.TargetRangeOrigin = scaleTransform.GetUnityPosition(targetBlock.Value.Position);
				grid.TargetRangeRadius = scaleTransform.GetUnityScale(model.Ship.Statistics.Value.TransitRange);

				grid.RangeRadius = grid.RangeRadius + ((grid.TargetRangeRadius - grid.RangeRadius) * transitRangeNormal);
			}

			var zoomProgress = View.ZoomCurve.Evaluate(progress);
			var zoomScalar = 1f;

			if (grid.IsTarget)
			{
				if (grid.ZoomingUp) zoomScalar = unitMap.ZoomUpBase + (unitMap.ZoomUpDelta * zoomProgress);
				else zoomScalar = unitMap.ZoomDownBase + (unitMap.ZoomDownDelta * zoomProgress);
			}
			else
			{
				var zoomProgressInverse = 1f - zoomProgress;
				if (grid.ZoomingUp) zoomScalar = unitMap.ZoomDownBase + (unitMap.ZoomDownDelta * zoomProgressInverse);
				else zoomScalar = unitMap.ZoomUpBase + (unitMap.ZoomUpDelta * zoomProgressInverse);
			}

			grid.Tiling = Tiling * zoomScalar;

			var offset = scaleTransform.GetGridOffset(UniversePosition.ToUniverseDistance(unitMap.LightYears));
			grid.Offset = new Vector2(offset.x, offset.z);

			var alphaCurve = grid.IsTarget ? View.RevealScaleAlpha : View.HideScaleAlpha;

			grid.Alpha = grid.IsActive ? alphaCurve.Evaluate(progress) : 0f;

			scale.Opacity.Value = grid.Alpha;
			scale.Transform.Value = DefineTransform(unitMap, scaleTransform.UniverseOrigin, zoomScalar);

			return grid;
		}

		UniverseTransform DefineTransform(
			UnitMap unitMap,
			UniversePosition universeOrigin,
			float zoomScalar
		)
		{
			var unityUnitsPerTile = (Tiling * 0.5f * zoomScalar) / View.GridUnityRadius;
			var universeUnitsPerTile = UniversePosition.ToUniverseDistance(unitMap.LightYears);
			var universeUnitsPerUnityUnit = unityUnitsPerTile * universeUnitsPerTile;

			return new UniverseTransform(
				unitMap.Scale,
				View.GridUnityOrigin,
				View.GridUnityRadius,
				universeOrigin,
				Vector3.one * universeUnitsPerUnityUnit,
				Vector3.one * (1f / universeUnitsPerUnityUnit),
				Quaternion.identity
			);
		}

		protected override void OnTransitionActive(TransitionFocusRequest request, SetFocusTransition transition, SystemFocusDetails startDetails, SystemFocusDetails endDetails)
		{
			if (transition.Start.Enabled == transition.End.Enabled) return; // I added this without testing it, no idea if it will break things.
			// TODO: This needs to be called multiple times while focusing and I don't know why.
			if (transition.End.Enabled && (request.FirstActive || request.LastActive)) BeginZoom(model.Context.FocusTransform.Value.Zoom, true);
		}

		#region
		void OnUpdate(float delta)
		{
			if (!View.Visible) return;
			model.Context.Grid.HazardOffset.Value = (model.Context.Grid.HazardOffset.Value + (delta * View.GridHazardOffsetMultiplierCurve.Evaluate(model.Context.TransitState.Value.RelativeTimeScalar))) % 1f;

			OnCheckTween(model.Context.FocusTransform.Value, delta);

			OnCheckDragging(delta);
		}

		void OnGridHazardOffset(float offset)
		{
			if (!View.Visible) return;

			View.GridHazardOffset = offset;
		}

		void OnCheckTween(FocusTransform transform, float delta)
		{
			switch(tweenState)
			{
				case TweenStates.Complete:
					scrollCooldownRemaining = Mathf.Max(0f, scrollCooldownRemaining - delta);
					return;
				case TweenStates.Unknown:
					Debug.LogError("Tween state should already have been initialized with an instant update in the constructor");
					return;
			}

			animationRemaining = Mathf.Max(0f, animationRemaining - delta);
			var progress = (animationDuration - animationRemaining) / animationDuration;

			var zoomTween = transform.Zoom;
			var nudgeTween = transform.NudgeZoom;

			switch (tweenState)
			{
				case TweenStates.Zooming:
					zoomTween = AnimateZoom(progress, zoomTween);
					break;
				case TweenStates.Nudging:
					nudgeTween = AnimateZoom(progress, nudgeTween);
					break;
				default:
					Debug.LogError("Unrecognized state " + tweenState);
					return;
			}

			if (Mathf.Approximately(0f, animationRemaining))
			{
				tweenState = TweenStates.Complete;
				SetGrid(); // TODO: Find out why I need to do this... it should not have such problems...
			}

			model.Context.FocusTransform.Value = transform.Duplicate(
				zoomTween,
				nudgeTween
			);
		}

		void OnCurrentScrollGesture(ScrollGesture gesture)
		{
			if (!View.Visible || !View.Highlighted) return;

			var value = gesture.CurrentScaledByDelta.y;

			if (tweenState != TweenStates.Complete)
			{
				if (!Mathf.Approximately(scrollSignWhenLastAnimated, Mathf.Sign(value))) return;
				if (scrollWhenLastAnimated < Mathf.Abs(value)) scrollWhenLastAnimated = Mathf.Abs(value);
				return;
			}

			var falloff = scrollWhenLastAnimated * View.ScrollCooldownFalloff.Evaluate(1f - (scrollCooldownRemaining / View.ScrollCooldown));

			if (Mathf.Abs(value) < falloff) return;
			if (Mathf.Abs(value) < (View.NudgeThreshold * View.ZoomThreshold)) return;

			scrollSignWhenLastAnimated = Mathf.Sign(value);
			scrollWhenLastAnimated = Mathf.Abs(value);
			scrollCooldownRemaining = View.ScrollCooldown;

			var isUp = 0f < value;

			var targetZoom = model.Context.FocusTransform.Value.Zoom.Current;
			if (isUp) targetZoom = Mathf.Min(targetZoom + 1f, 5f);
			else targetZoom = Mathf.Max(0f, targetZoom - 1f);
			var minOrMaxReached = Mathf.Approximately(targetZoom, model.Context.FocusTransform.Value.Zoom.Current);

			if (!minOrMaxReached && View.ZoomThreshold < Mathf.Abs(value))
			{
				// We're scrolling.
				BeginZoom(TweenBlock.Create(model.Context.FocusTransform.Value.Zoom.Current, targetZoom));
			}
			else if (minOrMaxReached)
			{
				// We're nudging.
				targetZoom += isUp ? 1f : -1f;
				BeginNudge(TweenBlock.CreatePingPong(model.Context.FocusTransform.Value.Zoom.Current, targetZoom));
			}
		}

		void BeginZoom(TweenBlock<float> zoomTween, bool instant = false)
		{
			InitializeBegin(View.ZoomAnimationDuration);

			LanguageStringModel fromScaleName;
			LanguageStringModel toScaleName;
			Func<string> fromGetUnitCount;
			Func<string> toGetUnitCount;
			LanguageStringModel fromUnitType;
			LanguageStringModel toUnitType;
			
			var fromGrid = unitMaps.FirstOrDefault(u => Mathf.Approximately(u.ZoomBegin, zoomTween.Begin));
			var toGrid = unitMaps.FirstOrDefault(u => Mathf.Approximately(u.ZoomBegin, zoomTween.End));

			UniverseScales fromScale;
			info.GetScaleName(fromGrid.ZoomBegin, out fromScale, out fromScaleName);

			UniverseScales toScale;
			info.GetScaleName(toGrid.ZoomBegin, out toScale, out toScaleName);
			
			info.GetUnitModels(fromGrid.LightYears, info.LightYearUnit, out fromGetUnitCount, out fromUnitType);
			info.GetUnitModels(toGrid.LightYears, info.LightYearUnit, out toGetUnitCount, out toUnitType);

			var transform = model.Context.FocusTransform.Value.Duplicate(zoomTween, model.Context.FocusTransform.Value.NudgeZoom.DuplicateNoChange());

			transform.SetLanguage(
				fromScale,
				toScale,
				fromScaleName,
				toScaleName,
				fromGetUnitCount,
				toGetUnitCount,
				fromUnitType,
				toUnitType
			);

			tweenState = TweenStates.Zooming;

			OnCheckTween(transform, instant ? animationDuration : 0f);
		}

		void BeginNudge(TweenBlock<float> nudgeTween)
		{
			InitializeBegin(View.NudgeAnimationDuration);

			tweenState = TweenStates.Nudging;
			model.Context.FocusTransform.Value = model.Context.FocusTransform.Value.Duplicate(model.Context.FocusTransform.Value.Zoom.DuplicateNoChange(), nudgeTween);
		}

		void InitializeBegin(float duration)
		{
			animationDuration = duration;
			animationRemaining = duration;
			scaleTransformsOnBeginAnimation.Clear();

			var origin = ShipPositionOnPlane;
			var activeScale = model.Context.ActiveScale.Value;

			if (activeScale != null) origin = activeScale.Transform.Value.UniverseOrigin;

			foreach (var scale in EnumExtensions.GetValues(UniverseScales.Unknown))
			{
				scaleTransformsOnBeginAnimation.Add(scale, model.Context.GetScale(scale).Transform.Value.Duplicate(origin));
			}
		}

		void OnDragging(bool isDragging)
		{
			this.isDragging = isDragging;
		}

		void OnClick()
		{
			if (wasDragging || View.TransitionState != TransitionStates.Shown || tweenState != TweenStates.Complete || model.Context.CelestialSystemStateLastSelected.Value.State != CelestialSystemStateBlock.States.Selected) return;

			model.Context.SetCelestialSystemState(
				CelestialSystemStateBlock.UnSelect(model.Context.CelestialSystemStateLastSelected.Value.Position, model.Context.CelestialSystemStateLastSelected.Value.System)
          	);
		}

		void OnCurrentGesture(Gesture gesture) { lastgesture = gesture; }

		void OnCheckDragging(float delta)
		{
			if (!View.Visible || tweenState != TweenStates.Complete)
			{
				isDragging = false;
				wasDragging = false;
				return;
			}

			if (wasDragging != isDragging)
			{
				if (isDragging)
				{
					// start drag logic
					bool inRadius;
					var isValidDrag = View.ProcessDrag(Gesture.GetViewport(lastgesture.Begin), out unityPosOnDragBegin, out inRadius);

					if (!inRadius || !isValidDrag)
					{
						isDragging = false;
						wasDragging = false;
						return;
					}
					transformOnBeginDrag = model.Context.ActiveScale.Value.Transform.Value;
				}
				else
				{
					// end drag logic
					if (lastgesture.IsSecondary) App.Callbacks.CameraTransformRequest(CameraTransformRequest.InputComplete());
					else model.Context.GridInput.Value = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Input);
				}
				wasDragging = isDragging;
			}

			if (!isDragging) return;

			if (lastgesture.IsSecondary) OnCheckSecondaryDragging(delta);
			else OnCheckPrimaryDragging(delta);

			wasDragging = isDragging;
		}

		void OnCheckPrimaryDragging(float delta)
		{
			Vector3 currUnityDrag;
			bool currInRadius;
			var isValidDrag = View.ProcessDrag(Gesture.GetViewport(lastgesture.End), out currUnityDrag, out currInRadius);

			if (isValidDrag) unityPosOnDragLast = currUnityDrag;
			else currUnityDrag = unityPosOnDragLast;

			var offset = View.GridUnityOrigin + (unityPosOnDragBegin - currUnityDrag);
			var universePos = transformOnBeginDrag.GetUniversePosition(offset);

			model.Context.ActiveScale.Value.Transform.Value = model.Context.ActiveScale.Value.Transform.Value.Duplicate(universePos);

			SetGrid();

			model.Context.GridInput.Value = new GridInputRequest(GridInputRequest.States.Active, GridInputRequest.Transforms.Input);
		}

		void OnCheckSecondaryDragging(float delta)
		{
			var yaw = -lastgesture.DeltaSinceLastScaledByDelta.x;
			var pitch = -lastgesture.DeltaSinceLastScaledByDelta.y;
			App.Callbacks.CameraTransformRequest(CameraTransformRequest.Input(yaw, pitch));
		}

		void OnShipPosition(UniversePosition position)
		{
			foreach (var transformProperty in unitMaps.Select(u => model.Context.GetScale(u.Scale).TransformDefault))
			{
				transformProperty.Value = transformProperty.Value.Duplicate(ShipPositionOnPlane);
			}
		}

		void OnShipStatistics(ShipStatistics shipStatistics)
		{
			SetGrid();
		}

		void OnTransitKeyValues(KeyValueListModel transitKeyValues)
		{
			if (model.Context.TransitState.Value.State == TransitState.States.Complete) SetGrid();
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			if (!View.Visible) return;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
					View.SetGridSelected(GridStates.Idle);
					break;
				case CelestialSystemStateBlock.States.Selected:
					View.SetGridSelected(GridStates.Selected);
					break;
			}
		}

		void OnTransitState(TransitState transitState)
		{
			switch (transitState.State)
			{
				case TransitState.States.Request:
					model.Context.GridInput.Value = new GridInputRequest(GridInputRequest.States.Active, GridInputRequest.Transforms.Animation);
					break;
				case TransitState.States.Active:
					var universePos = transitState.CurrentPosition.NewLocal(transitState.CurrentPosition.Local.NewY(0f));

					switch (transitState.Step)
					{
						case TransitState.Steps.Prepare:
							if (transitState.CurrentStep.Initializing)
							{
								universeOriginOnTransitPrepareInitialize = model.Context.ActiveScale.Value.Transform.Value.UniverseOrigin;
							}
							var universeCenterPos = UniversePosition.Lerp(View.PositionCenterCurve.Evaluate(transitState.CurrentStep.Progress), universeOriginOnTransitPrepareInitialize, universePos);
							model.Context.ActiveScale.Value.Transform.Value = model.Context.ActiveScale.Value.Transform.Value.Duplicate(universeCenterPos);
							SetGrid();
							break;
						case TransitState.Steps.Transit:
							model.Context.ActiveScale.Value.Transform.Value = model.Context.ActiveScale.Value.Transform.Value.Duplicate(universePos);
							model.Ship.Position.Value = transitState.CurrentPosition;
							SetGrid();
							break;
						case TransitState.Steps.Finalize:
							if (transitState.CurrentStep.Initializing)
							{
								transitState.BeginSystem.Visited.Value = true;

								model.Context.ActiveScale.Value.Transform.Value = model.Context.ActiveScale.Value.Transform.Value.Duplicate(universePos);
								model.Ship.Position.Value = transitState.EndSystem.Position;

								model.Context.SetCurrentSystem(transitState.EndSystem);
								model.Context.SetCelestialSystemState(CelestialSystemStateBlock.UnSelect(model.Context.CelestialSystemState.Value.System.Position.Value, model.Context.CelestialSystemState.Value.System));

								SetGrid();
							}
							break;
					}

					model.Context.GridInput.Value = new GridInputRequest(GridInputRequest.States.Active, GridInputRequest.Transforms.Animation);
					break;
				case TransitState.States.Complete:
					model.Context.GridInput.Value = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Animation);
					break;
			}
		}
		#endregion
	}
}