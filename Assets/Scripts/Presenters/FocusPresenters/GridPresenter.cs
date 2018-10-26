using System;
using System.Linq;

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
			Ship = 10,
			GalacticOrigin = 20,
			ClusterOrigin = 30
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
			public UniverseFocuses Focus;

			public UnitMap(
				float zoomBegin,
				float lightYears,
				UniverseScales scale,
				UniverseFocuses focus
			)
			{
				ZoomBegin = zoomBegin;
				LightYears = lightYears;
				Scale = scale;
				Focus = focus;
			}
		}

		GameModel model;
		GridInfoBlock info;
		UnitMap[] unitMaps;

		TweenStates tweenState;

		float animationDuration;
		float animationRemaining;

		float scrollSignWhenLastAnimated;
		float scrollWhenLastAnimated;
		float scrollCooldownRemaining;

		public GridPresenter(
			GameModel model,
			GridInfoBlock info
		)
		{
			this.model = model;
			this.info = info;

			unitMaps = new UnitMap[]
			{
				new UnitMap(0f, 0.1f, UniverseScales.System, UniverseFocuses.Ship),
				new UnitMap(1f, 1f, UniverseScales.Local, UniverseFocuses.Ship),
				new UnitMap(2f, 10f, UniverseScales.Stellar, UniverseFocuses.Ship),
				new UnitMap(3f, 10000f, UniverseScales.Quadrant, UniverseFocuses.Ship),
				new UnitMap(4f, 50000f, UniverseScales.Galactic, UniverseFocuses.GalacticOrigin),
				new UnitMap(5f, 750000f, UniverseScales.Cluster, UniverseFocuses.ClusterOrigin)
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

		TweenBlock<float> AnimateZoom(float progress, TweenBlock<float> tween)
		{
			if (!tween.IsPingPong)
			{
				var result = new GridView.Grid[unitMaps.Length];

				for (var i = 0; i < unitMaps.Length; i++)
				{
					const float Tiling = 8f;

					var curr = unitMaps[i];
					var grid = new GridView.Grid();

					grid.ZoomingUp = tween.Transition == TweenTransitions.ToHigher;
					grid.IsTarget = Mathf.Approximately(curr.ZoomBegin, tween.End);
					grid.IsActive = grid.IsTarget || (Mathf.Approximately(curr.ZoomBegin, tween.Begin) && !Mathf.Approximately(progress, 1f));
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

					var alphaCurve = grid.IsTarget ? View.RevealScaleAlpha : View.HideScaleAlpha;

					grid.Alpha = alphaCurve.Evaluate(progress);

					result[i] = grid;

					var currLightYearsInTile = progress * curr.LightYears;

					var unityUnitsPerTile = (Tiling * 0.5f * tileScalar) / View.GridUnityWidth;
					var universeUnitsPerTile = UniversePosition.ToUniverseDistance(curr.LightYears);
					var universeUnitsPerUnityUnit = unityUnitsPerTile * universeUnitsPerTile;

					var scale = model.GetScale(curr.Scale);
					scale.Opacity.Value = grid.Alpha;
					scale.Transform.Value = new UniverseTransform(
						View.GridUnityOrigin,
						UniversePosition.Zero,
						Vector3.one * universeUnitsPerUnityUnit,
						Vector3.one * (1f / universeUnitsPerUnityUnit),
						Quaternion.identity
					);
				}

				View.Grids = result;
			}

			return tween.Duplicate(tween.Begin + ((tween.End - tween.Begin) * progress), progress);

			//model.FocusTransform.Value = new FocusTransform(
			//	TweenBlock.Create(fromZoom, toZoom, fromZoom + ((toZoom - fromZoom) * progress), progress),
			//	TweenBlock.Zero,
			//	fromScaleName,
			//	toScaleName,
			//	fromGetUnitCount,
			//	toGetUnitCount,
			//	fromUnitType,
			//	toUnitType
			//);

			//View.Grids = result;

			//if (Mathf.Approximately(1f, progress)) zoomRemaining = null;
		}

		#region
		void OnUpdate(float delta)
		{
			OnCheckTween(model.FocusTransform.Value, delta);
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

		void OnDragging(bool isDragging)
		{
			if (isDragging) return;
		}

		void OnCurrentGesture(Gesture gesture)
		{
			
		}

		void BeginZoom(TweenBlock<float> zoomTween, bool instant = false)
		{
			animationDuration = View.ZoomAnimationDuration;
			animationRemaining = animationDuration;

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
			animationDuration = View.NudgeAnimationDuration;
			animationRemaining = animationDuration;
			tweenState = TweenStates.Nudging;
			model.FocusTransform.Value = model.FocusTransform.Value.Duplicate(model.FocusTransform.Value.Zoom.DuplicateNoChange(), nudgeTween);
		}

		void OnDrawGizmos()
		{
#if UNITY_EDITOR

			var uniOrigin = UniversePosition.Zero;
			//var oneLightYear = new UniversePosition(new Vector3(1f, 0f, 1f) * UniversePosition.ToUniverseDistance(1f), Vector3.zero);
			var oneLightYear = new UniversePosition(new Vector3(0.02f, 0f, 0.02f));
			//var oneLightYear = new UniversePosition(new Vector3(1f, 0f, 1f));

			var localScale = model.GetScale(UniverseScales.Local).Transform.Value;
			var stellarScale = model.GetScale(UniverseScales.Stellar).Transform.Value;

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(localScale.GetUnityPosition(uniOrigin), 0.02f);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(localScale.GetUnityPosition(oneLightYear), 0.1f);

			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(stellarScale.GetUnityPosition(oneLightYear), 0.05f);

			Gizmos.color = Color.yellow;
			var testPos = localScale.UnityOrigin + new Vector3(0f, 0f, 1f);
			Gizmos.DrawWireSphere(testPos, 0.1f);
			Handles.Label(testPos + new Vector3(0f, 0f, 0.5f), localScale.GetUniversePosition(testPos).ToString());
#endif
		}
		#endregion
	}
}