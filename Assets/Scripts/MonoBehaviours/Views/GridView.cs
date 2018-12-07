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
		}

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
		float gridDragRadius;

		[SerializeField]
		Transform followCameraArea;

		Material gridBackground;
		Material[] grids;

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
		public Color HoloColor { set { if (gridBackground != null) gridBackground.SetColor(ShaderConstants.HoloGridBackground.Tint, value); } }

		public AnimationCurve HideScaleAlpha { get { return hideScaleAlpha; } }
		public AnimationCurve RevealScaleAlpha { get { return revealScaleAlpha; } }
		public AnimationCurve GetPositionCurve(bool zoomingUp) { return zoomingUp ? positionZoomUpCurve : positionZoomDownCurve; }
		public AnimationCurve ZoomCurve { get { return zoomCurve; } }

		public Action DrawGizmos { set; private get; }

		public Grid[] Grids
		{
			set
			{
				if (value == null || value.Length == 0)
				{
					gridMesh.materials = new Material[0];
					gridBackground = null;
					grids = null;
					return;
				}
				if (grids == null || grids.Length == 0)
				{
					gridBackground = new Material(gridBackgroundMaterial);
					grids = new Material[value.Length];
					for (var i = 0; i < value.Length; i++) grids[i] = new Material(gridMaterial);
				}

				var activeMaterials = new List<Material>();

				gridBackground.renderQueue = renderQueue;
				activeMaterials.Add(gridBackground);

				for (var i = 0; i < value.Length; i++)
				{
					if (!value[i].IsActive) continue;

					var block = value[i];
					var grid = grids[i];

					grid.renderQueue = renderQueue;
					grid.SetFloat(ShaderConstants.HoloGridBasic.Tiling, block.Tiling);
					grid.SetVector(ShaderConstants.HoloGridBasic.Offset, block.Offset);
					grid.SetFloat(ShaderConstants.HoloGridBasic.Alpha, block.Alpha);

					activeMaterials.Add(grid);
				}

				gridMesh.materials = activeMaterials.ToArray();
			}
		}

		public override void Reset()
		{
			base.Reset();

			Grids = null;
			Dragging = ActionExtensions.GetEmpty<bool>();
			Highlighted = false;
			SetRadius(0f, true);
			HoloColor = Color.white;

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

		public void ProcessDrag(Vector2 viewport, out Vector3 unityPosition, out bool inRadius)
		{
			unityPosition = Vector3.zero;
			inRadius = false;

			var plane = new Plane(Vector3.up, transform.position);
			var ray = App.V.CameraViewportPointToRay(viewport);

			float distance;
			if (!plane.Raycast(ray, out distance)) return;

			Debug.DrawLine(ray.origin, ray.origin + (ray.direction * distance), Color.red);


			unityPosition = ray.origin + (ray.direction * distance);
			inRadius = Vector3.Distance(unityPosition, GridUnityOrigin) <= gridDragRadius;
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

		void OnDrawGizmos()
		{
			if (DrawGizmos != null) DrawGizmos();
#if UNITY_EDITOR
			Handles.color = Color.red;
			Handles.DrawWireDisc(GridUnityOrigin, Vector3.up, gridDragRadius);
#endif
		}
	}

	public interface IGridView : IView, IHoloColorView
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

		AnimationCurve GetPositionCurve(bool zoomingUp);

		void ProcessDrag(Vector2 viewport, out Vector3 unityPosition, out bool inRadius);
		void SetRadius(float scalar, bool showing);

		Action DrawGizmos { set; }
	}
}