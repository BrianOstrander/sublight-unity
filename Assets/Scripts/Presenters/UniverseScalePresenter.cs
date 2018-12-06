﻿using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public abstract class UniverseScalePresenter<V> : Presenter<V>
		where V : class, IUniverseScaleView
	{
		const float MinimumScale = 0.001f;

		protected virtual UniversePosition ScaleInUniverse { get { return new UniversePosition(Vector3.one); } }
		protected abstract UniversePosition PositionInUniverse { get; }
		protected virtual bool CanShow { get { return true; } }

		protected GameModel Model { get; private set; }
		protected UniverseScales Scale { get; private set; }
		protected UniverseScaleModel ScaleModel { get; private set; }

		protected Vector3 GridOrigin { private set; get; }
		protected float GridRadius { private set; get; }

		public UniverseScalePresenter(GameModel model, UniverseScales scale)
		{
			Model = model;
			Scale = scale;
			ScaleModel = model.GetScale(scale);

			ScaleModel.Transform.Changed += OnScaleTransform;
			ScaleModel.Opacity.Changed += OnOpacity;
		}

		protected override void OnUnBind()
		{
			ScaleModel.Transform.Changed -= OnScaleTransform;
			ScaleModel.Opacity.Changed -= OnOpacity;
		}

		void ApplyScaleTransform(UniverseTransform transform)
		{
			var scale = transform.GetUnityScale(ScaleInUniverse);
			scale = new Vector3(
				Mathf.Max(MinimumScale, scale.x),
				Mathf.Max(MinimumScale, scale.y),
				Mathf.Max(MinimumScale, scale.z)
			);
			var position = ScaleModel.Transform.Value.GetUnityPosition(PositionInUniverse);

			var rawScale = scale;
			switch (View.ScaleIgnores)
			{
				case UniverseScaleAxises.None: break;
				case UniverseScaleAxises.X: scale = scale.NewX(1f); break;
				case UniverseScaleAxises.Y: scale = scale.NewY(1f); break;
				case UniverseScaleAxises.Z: scale = scale.NewZ(1f); break;
				default:
					Debug.LogError("Unrecognized axis: " + View.ScaleIgnores);
					break;
			}

			View.SetScale(scale, rawScale);

			var rawPosition = position;
			switch (View.PositionIgnores)
			{
				case UniverseScaleAxises.None: break;
				case UniverseScaleAxises.X: position = position.NewX(View.transform.position.x); break;
				case UniverseScaleAxises.Y: position = position.NewY(View.transform.position.y); break;
				case UniverseScaleAxises.Z: position = position.NewZ(View.transform.position.z); break;
				default:
					Debug.LogError("Unrecognized axis: " + View.PositionIgnores);
					break;
			}

			var radiusNormal = GetRadiusNormal(position);
			var isInBounds = radiusNormal < 1f;
			var isInBoundsUnscaled = radiusNormal < scale.x;

			View.SetPosition(position, rawPosition, isInBounds, isInBoundsUnscaled);
		}

		protected void ShowViewInstant()
		{
			View.Reset();

			View.GetRadiusNormalCallback = GetRadiusNormal;
			View.GetPositionIsInRadiusCallback = GetPositionIsInRadius;

			OnShowView();

			ShowView(instant: true);
		}

		protected void CloseViewInstant()
		{
			OnCloseView();

			CloseView(true);
		}

		protected void ForceApplyScaleTransform()
		{
			ApplyScaleTransform(ScaleModel.Transform);
		}

		protected void SetGrid(Vector3 gridOrigin, float gridRadius)
		{
			GridOrigin = gridOrigin;
			GridRadius = gridRadius;

			View.SetGrid(gridOrigin, gridRadius);
		}

		protected float GetRadiusNormal(Vector3 worldPosition, float margin = 0f)
		{
			if (Mathf.Approximately(0f, GridRadius)) return 1f;
			worldPosition = worldPosition.NewY(GridOrigin.y);
			return Vector3.Distance(GridOrigin, worldPosition) / Mathf.Max(0f, GridRadius - margin);
		}

		protected bool GetPositionIsInRadius(Vector3 worldPosition, float margin = 0f)
		{
			return Vector3.Distance(GridOrigin, worldPosition.NewY(GridOrigin.y)) <= Mathf.Max(0f, GridRadius - margin);
		}

		#region Events
		void OnScaleTransform(UniverseTransform transform)
		{
			if (!View.Visible) return;
			ApplyScaleTransform(transform);
		}

		void OnOpacity(float opacity)
		{
			if (!CanShow) return;

			var isOpacityZero = Mathf.Approximately(0f, opacity);

			switch (View.TransitionState)
			{
				case TransitionStates.Closed:
					if (isOpacityZero) return;
					ShowViewInstant();
					//if (View.RestrictVisibiltyInBounds)
					//{

					//}
					//else ShowViewInstant();

					break;
				case TransitionStates.Shown:
					if (isOpacityZero) CloseViewInstant();
					break;
			}
			OnSetOpacity(opacity);
		}

		protected virtual void OnShowView() {}
		protected virtual void OnCloseView() {}
		protected virtual void OnSetOpacity(float opacity) {}
		#endregion
	}
}