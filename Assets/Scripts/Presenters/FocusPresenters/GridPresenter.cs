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

		bool isDragging;

		float lastZoom;
		float zoomDuration;
		float? zoomRemaining;

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
			model.Zoom.Changed += OnZoom;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture -= OnCurrentScrollGesture;
			App.Callbacks.CurrentGesture -= OnCurrentGesture;
			model.Zoom.Changed -= OnZoom;
		}

		protected override void OnUpdateEnabled()
		{
			View.Dragging = OnDragging;
			View.DrawGizmos = OnDrawGizmos;
			AnimateZoom(1f);
		}

		void AnimateZoom(float scalar)
		{
			var zoomingUp = lastZoom < model.Zoom.Value;
			var result = new GridView.Grid[unitMaps.Length];
			for (var i = 0; i < unitMaps.Length; i++)
			{
				const float Tiling = 8f;

				var curr = unitMaps[i];
				var grid = new GridView.Grid();

				grid.ZoomingUp = zoomingUp;
				grid.IsTarget = Mathf.Approximately(curr.ZoomBegin, model.Zoom.Value);
				grid.IsActive = grid.IsTarget || (Mathf.Approximately(curr.ZoomBegin, lastZoom) && !Mathf.Approximately(scalar, 1f));
				grid.Progress = scalar;

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

				grid.Alpha = alphaCurve.Evaluate(scalar);

				result[i] = grid;

				var currLightYearsInTile = scalar * curr.LightYears;

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

			if (Mathf.Approximately(1f, scalar))
			{
				lastZoom = model.Zoom.Value;
				zoomRemaining = null;
			}
		}

		void ReasonableUnits(float units, out float expressed, out float multiplier)
		{
			expressed = units;
			multiplier = 1f;

			if (1000000f <= units)
			{
				expressed = units / 1000000;
				multiplier = 1000000;
				return;
			}
			if (1000 <= units)
			{
				expressed = units / 1000;
				multiplier = 1000;
			}
		}

		#region
		void OnUpdate(float delta)
		{
			if (!zoomRemaining.HasValue) return;

			zoomRemaining = Mathf.Max(0f, zoomRemaining.Value - delta);
			AnimateZoom((zoomDuration - zoomRemaining.Value) / zoomDuration);
		}

		void OnZoom(float zoom)
		{
			if (!View.Visible) return;
			zoomDuration = 2.5f;
			zoomRemaining = zoomDuration;
		}

		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnCurrentScrollGesture(ScrollGesture gesture)
		{
			if (!View.Visible || !View.Highlighted) return;

		}

		void OnDragging(bool isDragging)
		{
			if (!isDragging)
			{
				var wasZoom = model.Zoom.Value;
				var newZoom = Mathf.Approximately(model.Zoom.Value, 1f) ? 2f : 1f;
				var name = Mathf.Approximately(newZoom, 1f) ? "Local" : "Stellar";
				Debug.Log("Zooming " + ((wasZoom < newZoom) ? "Up" : "Down")+" to "+name);

				model.Zoom.Value = newZoom;
			}
		}

		void OnCurrentGesture(Gesture gesture)
		{
			if (!View.Visible || !isDragging) return;


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