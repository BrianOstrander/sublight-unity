using System;

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

		TweenStates lastZoomState;
		UniverseTransform? updatedTransform;

		public UniverseScalePresenter(GameModel model, UniverseScales scale)
		{
			Model = model;
			Scale = scale;
			ScaleModel = model.Context.GetScale(scale);

			Model.Context.FocusTransform.Changed += OnFocusTransform;
			ScaleModel.Transform.Changed += OnScaleTransform;
			ScaleModel.Opacity.Changed += OnOpacity;

			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
			App.Heartbeat.LateUpdate += OnLateUpdate;
		}

		protected override void OnUnBind()
		{
			Model.Context.FocusTransform.Changed -= OnFocusTransform;
			ScaleModel.Transform.Changed -= OnScaleTransform;
			ScaleModel.Opacity.Changed -= OnOpacity;

			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
			App.Heartbeat.LateUpdate -= OnLateUpdate;
		}

		void ApplyScaleTransform(UniverseTransform transform)
		{
			updatedTransform = null;
			var scale = transform.GetUnityScale(ScaleInUniverse);
			scale = new Vector3(
				Mathf.Max(MinimumScale, scale.x),
				Mathf.Max(MinimumScale, scale.y),
				Mathf.Max(MinimumScale, scale.z)
			);

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

			var position = transform.GetUnityPosition(PositionInUniverse);

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

			View.SetPosition(position, rawPosition, isInBounds, radiusNormal);
		}

		protected void ShowViewInstant(bool onlyReset = false)
		{
			View.Reset();

			View.GetRadiusNormalCallback = GetRadiusNormal;
			View.GetPositionIsInRadiusCallback = GetPositionIsInRadius;
			View.PushOpacity(() => ScaleModel.Opacity.Value);

			SetGrid(ScaleModel.Transform.Value.UnityOrigin, ScaleModel.Transform.Value.UnityRadius);

			OnShowView();

			if (!onlyReset) ShowView(instant: true);
			ForceApplyScaleTransform();
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
			if (Mathf.Approximately(0f, GridRadius))
			{
				Debug.LogError("GridRadius is zero, unexpected behaviour may occur");
				return 1f;
			}
			worldPosition = worldPosition.NewY(GridOrigin.y);
			return Vector3.Distance(GridOrigin, worldPosition) / Mathf.Max(0f, GridRadius - margin);
		}

		protected bool GetPositionIsInRadius(Vector3 worldPosition, float margin = 0f)
		{
			return Vector3.Distance(GridOrigin, worldPosition.NewY(GridOrigin.y)) <= Mathf.Max(0f, GridRadius - margin);
		}

		void TransitionActive(TransitionFocusRequest request, SetFocusTransition transition)
		{
			if (!transition.End.Enabled && request.LastActive)
			{
				lastZoomState = TweenStates.Unknown;
				if (View.TransitionState == TransitionStates.Shown) CloseViewInstant();
			}
		}

		#region Events
		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			switch (request.State)
			{
				case TransitionFocusRequest.States.Active:
					break;
				default:
					return;
			}
			SetFocusTransition transition;
			if (request.GetTransition(SetFocusLayers.System, out transition)) TransitionActive(request, transition);
		}

		void OnScaleTransform(UniverseTransform transform)
		{
			if (!View.Visible) return;
			updatedTransform = transform;
		}

		void OnFocusTransform(FocusTransform focusTransform)
		{
			if (focusTransform.Zoom.State == lastZoomState) return;
			var wasUnknown = lastZoomState == TweenStates.Unknown;
			lastZoomState = focusTransform.Zoom.State;

			if (focusTransform.ToScale == Scale) OnFocusTransformShowing(focusTransform, wasUnknown);
			else if (focusTransform.FromScale == Scale) OnFocusTransformClosing(focusTransform);
		}

		void OnFocusTransformShowing(FocusTransform focusTransform, bool wasUnknown)
		{
			if (focusTransform.Zoom.State == TweenStates.Complete && !wasUnknown) return;
			if (!CanShow) return;
			if (View.Visible) return;

			if (View.RestrictVisibiltyInBounds)
			{
				if (focusTransform.Zoom.State == TweenStates.Active)
				{
					if (1f <= GetRadiusNormal(ScaleModel.TransformDefault.Value.GetUnityPosition(PositionInUniverse))) return;
				}
			}
			ShowViewInstant();
		}

		void OnFocusTransformClosing(FocusTransform focusTransform)
		{
			if (focusTransform.Zoom.State == TweenStates.Active) return;
			CloseViewInstant();
		}

		void OnLateUpdate(float delta)
		{
			if (View.Visible && updatedTransform.HasValue) ApplyScaleTransform(updatedTransform.Value);
		}

		protected virtual void OnShowView() {}
		protected virtual void OnCloseView() {}
		protected virtual void OnOpacity(float opacity)
		{
			View.SetOpacityStale();
		}
		#endregion
	}
}