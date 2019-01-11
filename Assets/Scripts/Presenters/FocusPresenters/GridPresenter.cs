using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridPresenter : FocusPresenter<IGridView, SystemFocusDetails>
	{
		const float Tiling = 8f;

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

		UniversePosition ShipPositionOnPlane
		{
			get
			{
				var position = model.Ship.Value.Position.Value;
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
			var localScale = 150f;
			var stellarScale = 1000f;
			var quadrantScale = 4000f;
			var galacticScale = 12000f;
			var clusterScale = 36000f;

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
				var currTransform = model.GetScale(unitMap.Scale);
				currTransform.TransformDefault.Value = DefineTransform(unitMap, UniversePosition.Zero, 1f);

			}

			model.Ship.Value.Position.Changed += OnShipPosition;
			OnShipPosition(model.Ship.Value.Position.Value);

			model.Ship.Value.Range.Changed += OnTravelRange;
			model.CelestialSystemState.Changed += OnCelestialSystemState;

			model.TransitState.Changed += OnTransitState;

			BeginZoom(model.FocusTransform.Value.Zoom, true);
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.CurrentScrollGesture -= OnCurrentScrollGesture;
			App.Callbacks.CurrentGesture -= OnCurrentGesture;

			model.Ship.Value.Position.Changed -= OnShipPosition;
			model.Ship.Value.Range.Changed -= OnTravelRange;
			model.CelestialSystemState.Changed -= OnCelestialSystemState;

			model.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{
			View.Dragging = OnDragging;
			View.DrawGizmos = OnDrawGizmos;
			View.SetGridSelected(model.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected, true);
			BeginZoom(model.FocusTransform.Value.Zoom, true);
		}

		void SetGrid()
		{
			var activeScale = model.ActiveScale.Value.Scale.Value;
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
			var scale = model.GetScale(unitMap.Scale);
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
					case UniverseFocuses.GalacticOrigin: endPosition = model.Galaxy.GalaxyOrigin; break;
					case UniverseFocuses.ClusterOrigin: endPosition = model.Galaxy.ClusterOrigin; break;
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
			grid.RangeOrigin = scaleTransform.GetUnityPosition(model.Ship.Value.Position.Value);
			grid.RangeRadius = scaleTransform.GetUnityScale(model.Ship.Value.Range.Value.Total);

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

			var currLightYearsInTile = zoomProgress * unitMap.LightYears;

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

		#region
		void OnUpdate(float delta)
		{
			OnCheckTween(model.FocusTransform.Value, delta);

			OnCheckDragging(delta);
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

			model.FocusTransform.Value = transform.Duplicate(
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

			var targetZoom = model.FocusTransform.Value.Zoom.Current;
			if (isUp) targetZoom = Mathf.Min(targetZoom + 1f, 5f);
			else targetZoom = Mathf.Max(0f, targetZoom - 1f);
			var minOrMaxReached = Mathf.Approximately(targetZoom, model.FocusTransform.Value.Zoom.Current);

			if (!minOrMaxReached && View.ZoomThreshold < Mathf.Abs(value))
			{
				// We're scrolling.
				BeginZoom(TweenBlock.Create(model.FocusTransform.Value.Zoom.Current, targetZoom));
			}
			else if (minOrMaxReached)
			{
				// We're nudging.
				targetZoom += isUp ? 1f : -1f;
				BeginNudge(TweenBlock.CreatePingPong(model.FocusTransform.Value.Zoom.Current, targetZoom));
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

			var transform = model.FocusTransform.Value.Duplicate(zoomTween, model.FocusTransform.Value.NudgeZoom.DuplicateNoChange());

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
			model.FocusTransform.Value = model.FocusTransform.Value.Duplicate(model.FocusTransform.Value.Zoom.DuplicateNoChange(), nudgeTween);
		}

		void InitializeBegin(float duration)
		{
			animationDuration = duration;
			animationRemaining = duration;
			scaleTransformsOnBeginAnimation.Clear();

			var origin = ShipPositionOnPlane;
			var activeScale = model.ActiveScale.Value;

			if (activeScale != null) origin = activeScale.Transform.Value.UniverseOrigin;

			foreach (var scale in EnumExtensions.GetValues(UniverseScales.Unknown))
			{
				scaleTransformsOnBeginAnimation.Add(scale, model.GetScale(scale).Transform.Value.Duplicate(origin));
			}
		}

		void OnDragging(bool isDragging)
		{
			this.isDragging = isDragging;
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
					transformOnBeginDrag = model.ActiveScale.Value.Transform.Value;
				}
				else
				{
					// end drag logic
					if (lastgesture.IsSecondary) App.Callbacks.CameraTransformRequest(CameraTransformRequest.InputComplete());
					else model.GridInput.Value = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Input);
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

			model.ActiveScale.Value.Transform.Value = model.ActiveScale.Value.Transform.Value.Duplicate(universePos);

			SetGrid();

			model.GridInput.Value = new GridInputRequest(GridInputRequest.States.Active, GridInputRequest.Transforms.Input);
		}

		void OnCheckSecondaryDragging(float delta)
		{
			var yaw = -lastgesture.DeltaSinceLastScaledByDelta.x;
			var pitch = -lastgesture.DeltaSinceLastScaledByDelta.y;
			App.Callbacks.CameraTransformRequest(CameraTransformRequest.Input(yaw, pitch));
		}

		void OnShipPosition(UniversePosition position)
		{
			foreach (var transformProperty in unitMaps.Select(u => model.GetScale(u.Scale).TransformDefault))
			{
				transformProperty.Value = transformProperty.Value.Duplicate(ShipPositionOnPlane);
			}
		}

		void OnTravelRange(TransitRange range)
		{
			if (tweenState == TweenStates.Complete) SetGrid();
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			if (!View.Visible) return;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
					View.SetGridSelected(false);
					break;
				case CelestialSystemStateBlock.States.Selected:
					View.SetGridSelected(true);
					break;
			}
		}

		void OnTransitState(TransitState transitState)
		{

			switch (transitState.State)
			{
				case TransitState.States.Request:
					model.GridInput.Value = new GridInputRequest(GridInputRequest.States.Active, GridInputRequest.Transforms.Animation);
					break;
				case TransitState.States.Active:
					var universePos = transitState.CurrentPosition.NewLocal(transitState.CurrentPosition.Local.NewY(0f));

					switch (transitState.Step)
					{
						case TransitState.Steps.Prepare:
							// Tween to universePos here!
							break;
						case TransitState.Steps.Transit:
							model.ActiveScale.Value.Transform.Value = model.ActiveScale.Value.Transform.Value.Duplicate(universePos);
							model.Ship.Value.Position.Value = transitState.CurrentPosition;
							SetGrid();
							break;
						case TransitState.Steps.Finalize:
							if (transitState.CurrentStep.Initializing)
							{
								transitState.BeginSystem.Visited.Value = true;

								model.ActiveScale.Value.Transform.Value = model.ActiveScale.Value.Transform.Value.Duplicate(universePos);
								model.Ship.Value.Position.Value = transitState.EndSystem.Position;

								model.Ship.Value.SetCurrentSystem(transitState.EndSystem);
								model.CelestialSystemState.Value = CelestialSystemStateBlock.UnSelect(model.CelestialSystemState.Value.System.Position.Value, model.CelestialSystemState.Value.System);

								SetGrid();
							}
							break;
					}

					model.GridInput.Value = new GridInputRequest(GridInputRequest.States.Active, GridInputRequest.Transforms.Animation);
					break;
				case TransitState.States.Complete:
					model.GridInput.Value = new GridInputRequest(GridInputRequest.States.Complete, GridInputRequest.Transforms.Animation);
					break;
			}
		}

		void OnDrawGizmos()
		{
#if UNITY_EDITOR

			//var uniOrigin = UniversePosition.Zero;
			//var oneLightYear = new UniversePosition(new Vector3(0.02f, 0f, 0.02f));

			//var localScale = model.GetScale(UniverseScales.Local);
			//var interstellarScale = model.GetScale(UniverseScales.Stellar);
			//var quadrantScale = model.GetScale(UniverseScales.Quadrant);
			//var galacticScale = model.GetScale(UniverseScales.Galactic);
			//var clusterScale = model.GetScale(UniverseScales.Cluster);

			//var activeScale = model.ActiveScale.Value;

			//Gizmos.color = Color.green;
			//Gizmos.DrawWireSphere(activeScale.Transform.Value.GetUnityPosition(model.Ship.Value.Position), 0.03f);
			//Gizmos.color = Color.yellow;
			//Gizmos.DrawWireSphere(activeScale.Transform.Value.GetUnityPosition(model.Galaxy.GalaxyOrigin), 0.06f);
			//Gizmos.color = Color.magenta;
			//Gizmos.DrawWireSphere(activeScale.Transform.Value.GetUnityPosition(model.Galaxy.ClusterOrigin), 0.08f);

			//Gizmos.color = Color.red;
			//Gizmos.DrawWireSphere(localScale.GetUnityPosition(uniOrigin), 0.02f);
			//Gizmos.color = Color.blue;
			//Gizmos.DrawWireSphere(localScale.GetUnityPosition(oneLightYear), 0.1f);
#endif
		}
		#endregion
	}
}