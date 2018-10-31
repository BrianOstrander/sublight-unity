﻿using System;
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
			public float LightYears;
			public UniverseScales Scale;
			public UniverseFocuses FocusFromZoomDown;
			public UniverseFocuses FocusFromZoomUp;

			public UnitMap(
				float zoomBegin,
				float lightYears,
				UniverseScales scale,
				UniverseFocuses focusFromZoomDown,
				UniverseFocuses focusFromZoomUp
			)
			{
				ZoomBegin = zoomBegin;
				LightYears = lightYears;
				Scale = scale;
				FocusFromZoomDown = focusFromZoomDown;
				FocusFromZoomUp = focusFromZoomUp;
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
		UniverseTransform transformOnBeginDrag;

		public GridPresenter(
			GameModel model,
			GridInfoBlock info
		)
		{
			this.model = model;
			this.info = info;

			unitMaps = new UnitMap[]
			{
				new UnitMap(0f, 0.1f, UniverseScales.System, UniverseFocuses.Ship, UniverseFocuses.Ship),
				new UnitMap(1f, 1f, UniverseScales.Local, UniverseFocuses.Ship, UniverseFocuses.Ship),
				new UnitMap(2f, 10f, UniverseScales.Stellar, UniverseFocuses.Ship, UniverseFocuses.Ship),
				new UnitMap(3f, 10000f, UniverseScales.Quadrant, UniverseFocuses.Ship, UniverseFocuses.Ship),
				new UnitMap(4f, 50000f, UniverseScales.Galactic, UniverseFocuses.GalacticOrigin, UniverseFocuses.GalacticOrigin),
				new UnitMap(5f, 750000f, UniverseScales.Cluster, UniverseFocuses.ClusterOrigin, UniverseFocuses.None)
			};

			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture += OnCurrentScrollGesture;
			App.Callbacks.CurrentGesture += OnCurrentGesture;

			BeginZoom(model.FocusTransform.Value.Zoom, true);
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture -= OnCurrentScrollGesture;
			App.Callbacks.CurrentGesture -= OnCurrentGesture;
		}

		protected override void OnUpdateEnabled()
		{
			View.Dragging = OnDragging;
			View.DrawGizmos = OnDrawGizmos;
			BeginZoom(model.FocusTransform.Value.Zoom, true);
		}

		void SetGrid()
		{
			var activeScale = model.ActiveScale.Scale.Value;
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
			const float Tiling = 8f;

			var scale = model.GetScale(unitMap.Scale);
			var scaleTransform = scale.Transform.Value;

			if (isZooming)
			{
				var beginPosition = scaleTransformsOnBeginAnimation[scale.Scale.Value].UniverseOrigin;
				var endPosition = beginPosition;

				var focus = zoomingUp ? unitMap.FocusFromZoomDown : unitMap.FocusFromZoomUp;

				switch (focus)
				{
					case UniverseFocuses.None: break;
					case UniverseFocuses.Ship: endPosition = model.Ship.Value.Position.Value; break;
					case UniverseFocuses.GalacticOrigin: endPosition = model.Galaxy.GalaxyOrigin.Value; break;
					case UniverseFocuses.ClusterOrigin: endPosition = model.Galaxy.ClusterOrigin.Value; break;
					default:
						Debug.LogError("unrecognized focus: " + focus);
						break;
				}

				var currPosition = UniversePosition.Lerp(View.PositionCurve.Evaluate(progress), beginPosition, endPosition);
				scaleTransform = scale.Transform.Value.Duplicate(currPosition);
			}

			var grid = new GridView.Grid();
			grid.ZoomingUp = zoomingUp;
			grid.IsTarget = isTarget;
			grid.IsActive = isActive;
			grid.Progress = progress;

			var tileScalar = 1f;

			if (grid.IsTarget)
			{
				if (grid.ZoomingUp) tileScalar = 1f - (0.5f * (1f - grid.Progress));
				else tileScalar = 1f + (1f - grid.Progress);
			}
			else
			{
				if (grid.ZoomingUp) tileScalar = 1f + grid.Progress;
				else tileScalar = 1f - (0.5f * grid.Progress);
			}

			grid.Tiling = Tiling * tileScalar;

			var offset = scaleTransform.GetGridOffset(UniversePosition.ToUniverseDistance(unitMap.LightYears));
			grid.Offset = new Vector2(offset.x, offset.z);

			var alphaCurve = grid.IsTarget ? View.RevealScaleAlpha : View.HideScaleAlpha;

			grid.Alpha = grid.IsActive ? alphaCurve.Evaluate(progress) : 0f;

			var currLightYearsInTile = progress * unitMap.LightYears;

			var unityUnitsPerTile = (Tiling * 0.5f * tileScalar) / View.GridUnityWidth;
			var universeUnitsPerTile = UniversePosition.ToUniverseDistance(unitMap.LightYears);
			var universeUnitsPerUnityUnit = unityUnitsPerTile * universeUnitsPerTile;

			scale.Opacity.Value = grid.Alpha;
			scale.Transform.Value = new UniverseTransform(
				View.GridUnityOrigin,
				scaleTransform.UniverseOrigin,
				Vector3.one * universeUnitsPerUnityUnit,
				Vector3.one * (1f / universeUnitsPerUnityUnit),
				Quaternion.identity
			);

			return grid;
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

			if (Mathf.Approximately(0f, animationRemaining)) tweenState = TweenStates.Complete;

			model.FocusTransform.Value = transform.Duplicate(
				zoomTween,
				nudgeTween
			);
		}

		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
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
			
			fromScaleName = info.GetScaleName(fromGrid.ZoomBegin);
			toScaleName = info.GetScaleName(toGrid.ZoomBegin);
			
			info.GetUnitModels(fromGrid.LightYears, info.LightYearUnit, out fromGetUnitCount, out fromUnitType);
			info.GetUnitModels(toGrid.LightYears, info.LightYearUnit, out toGetUnitCount, out toUnitType);

			var transform = model.FocusTransform.Value.Duplicate(zoomTween, model.FocusTransform.Value.NudgeZoom.DuplicateNoChange());

			transform.SetLanguage(
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

			var origin = model.Ship.Value.Position.Value;
			var activeScale = model.ActiveScale;

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
					View.ProcessDrag(Gesture.GetViewport(lastgesture.Begin), out unityPosOnDragBegin, out inRadius);

					if (!inRadius)
					{
						isDragging = false;
						wasDragging = false;
						return;
					}
					transformOnBeginDrag = model.ActiveScale.Transform.Value;
				}
				else
				{
					// end drag logic
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
			View.ProcessDrag(Gesture.GetViewport(lastgesture.End), out currUnityDrag, out currInRadius);

			var offset = View.GridUnityOrigin + (unityPosOnDragBegin - currUnityDrag);
			var universePos = transformOnBeginDrag.GetUniversePosition(offset);

			model.ActiveScale.Transform.Value = model.ActiveScale.Transform.Value.Duplicate(universePos);

			SetGrid();
		}

		void OnCheckSecondaryDragging(float delta)
		{
			Debug.Log("lol secondary");
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

			var activeScale = model.ActiveScale;

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(activeScale.Transform.Value.GetUnityPosition(model.Ship.Value.Position), 0.03f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(activeScale.Transform.Value.GetUnityPosition(model.Galaxy.GalaxyOrigin), 0.06f);
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