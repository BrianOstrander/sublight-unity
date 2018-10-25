using UnityEngine;

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
			public float Units;
			public UniverseFocuses Focus;

			public UnitMap(
				float zoomBegin,
				float units,
				UniverseFocuses focus
			)
			{
				ZoomBegin = zoomBegin;
				Units = units;
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
				new UnitMap(0f, 1f, UniverseFocuses.Ship),
				new UnitMap(1f, 10f, UniverseFocuses.Ship),
				new UnitMap(2f, 10000f, UniverseFocuses.Ship),
				new UnitMap(3f, 50000f, UniverseFocuses.GalacticOrigin),
				new UnitMap(4f, 750000f, UniverseFocuses.ClusterOrigin)
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
			AnimateZoom(1f);
		}

		void AnimateZoom(float scalar)
		{
			var zoomingUp = lastZoom < model.Zoom.Value;
			var result = new GridView.Grid[unitMaps.Length];
			for (var i = 0; i < unitMaps.Length; i++)
			{
				var curr = unitMaps[i];
				var grid = new GridView.Grid();

				grid.IsTarget = Mathf.Approximately(curr.ZoomBegin, model.Zoom.Value);
				grid.IsActive = grid.IsTarget || (Mathf.Approximately(curr.ZoomBegin, lastZoom) && !Mathf.Approximately(scalar, 1f));
				grid.Progress = scalar;
				grid.Tiling = 8f;

				var alphaCurve = grid.IsTarget ? View.RevealScaleAlpha : View.HideScaleAlpha;

				grid.Alpha = alphaCurve.Evaluate(scalar);

				grid.ZoomingUp = zoomingUp;

				result[i] = grid;
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

				Debug.Log("Zooming " + ((wasZoom < newZoom) ? "Up" : "Down"));

				model.Zoom.Value = newZoom;
			}
		}

		void OnCurrentGesture(Gesture gesture)
		{
			if (!View.Visible || !isDragging) return;


		}
		#endregion
	}
}