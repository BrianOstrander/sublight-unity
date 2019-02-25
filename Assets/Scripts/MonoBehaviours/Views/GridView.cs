using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class GridView : View, IGridView
	{
		public struct Grid
		{
			public bool IsTarget;
			public bool IsActive;
			public bool ZoomingUp;
			public float Progress;
			public float Tiling;
			public float Alpha;
			public Vector2 Offset;
			public Vector3 RangeOrigin;
			public float RangeRadius;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float zoomAnimationDuration;
		[SerializeField]
		float nudgeAnimationDuration;
		[SerializeField]
		float zoomThreshold;
		[SerializeField]
		float nudgeThreshold;
		[SerializeField]
		float scrollCooldown;
		[SerializeField]
		AnimationCurve scrollCooldownFalloff;

		[SerializeField]
		AnimationCurve revealCurve;

		[SerializeField]
		float gridUnityRadius;

		[SerializeField]
		int renderQueue;
		[SerializeField]
		Material gridMaterial;
		[SerializeField]
		Material gridHazardMaterial;
		[SerializeField]
		Material gridBackgroundMaterial;
		[SerializeField]
		MeshRenderer gridMesh;
		[SerializeField]
		GameObject gridArea;
		[SerializeField]
		AnimationCurve hideScaleAlpha;
		[SerializeField]
		AnimationCurve revealScaleAlpha;
		[SerializeField]
		AnimationCurve positionZoomUpCurve;
		[SerializeField]
		AnimationCurve positionZoomDownCurve;
		[SerializeField]
		AnimationCurve zoomCurve;
		[SerializeField]
		AnimationCurve positionCenterCurve;

		[SerializeField]
		float gridDragRadius;

		[SerializeField]
		Transform followCameraArea;

		[SerializeField]
		float gridSelectDuration;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		Material[] gridBackgrounds;
		Material[] gridsHazards;
		Material[] grids;

		float fromShiftValue;
		float toShiftValue;
		float lastShiftValue;
		float? gridSelectRemaining;

		public float ZoomAnimationDuration { get { return zoomAnimationDuration; } }
		public float NudgeAnimationDuration { get { return nudgeAnimationDuration; } }
		public float ZoomThreshold { get { return zoomThreshold; } }
		public float NudgeThreshold { get { return nudgeThreshold; } }
		public float ScrollCooldown { get { return scrollCooldown; } }
		public AnimationCurve ScrollCooldownFalloff { get { return scrollCooldownFalloff; } }

		public Vector3 GridUnityOrigin { get { return gridMesh.transform.position; } }
		public float GridUnityRadius { get { return gridUnityRadius; } }
		public Action<bool> Dragging { set; private get; }
		public bool Highlighted { get; private set; }

		public AnimationCurve HideScaleAlpha { get { return hideScaleAlpha; } }
		public AnimationCurve RevealScaleAlpha { get { return revealScaleAlpha; } }
		public AnimationCurve GetPositionCurve(bool zoomingUp) { return zoomingUp ? positionZoomUpCurve : positionZoomDownCurve; }
		public AnimationCurve ZoomCurve { get { return zoomCurve; } }
		public AnimationCurve PositionCenterCurve { get { return positionCenterCurve; } }

		public Action DrawGizmos { set; private get; }

		public Grid[] Grids
		{
			set
			{
				if (value == null || value.Length == 0)
				{
					gridMesh.materials = new Material[0];
					gridBackgrounds = null;
					gridsHazards = null;
					grids = null;
					return;
				}
				if (grids == null || grids.Length == 0)
				{
					gridBackgrounds = new Material[value.Length];
					gridsHazards = new Material[value.Length];
					grids = new Material[value.Length];
					for (var i = 0; i < value.Length; i++)
					{
						gridBackgrounds[i] = new Material(gridBackgroundMaterial);
						gridsHazards[i] = new Material(gridHazardMaterial);
						grids[i] = new Material(gridMaterial);
					}
				}

				var activeGridBackgrounds = new List<Material>();
				var activeGridHazards = new List<Material>();
				var activeGrids = new List<Material>();

				for (var i = 0; i < value.Length; i++)
				{
					if (!value[i].IsActive) continue;

					var block = value[i];
					var background = gridBackgrounds[i];
					var hazard = gridsHazards[i];
					var grid = grids[i];

					background.renderQueue = renderQueue;
					background.SetVector(ShaderConstants.HoloGridBackgroundRange.RangeOrigin, block.RangeOrigin);
					background.SetFloat(ShaderConstants.HoloGridBackgroundRange.RangeRadius, block.RangeRadius);
					OnSetGridSelection(background, lastShiftValue);

					hazard.renderQueue = renderQueue + 1;
					hazard.SetVector(ShaderConstants.HoloGridRange.RangeOrigin, block.RangeOrigin);
					hazard.SetFloat(ShaderConstants.HoloGridRange.RangeRadius, block.RangeRadius);
					hazard.SetFloat(ShaderConstants.HoloGridRange.RadiusV, block.RangeRadius);
					OnSetGridSelection(hazard, lastShiftValue);

					grid.renderQueue = renderQueue + 2;
					grid.SetFloat(ShaderConstants.HoloGridBasic.Tiling, block.Tiling);
					grid.SetVector(ShaderConstants.HoloGridBasic.Offset, block.Offset);
					grid.SetFloat(ShaderConstants.HoloGridBasic.Alpha, block.Alpha);

					activeGridBackgrounds.Add(background);
					activeGridHazards.Add(hazard);
					activeGrids.Add(grid);
				}

				gridMesh.materials = activeGridBackgrounds.Concat(activeGridHazards).Concat(activeGrids).ToArray();
			}
		}

		public void SetGridSelected(bool value, bool instant = false)
		{
			var shiftValue = value ? 1f : 0f;

			if (gridBackgrounds == null)
			{
				fromShiftValue = shiftValue;
				toShiftValue = shiftValue;
				lastShiftValue = shiftValue;
				gridSelectRemaining = null;
				return;
			}

			fromShiftValue = toShiftValue;
			toShiftValue = shiftValue;
			lastShiftValue = fromShiftValue;
			gridSelectRemaining = gridSelectDuration;

			if (instant) OnUpdateGridSelection(gridSelectDuration);
		}

		void OnUpdateGridSelection(float delta)
		{
			if (!gridSelectRemaining.HasValue) return;

			gridSelectRemaining = Mathf.Max(0f, gridSelectRemaining.Value - delta);

			if (Mathf.Approximately(0f, gridSelectRemaining.Value))
			{
				lastShiftValue = toShiftValue;
				foreach (var background in gridBackgrounds) OnSetGridSelection(background, toShiftValue);
				foreach (var hazard in gridsHazards) OnSetGridSelection(hazard, lastShiftValue);
				gridSelectRemaining = null;
				return;
			}

			var scalar = 1f - (gridSelectRemaining.Value / gridSelectDuration);
			lastShiftValue = fromShiftValue + ((toShiftValue - fromShiftValue) * scalar);
			foreach (var background in gridBackgrounds) OnSetGridSelection(background, lastShiftValue);
			foreach (var hazard in gridsHazards) OnSetGridSelection(hazard, lastShiftValue);
		}

		void OnSetGridSelection(Material material, float value)
		{
			material.SetFloat(ShaderConstants.HoloGridBackgroundRange.RangeShifted, value);
		}

		public override void Reset()
		{
			base.Reset();

			Grids = null;
			Dragging = ActionExtensions.GetEmpty<bool>();
			Highlighted = false;
			SetRadius(0f, true);

			fromShiftValue = 0f;
			toShiftValue = 0f;
			lastShiftValue = 0f;
			gridSelectRemaining = null;

			DrawGizmos = ActionExtensions.Empty;
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			SetRadius(scalar, true);

			FollowCamera();
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			SetRadius(scalar, false);

			FollowCamera();
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			FollowCamera();

			OnUpdateGridSelection(delta);
		}

		void FollowCamera()
		{
			var dir = (followCameraArea.position - App.V.CameraForward).normalized;
			var lookTarget = followCameraArea.position + dir.NewY(0).normalized;
			followCameraArea.LookAt(lookTarget);
		}

		public void SetRadius(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;
			gridMesh.material.SetFloat(ShaderConstants.HoloGridBasic.Alpha, revealCurve.Evaluate(scalar));
		}

		public bool ProcessDrag(Vector2 viewport, out Vector3 unityPosition, out bool inRadius)
		{
			unityPosition = Vector3.zero;
			inRadius = false;

			var plane = new Plane(Vector3.up, transform.position);
			var ray = App.V.CameraViewportPointToRay(viewport);

			float distance;
			if (!plane.Raycast(ray, out distance)) return false;

			Debug.DrawLine(ray.origin, ray.origin + (ray.direction * distance), Color.red);


			unityPosition = ray.origin + (ray.direction * distance);
			inRadius = Vector3.Distance(unityPosition, GridUnityOrigin) <= gridDragRadius;
			return true;
		}

		#region Events
		public void OnEnter()
		{
			Highlighted = true;
		}

		public void OnExit()
		{
			Highlighted = false;
		}

		public void OnDragBegin()
		{
			Dragging(true);
		}

		public void OnDragEnd()
		{
			Dragging(false);
		}
		#endregion

		void OnDrawGizmosSelected()
		{
			if (DrawGizmos != null) DrawGizmos();
#if UNITY_EDITOR
			Handles.color = Color.red;
			Handles.DrawWireDisc(GridUnityOrigin, Vector3.up, gridDragRadius);
#endif
		}
	}

	public interface IGridView : IView
	{
		float ZoomAnimationDuration { get; }
		float NudgeAnimationDuration { get; }
		float ZoomThreshold { get; }
		float NudgeThreshold { get; }
		float ScrollCooldown { get; }
		AnimationCurve ScrollCooldownFalloff { get; }

		Vector3 GridUnityOrigin { get; }
		float GridUnityRadius { get; }
		bool Highlighted { get; }
		Action<bool> Dragging { set; }
		GridView.Grid[] Grids { set; }
		AnimationCurve HideScaleAlpha { get; }
		AnimationCurve RevealScaleAlpha { get; }
		AnimationCurve ZoomCurve { get; }
		AnimationCurve PositionCenterCurve { get; }

		AnimationCurve GetPositionCurve(bool zoomingUp);

		bool ProcessDrag(Vector2 viewport, out Vector3 unityPosition, out bool inRadius);
		void SetRadius(float scalar, bool showing);

		void SetGridSelected(bool value, bool instant = false);

		Action DrawGizmos { set; }
	}
}