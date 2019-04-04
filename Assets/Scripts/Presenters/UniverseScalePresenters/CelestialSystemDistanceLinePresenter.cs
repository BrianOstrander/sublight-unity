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
			positionInUniverse = Model.Ship.Position;

			Model.Ship.Position.Changed += OnShipPosition;
			model.Context.TransitState.Changed += OnTransitState;
			Model.Context.CelestialSystemState.Changed += OnCelestialSystemState;

			ScaleModel.Transform.Changed += OnScaleTransform;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			Model.Ship.Position.Changed -= OnShipPosition;
			Model.Context.TransitState.Changed -= OnTransitState;
			Model.Context.CelestialSystemState.Changed -= OnCelestialSystemState;

			ScaleModel.Transform.Changed -= OnScaleTransform;
		}

		void SetPoints(UniversePosition begin, UniversePosition? end)
		{
			var noShow = !end.HasValue;

			lastOpacity = noShow ? 0f : 1f;

			View.SetOpacityStale();

			var transform = ScaleModel.Transform.Value;

			end = end ?? Model.Ship.Position;

			var minOffset = new Vector3(0f, View.YMinimumOffset, 0f);
			var beginResult = transform.GetUnityPosition(begin) + minOffset;
			var endResult = transform.GetUnityPosition(end.Value) + minOffset;

			Vector3? clampedBegin = null;
			Vector3? clampedEnd = null;
			var clamping = ClampPoints(beginResult, endResult, out clampedBegin, out clampedEnd);

			View.SetPoints(
				beginResult,
				endResult,
				clamping,
				clampedBegin,
				clampedEnd,
				transform.GetUnityScale(Model.KeyValues.Get(KeyDefines.Game.TransitRange))
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
			if (Model.Context.TransitState.Value.State == TransitState.States.Active) return;
			positionInUniverse = position;
		}

		void OnTransitState(TransitState transitState)
		{
			switch (transitState.State)
			{
				case TransitState.States.Active:
					positionInUniverse = transitState.BeginSystem.Position.Value;
					break;
				case TransitState.States.Complete:
					positionInUniverse = Model.Ship.Position.Value;
					SetPoints(Model.Ship.Position.Value, Model.Ship.Position.Value);
					ForceApplyScaleTransform();
					break;
			}
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

			UniversePosition begin = Model.Ship.Position;
			UniversePosition? end = null;

			if (Model.Context.TransitState.Value.State == TransitState.States.Active)
			{
				begin = Model.Context.TransitState.Value.BeginSystem.Position.Value;
				end = Model.Context.TransitState.Value.EndSystem.Position.Value;
			}
			else
			{
				if (Model.Context.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected)
				{
					switch (Model.Context.CelestialSystemState.Value.State)
					{
						case CelestialSystemStateBlock.States.Highlighted:
							end = Model.Context.CelestialSystemState.Value.Position;
							break;
						default:
							end = Model.Context.CelestialSystemStateLastSelected.Value.Position;
							break;
					}
				}
				else
				{
					switch (Model.Context.CelestialSystemState.Value.State)
					{
						case CelestialSystemStateBlock.States.Highlighted:
							end = Model.Context.CelestialSystemState.Value.Position;
							break;
					}
				}
			}

			SetPoints(begin, end);
		}

		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			UniversePosition? end = null;

			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected: break;
				case CelestialSystemStateBlock.States.Highlighted:
					switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							end = Model.Context.CelestialSystemStateLastSelected.Value.Position;
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
					switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							end = Model.Context.CelestialSystemStateLastSelected.Value.Position;
							break;
					}
					break;
			}

			SetPoints(Model.Ship.Position.Value, end);
		}
		#endregion
	}
}