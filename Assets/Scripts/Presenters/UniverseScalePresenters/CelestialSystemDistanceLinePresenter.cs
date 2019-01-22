using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CelestialSystemDistanceLinePresenter : UniverseScalePresenter<ICelestialSystemDistanceLineView>
	{
		UniversePosition positionInUniverse;
		float lastOpacity;

		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public CelestialSystemDistanceLinePresenter(
			GameModel model,
			UniverseScales scale
		) : base(model, scale)
		{
			positionInUniverse = Model.Ship.Value.Position;

			Model.Ship.Value.Position.Changed += OnShipPosition;
			Model.CelestialSystemState.Changed += OnCelestialSystemState;

			ScaleModel.Transform.Changed += OnScaleTransform;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Ship.Value.Position.Changed -= OnShipPosition;
			Model.CelestialSystemState.Changed -= OnCelestialSystemState;

			ScaleModel.Transform.Changed -= OnScaleTransform;
		}

		void SetPoints(UniversePosition? end)
		{
			var noShow = !end.HasValue;

			lastOpacity = noShow ? 0f : 1f;

			View.SetOpacityStale();

			var transform = ScaleModel.Transform.Value;

			end = end ?? Model.Ship.Value.Position;

			var minOffset = new Vector3(0f, View.YMinimumOffset, 0f);
			var begin = transform.GetUnityPosition(Model.Ship.Value.Position) + minOffset;
			var endResult = transform.GetUnityPosition(end.Value) + minOffset;

			Vector3? clampedBegin = null;
			Vector3? clampedEnd = null;
			var clamping = ClampPoints(begin, endResult, out clampedBegin, out clampedEnd);

			View.SetPoints(
				begin,
				endResult,
				clamping,
				clampedBegin,
				clampedEnd,
				transform.GetUnityScale(Model.Ship.Value.Range.Value.Total)
			);
		}

		#region Utility
		LineClamping ClampPoints(Vector3 begin, Vector3 end, out Vector3? clampedBegin, out Vector3? clampedEnd)
		{
			// Incase you can't tell... I did not write this. Sourced frome here http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/

			clampedBegin = null;
			clampedEnd = null;

			var beginIsInRadius = GetPositionIsInRadius(begin, View.GridRadiusMargin);
			var endIsInRadius = GetPositionIsInRadius(end, View.GridRadiusMargin);

			if (beginIsInRadius && endIsInRadius) return LineClamping.NoClamping;

			var pointInside = beginIsInRadius ? begin : end;
			var pointOutside = beginIsInRadius ? end : begin;

			var radius = GridRadius - View.GridRadiusMargin;

			float xDelta, zDelta, A, B, C, det, t;

			xDelta = pointOutside.x - pointInside.x;
			zDelta = pointOutside.z - pointInside.z;

			A = xDelta * xDelta + zDelta * zDelta;
			B = 2f * (xDelta * (pointInside.x - GridOrigin.x) + zDelta * (pointInside.z - GridOrigin.z));
			C = (pointInside.x - GridOrigin.x) * (pointInside.x - GridOrigin.x) + (pointInside.z - GridOrigin.z) * (pointInside.z - GridOrigin.z) - radius * radius;

			det = B * B - 4f * A * C;
			if (Mathf.Approximately(A, 0f) || (det < 0f))
			{
				// No real solutions.
				return LineClamping.NotVisible;
			}

			if (Mathf.Approximately(det, 0f))
			{
				// We don't care about a single tangent...
				return LineClamping.NotVisible;
				/*
				// One solution.
				t = -B / (2f * A);
				var result = new Vector3(pointInside.x + t * xDelta, GridOrigin.y, pointInside.z + t * zDelta);
				var status = ClampedPointResults.NoClamping;

				if (beginIsInRadius)
				{
					clampedEnd = result;
					status = ClampedPointResults.EndClamped;
				}
				else 
				{
					clampedBegin = result;
					status = ClampedPointResults.BeginClamped;
				}
				return status;
				*/
			}

			// Two solutions.
			var doubleA = 2f * A;
			var sqrtDet = Mathf.Sqrt(det);

			t = (-B + sqrtDet) / doubleA;
			var result0 = new Vector3(pointInside.x + t * xDelta, GridOrigin.y, pointInside.z + t * zDelta);
			t = (-B - sqrtDet) / doubleA;
			var result1 = new Vector3(pointInside.x + t * xDelta, GridOrigin.y, pointInside.z + t * zDelta);

			if (beginIsInRadius)
			{
				clampedEnd = CalculateClamped(begin, end, result0);
				return LineClamping.EndClamped;
			}

			if (endIsInRadius)
			{
				clampedBegin = CalculateClamped(end, begin, result0);
				return LineClamping.BeginClamped;
			}

			var minPoint = begin.NewY(GridOrigin.y);
			var maxPoint = end.NewY(GridOrigin.y);

			if (Vector3.Distance(GridOrigin, maxPoint) < Vector3.Distance(GridOrigin, minPoint))
			{
				var newMax = minPoint;
				minPoint = maxPoint;
				maxPoint = newMax;
			}

			var midPointDelta = (maxPoint - minPoint) * 0.5f;

			var midPoint = minPoint + midPointDelta;

			if (Vector3.Distance(GridOrigin, minPoint) < Vector3.Distance(GridOrigin, midPoint))
			{
				// Line is outside the circle and doesn't intersect it.
				return LineClamping.NotVisible;
			}

			clampedBegin = CalculateClamped(begin, end, result0);
			clampedEnd = CalculateClamped(end, begin, result1);

			return LineClamping.BothClamped;
		}

		Vector3? CalculateClamped(Vector3 clampOrigin, Vector3 clampTermination, Vector3 position)
		{
			var originalDistance = Vector3.Distance(clampOrigin, clampTermination);
			var orginOnGrid = clampOrigin.NewY(GridOrigin.y);
			var terminationOnGrid = clampTermination.NewY(GridOrigin.y);
			var distanceOnGrid = Vector3.Distance(orginOnGrid, terminationOnGrid);

			var scalar = Vector3.Distance(orginOnGrid, position) / distanceOnGrid;
			return clampOrigin + ((clampTermination - clampOrigin).normalized * (originalDistance * scalar));
		}
		#endregion

		#region Events
		void OnShipPosition(UniversePosition position)
		{
			positionInUniverse = position;
		}

		protected override void OnShowView()
		{
			OnScaleTransformForced(ScaleModel.Transform.Value);
			View.PushOpacity(() => lastOpacity);
		}

		void OnScaleTransform(UniverseTransform transform)
		{
			if (!View.Visible) return;
			OnScaleTransformForced(transform);
		}

		void OnScaleTransformForced(UniverseTransform transform)
		{
			SetGrid(transform.UnityOrigin, transform.UnityRadius);

			UniversePosition? end = null;
			if (Model.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected)
			{
				switch (Model.CelestialSystemState.Value.State)
				{
					case CelestialSystemStateBlock.States.Highlighted:
						end = Model.CelestialSystemState.Value.Position;
						break;
					default:
						end = Model.CelestialSystemStateLastSelected.Value.Position;
						break;
				}
			}
			else
			{
				switch (Model.CelestialSystemState.Value.State)
				{
					case CelestialSystemStateBlock.States.Highlighted:
						end = Model.CelestialSystemState.Value.Position;
						break;
				}
			}

			SetPoints(end);
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			UniversePosition? end = null;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected: break;
				case CelestialSystemStateBlock.States.Highlighted:
					switch (Model.CelestialSystemStateLastSelected.Value.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							end = Model.CelestialSystemStateLastSelected.Value.Position;
							break;
						default:
							end = block.Position;
							break;
					}
					break;
				case CelestialSystemStateBlock.States.Selected:
					end = block.Position;
					break;
				case CelestialSystemStateBlock.States.Idle:
					switch (Model.CelestialSystemStateLastSelected.Value.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							end = Model.CelestialSystemStateLastSelected.Value.Position;
							break;
					}
					break;
			}

			SetPoints(end);
		}
		#endregion
	}
}