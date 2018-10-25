using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridPresenter : FocusPresenter<IGridView, SystemFocusDetails>
	{
		struct UnitMap
		{
			public float ZoomBegin;
			public float Units;

			public UnitMap(
				float zoomBegin,
				float units
			)
			{
				ZoomBegin = zoomBegin;
				Units = units;
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
				new UnitMap(0f, 1f),
				new UnitMap(1f, 10f),
				new UnitMap(2f, 10000f),
				new UnitMap(3f, 50000f),
				new UnitMap(4f, 750000f)
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

		//GridView.Grid[] GetGrids(float zoom)
		//{
		//	var result = new GridView.Grid[unitMaps.Length];
		//	for (var i = 0; i < unitMaps.Length; i++)
		//	{
		//		var curr = unitMaps[i];
		//		var grid = new GridView.Grid();
		//		grid.Alpha = curr.GetTransition(zoom);

		//	}
		//}

		void AnimateZoom(float scalar)
		{
			var zoomingUp = lastZoom < model.Zoom.Value;
			var result = new GridView.Grid[unitMaps.Length];
			for (var i = 0; i < unitMaps.Length; i++)
			{
				var curr = unitMaps[i];
				var grid = new GridView.Grid();
				//float expressed;
				//float multiplier;

				grid.IsTarget = Mathf.Approximately(curr.ZoomBegin, model.Zoom.Value);
				grid.IsActive = grid.IsTarget || (Mathf.Approximately(curr.ZoomBegin, lastZoom) && !Mathf.Approximately(scalar, 1f));
				//grid.IsActive = grid.IsTarget || Mathf.Approximately(curr.ZoomBegin, lastZoom);
				grid.Progress = scalar;
				grid.Tiling = 8f;

				grid.ZoomingUp = zoomingUp;

				//if (Mathf.Approximately(curr.ZoomBegin, model.Zoom.Value))
				//{
				//	// if we're zooming to us
				//	ReasonableUnits(curr.Units, out expressed, out multiplier);


				//}
				//else if (Mathf.Approximately(curr.ZoomBegin, lastZoom))
				//{
				//	// if we're zooming from us
				//	ReasonableUnits(curr.Units, out expressed, out multiplier);

				//	grid.Transition = scalar;
				//	grid.Tiling = 4f;
				//}

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
		//void OnUpdateZoomInfo(ZoomInfoBlock block)
		//{
		//	if (!ignoreNextZoom) model.Zoom.Value = block.Zoom;

		//	block.ScaleName = info.ScaleNames[Mathf.Min(info.ScaleNames.Length - 1, block.ScaleIndex)];

		//	switch (block.UnitType)
		//	{
		//		case GridUnitTypes.AstronomicalUnit: block.UnitName = info.AstronomicalUnit.Get(block.UnitAmountMinimum); break;
		//		case GridUnitTypes.LightYear: block.UnitName = info.LightYearUnit.Get(block.UnitAmountMinimum); break;
		//		default:
		//			Debug.LogError("Unrecognized grid unit: " + block.UnitType);
		//			break;
		//	}

		//	block.Position = model.ZoomInfo.Value.Position;
		//	block.PositionOffset = new UniversePosition(UniversePosition.ToUniverseDistance(block.LightYearOffset));

		//	block.UnitAmountFormatted = OnUnitAmountFormatted;

		//	model.ZoomInfo.Value = block;
		//}

		//string OnUnitAmountFormatted()
		//{
		//	var block = model.ZoomInfo.Value;

		//	var unitValue = block.UnitAmountMinimum;
		//	var suffix = string.Empty;

		//	if (block.UnitAmountMinimum < 1000f) suffix = string.Empty;
		//	else if (block.UnitAmountMinimum < 1000000)
		//	{
		//		suffix = info.ThousandUnit.Value.Value;
		//		unitValue /= 1000f;
		//	}
		//	else
		//	{
		//		suffix = info.MillionUnit.Value.Value;
		//		unitValue /= 1000000;
		//	}

		//	return ((int)unitValue) + suffix;
		//}

		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnCurrentScrollGesture(ScrollGesture gesture)
		{
			if (!View.Visible || !View.Highlighted) return;

			//View.UpdateZoom(model.Zoom, gesture.Current.y * gesture.TimeDelta);
		}

		void OnDragging(bool isDragging)
		{
			if (!isDragging)
			{
				var wasZoom = model.Zoom.Value;
				var newZoom = Mathf.Approximately(model.Zoom.Value, 1f) ? 2f : 1f;

				Debug.Log("Zooming " + ((wasZoom < newZoom) ? "Up" : "Down"));
				//Debug.Log("Going from "+wasZoom+" to "+newZoom);
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